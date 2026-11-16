using System;
using System.IO;
using System.Reflection;
using Modding;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Uses the actual native lever, statue animator and inspection interaction.
    // Only the alternate-selection bit is virtual; the save's real Gruz Mother
    // completion data is never replaced with gauntlet progress.
    public sealed class GauntletPodium : MonoBehaviour
    {
        private static readonly FieldInfo ToggleField = typeof(BossStatue).GetField("dreamToggle", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo PendingSceneField = typeof(BossStatueLoadManager).GetField("currentBossScene", BindingFlags.NonPublic | BindingFlags.Static);
        internal bool OwnsPendingChallenge => gauntletScene != null && ReferenceEquals(PendingSceneField?.GetValue(null), gauntletScene);
        private BossStatue statue;
        private FieldInfo stateField;
        private BossStatueLever lever;
        private BossScene priorDreamScene, gauntletScene;
        private BossStatue.BossUIDetails priorDetails;
        private string priorDreamKey, stateKey;
        private bool priorUnlock, priorLeverActive, selected, swapping, disposed;
        private object priorToggle;
        private Action<bool> changed;
        private Texture2D art;
        private Sprite idol;
        private GameObject idolObject;
        private bool attached, initializedPlaques;
        private Vector3 originalPosition, alternatePosition;
        public bool Selected => selected;

        internal void Configure(BossStatue target, bool initialSelection, Action<bool> onChanged, bool nativeStartPending = true)
        {
            statue = target; selected = initialSelection; changed = onChanged;
            stateKey = statue.statueStatePD;
            stateField = typeof(PlayerData).GetField(stateKey);
            if (string.IsNullOrEmpty(stateKey) || stateField == null || ToggleField == null || PendingSceneField == null || statue.statueDisplayAlt == null)
                throw new InvalidOperationException("Gruz Mother statue has no native alternate display/toggle binding.");
            var leverRoot = statue.transform.Find("alt_lever");
            lever = leverRoot != null ? leverRoot.GetComponentInChildren<BossStatueLever>(true) : null;
            if (lever == null) throw new InvalidOperationException("Gruz Mother's native lever was not found.");
            priorDreamScene = statue.dreamBossScene; priorDreamKey = statue.dreamStatueStatePD;
            priorDetails = statue.dreamBossDetails; priorUnlock = statue.isAlwaysUnlockedDream;
            originalPosition = statue.statueDisplay.transform.localPosition;
            alternatePosition = statue.statueDisplayAlt.transform.localPosition;
            priorToggle = ToggleField.GetValue(statue); priorLeverActive = leverRoot.gameObject.activeSelf;
            gauntletScene = Instantiate(statue.bossScene);
            gauntletScene.name = "Gauntlet.SoulTyrant";
            gauntletScene.sceneName = ArenaController.ArenaScene;
            gauntletScene.requireUnlock = false;
            foreach (string field in new[] { "tier1Scene", "tier2Scene", "tier3Scene" })
                typeof(BossScene).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gauntletScene, null);
            CreateIdol();
            ModHooks.GetPlayerVariableHook += ReadState;
            ModHooks.SetPlayerVariableHook += WriteState;
            ModHooks.LanguageGetHook += Translate;
            attached = true;
            statue.dreamBossScene = gauntletScene;
            statue.dreamStatueStatePD = stateKey;
            statue.dreamBossDetails = new BossStatue.BossUIDetails
            { nameKey = "GAUNTLET_NAME", nameSheet = "Gauntlet", descriptionKey = "GAUNTLET_DESC", descriptionSheet = "Gauntlet" };
            statue.isAlwaysUnlockedDream = true;
            ToggleField.SetValue(statue, lever);
            // Configured from sceneLoaded before BossStatue.Start: the native
            // Start binds SetOwner exactly once and enables the unlocked lever.
            leverRoot.gameObject.SetActive(true);
            statue.OnStatueSwapFinished += SwapFinished;
            changed?.Invoke(selected);
            if (!nativeStartPending) { lever.SetOwner(statue); lever.SetState(true); }
        }
        private void CreateIdol()
        {
            using (var input = typeof(GauntletPodium).Assembly.GetManifestResourceStream("Gauntlet.idol.png"))
            using (var bytes = new MemoryStream())
            {
                if (input == null) throw new InvalidOperationException("Gauntlet idol artwork missing.");
                input.CopyTo(bytes); art = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!art.LoadImage(bytes.ToArray())) throw new InvalidOperationException("Cannot decode gauntlet idol.");
            }
            art.filterMode = FilterMode.Bilinear;
            // A bottom pivot places the sculpture on the existing stone plinth.
            idol = Sprite.Create(art, new Rect(0, 0, art.width, art.height), new Vector2(0.5f, 0.05f), art.height / 4.8f);
            idolObject = new GameObject("Gauntlet.Idol");
            idolObject.transform.SetParent(statue.statueDisplayAlt.transform, false);
            idolObject.transform.localPosition = new Vector3(0, -0.4f, 1.5f);
            var renderer = idolObject.AddComponent<SpriteRenderer>();
            renderer.sprite = idol;
            var originalRenderer = statue.statueDisplay.GetComponentInChildren<SpriteRenderer>(true);
            if (originalRenderer != null)
            {
                // Gruz's sculpture is at local z=2.29, behind its plinth at
                // z=1.96. Match that depth as the alternate display slides up.
                var position = idolObject.transform.localPosition;
                position.z = statue.statueDisplay.transform.InverseTransformPoint(originalRenderer.transform.position).z;
                idolObject.transform.localPosition = position;
                idolObject.layer = originalRenderer.gameObject.layer;
                renderer.sortingLayerID = originalRenderer.sortingLayerID;
                renderer.sortingOrder = originalRenderer.sortingOrder;
                renderer.maskInteraction = originalRenderer.maskInteraction;
                // The native statue uses Sprites-Diffuse Unity. Sharing that
                // material gives the idol the same Hall lighting and shadow.
                renderer.sharedMaterial = originalRenderer.sharedMaterial;
                // Gruz's native artwork is itself a dark silhouette. Its 90th
                // percentile luminance is ~44/255 vs ~180/255 in our stone art.
                // Match those highlights without changing the embedded image.
                renderer.color = originalRenderer.color * new Color(0.25f, 0.25f, 0.25f, 1f);
            }
        }
        private object ReadState(Type type, string name, object value)
        {
            if (disposed || name != stateKey || !(value is BossStatue.Completion completion)) return value;
            if (selected)
            {
                completion = BossStatue.Completion.None;
                completion.isUnlocked = true; completion.hasBeenSeen = true;
            }
            completion.usingAltVersion = selected;
            return completion;
        }
        private object WriteState(Type type, string name, object value)
        {
            if (disposed || name != stateKey || !(value is BossStatue.Completion completion)) return value;
            bool requested = completion.usingAltVersion;
            // Read the actual field, bypassing our virtual getter. Preserve its
            // original selector bit and all genuine completion changes.
            var actual = (BossStatue.Completion)stateField.GetValue(PlayerData.instance);
            bool wasSelected = selected;
            completion.usingAltVersion = actual.usingAltVersion;
            if (selected != requested)
            { selected = requested; swapping = true; changed?.Invoke(selected); }
            // Alternate-mode unlock/seen/tier writes are virtual too. A native
            // "seen statue" check must not unlock or complete the normal boss.
            return wasSelected || requested ? actual : completion;
        }
        private string Translate(string key, string sheet, string original)
        {
            if (sheet != "Gauntlet") return original;
            if (key == "GAUNTLET_NAME") return "THE HOLLOW TRIAL";
            if (key == "GAUNTLET_DESC") return "A gauntlet of foes. A Knight forged anew.";
            return original;
        }
        internal bool Toggle()
        {
            if (disposed || statue == null || swapping || GameManager.instance == null || GameManager.instance.isPaused) return false;
            statue.SetDreamVersion(!selected, true, true);
            return true;
        }
        private void SwapFinished()
        {
            swapping = false;
            if (statue == null) return;
            // Refresh the shared plinth's existing Hall completion badge.
            UpdatePlaques();
        }
        private void UpdatePlaques()
        {
            if (statue == null) return;
            // Use the same real completion data as the Hall's entrance list.
            // The selector's virtual None value must not hide the shared badge.
            var actual = (BossStatue.Completion)stateField.GetValue(PlayerData.instance);
            statue.regularPlaque.gameObject.SetActive(actual.isUnlocked || actual.completedTier1 || actual.completedTier2 || actual.completedTier3);
            statue.altPlaqueL.gameObject.SetActive(false);
            statue.altPlaqueR.gameObject.SetActive(false);
            statue.regularPlaque.SetDisplay(BossStatueTrophyPlaque.GetDisplayType(actual));
        }
        private void LateUpdate()
        {
            if (disposed || initializedPlaques) return;
            initializedPlaques = true; UpdatePlaques();
        }
        internal void Shutdown()
        {
            if (disposed) return;
            if (statue != null && attached)
            {
                // Restore normal presentation while the virtual selector is
                // still available, then detach the hooks and alternate scene.
                selected = false;
                statue.StopAllCoroutines();
                statue.SetDreamVersion(false, false, false);
                statue.statueDisplay.SetActive(statue.StatueState.isUnlocked);
                statue.statueDisplayAlt.SetActive(false);
                statue.statueDisplay.transform.localPosition = originalPosition;
                statue.statueDisplayAlt.transform.localPosition = alternatePosition;
                foreach (var display in new[] { statue.statueDisplay, statue.statueDisplayAlt })
                {
                    var animator = display.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;
                }
                if (statue.bossUIControlFSM != null)
                    FSMUtility.SendEventToGameObject(statue.bossUIControlFSM.gameObject, "NPC CONTROL ON", false);
                statue.dreamBossScene = priorDreamScene;
                statue.dreamStatueStatePD = priorDreamKey;
                statue.dreamBossDetails = priorDetails;
                statue.isAlwaysUnlockedDream = priorUnlock;
                ToggleField.SetValue(statue, priorToggle);
                var root = statue.transform.Find("alt_lever");
                if (root != null) root.gameObject.SetActive(priorLeverActive);
                statue.OnStatueSwapFinished -= SwapFinished;
            }
            disposed = true;
            ModHooks.GetPlayerVariableHook -= ReadState;
            ModHooks.SetPlayerVariableHook -= WriteState;
            ModHooks.LanguageGetHook -= Translate;
            if (idolObject != null) Destroy(idolObject);
            if (idol != null) Destroy(idol);
            if (art != null) Destroy(art);
            if (gauntletScene != null) Destroy(gauntletScene);
        }
        private void OnDestroy() => Shutdown();
    }
}
