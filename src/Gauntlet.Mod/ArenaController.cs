using System;
using System.Collections.Generic;
using System.Linq;
using Gauntlet.Core;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UScene = UnityEngine.SceneManagement.Scene;

namespace Gauntlet.Mod
{
    public sealed class ArenaController : MonoBehaviour
    {
        public Run Run { get; } = new Run();
        private readonly TrialCountdown countdown = new TrialCountdown();
        private readonly RewardTransition reward = new RewardTransition();
        private readonly VictoryTransition victory = new VictoryTransition();
        internal bool UpdatingLoadoutHealth => loadout != null && loadout.Active && loadout.Refreshing;
        internal bool ProtectingKnight => ownsRoom && (InventoryOpen || Run.Phase == RunPhase.ChoosingCard || Run.Phase == RunPhase.Victory);
        internal const string HallScene = "GG_Workshop";
        internal const string ArenaScene = "GG_Soul_Tyrant";
        private readonly List<Enemy> enemies = new List<Enemy>();
        private SoulTyrantArena stage;
        private ArenaLayouts layouts;
        private readonly ArenaCeiling ceiling = new ArenaCeiling();
        private readonly ArenaRoofArt roofArt = new ArenaRoofArt();
        private readonly WaveAnnouncement waveNotice = new WaveAnnouncement();
        private Dictionary<EnemyKind, GameObject> templates;
        private Action<string> report;
        private HeroController controlledHero;
        private CardPresentation cards;
        private RunLoadout loadout;
        private GauntletPodium podium;
        private bool gauntletSelected;
        private int selected, nextSeed = 1;
        private bool InventoryOpen => ownsRoom && Run.IsActive &&
            GameManager.instance?.inventoryFSM?.FsmVariables.FindFsmBool("Open")?.Value == true;
        private sealed class Arrival
        {
            public EncounterEnemy Definition;
            public int Index, Wave;
            public long Token;
        }
        private readonly Queue<Arrival> arrivals = new Queue<Arrival>();
        private GameObject bell;
        private Dictionary<PlatformKind, GameObject> stageTemplates;
        private float nextArrivalAt, warningAt, roomReadyAt, resetAt = -10f, nextCharmScanAt;
        private float left, right, floor;
        private Vector3? warningPoint;
        private ArrivalMarker marker;
        private GameObject arrivalPuff;
        private InventoryPauseUi inventoryUi;
        private readonly BlueHealthHud blueHealthHud = new BlueHealthHud();
        private readonly BenchlessCharms charmMenu = new BenchlessCharms();
        private readonly CardFreeze cardFreeze = new CardFreeze();
        private readonly HashSet<ParticleSystemRenderer> hiddenCardParticles = new HashSet<ParticleSystemRenderer>();
        private readonly TrialHud hud = new TrialHud();
        private long spawnedToken = -1;
        private bool shuttingDown, armed, takingOver, ownsRoom, boundsReady;
        private string message = "Strike the lever beside Gruz Mother to select The Hollow Trial. F6 also toggles it.";
        private sealed class Enemy
        {
            public GameObject Object;
            public HealthManager Health;
            public GameObject Corpse;
            public HealthManager.DeathEvent Callback;
            public bool Killed;
        }
        internal void Configure(Dictionary<EnemyKind, GameObject> prefabs, Dictionary<PlatformKind, GameObject> platforms, Action<string> log)
        {
            stageTemplates = platforms; templates = prefabs; layouts = new ArenaLayouts(platforms); report = log; loadout = new RunLoadout(HealthHud.Refresh); cards = new CardPresentation();
            inventoryUi = new InventoryPauseUi(() => ownsRoom && (InventoryOpen || Run.Phase == RunPhase.ChoosingCard) && !GameManager.instance.isPaused);
            arrivalPuff = ArrivalMarker.Resolve(prefabs[EnemyKind.GruzMother]);
            ModHooks.BeforeSceneLoadHook += BeforeSceneLoad;
            UnitySceneManager.sceneLoaded += SceneLoaded;
            UnitySceneManager.activeSceneChanged += SceneChanged;
            if (UnitySceneManager.GetActiveScene().name == HallScene) InstallPodium(false);
        }
        private string BeforeSceneLoad(string scene)
        {
            if (scene != ArenaScene) loadout?.Restore();
            takingOver = armed && scene == ArenaScene && podium != null && podium.OwnsPendingChallenge && !BossSequenceController.IsInSequence;
            if (!takingOver && scene != HallScene) armed = false;
            return scene;
        }
        private void SceneLoaded(UScene scene, LoadSceneMode mode)
        {
            if (scene.name == HallScene) { InstallPodium(true); return; }
            if (!takingOver || scene.name != ArenaScene) return;
            try
            {
                takingOver = false; armed = false; ownsRoom = true; boundsReady = false;
                stage = new SoulTyrantArena();
                stage.Claim(scene.GetRootGameObjects());
                roomReadyAt = Time.unscaledTime;
                countdown.Queue();
                message = "Waiting for dream arrival, then the trial starts in 3 seconds. F7 returns to the hall.";
                report?.Invoke("Soul Tyrant arena claimed for gauntlet; both boss phases suppressed, native scenery and music retained.");
            }
            catch (Exception ex)
            {
                report?.Invoke(ex.ToString());
                stage?.Dispose(); stage = null; roofArt.Dispose(); ceiling.Dispose();
                ownsRoom = false; message = "Arena takeover failed; normal challenge restored.";
            }
        }
        private void InstallPodium(bool nativeStartPending)
        {
            if (podium != null) return;
            var target = GameObject.Find("GG_Statue_Gruz")?.GetComponent<BossStatue>();
            if (target == null) { report?.Invoke("Gruz statue unavailable; F6 fallback remains active."); return; }
            podium = target.gameObject.AddComponent<GauntletPodium>();
            try
            {
                podium.Configure(target, gauntletSelected, PodiumModeChanged, nativeStartPending);
                report?.Invoke("Gruz gauntlet lever and alternate idol installed.");
            }
            catch (Exception error)
            {
                report?.Invoke("Podium setup failed; F6 fallback remains active. " + error);
                podium.Shutdown(); Destroy(podium); podium = null;
            }
        }
        private void PodiumModeChanged(bool selectedMode)
        {
            gauntletSelected = selectedMode; armed = selectedMode;
            message = selectedMode ? "The Hollow Trial selected. Challenge the idol on Attuned; return here to retry."
                : "Gruz Mother selected. Strike the lever beside her podium to reveal The Hollow Trial.";
        }
        private void SceneChanged(UScene from, UScene to)
        {
            loadout?.Restore();
            charmMenu.Restore(); inventoryUi?.Restore(); blueHealthHud.Restore(); RestoreCardTime(); controlledHero = null; reward.Cancel(); victory.Cancel(); layouts?.Clear(); ClearBell();
            if (Run.IsActive) Run.Abandon();
            CleanupEnemies();
            if (to.name != ArenaScene)
            { ownsRoom = false; boundsReady = false; stage?.Dispose(); stage = null; roofArt.Dispose(); ceiling.Dispose(); countdown.Cancel(); }
            if (to.name != HallScene && to.name != ArenaScene) armed = false;
        }
        private void Update()
        {
            if (shuttingDown) return;
            try { Tick(); }
            catch (Exception ex)
            {
                report?.Invoke(ex.ToString());
                Abort("Trial stopped after an error. F7 returns to the hall; details in ModLog.txt.");
            }
        }
        private bool Ready => ownsRoom && Time.unscaledTime >= roomReadyAt &&
            HeroController.instance != null && PlayerData.instance != null && PlayerData.instance.health > 0 &&
            GameManager.instance != null && !GameManager.instance.isPaused &&
            (BossSceneController.Instance == null || BossSceneController.Instance.HasTransitionedIn);
        private void Tick()
        {
            if (!Run.IsActive) waveNotice.Cancel();
            waveNotice.Advance(Time.unscaledDeltaTime, Run.Phase != RunPhase.Combat || InventoryOpen
                || GameManager.instance == null || GameManager.instance.isPaused || Time.time - resetAt < 1.5f);
            if (ownsRoom && Run.IsActive)
            {
                if (Time.unscaledTime >= nextCharmScanAt)
                {
                    PrepareUi();
                    charmMenu.Apply(Resources.FindObjectsOfTypeAll<PlayMakerFSM>()
                        .Where(f => f != null && f.gameObject.scene.IsValid()));
                    nextCharmScanAt = Time.unscaledTime + 0.25f;
                }
            }
            else { charmMenu.Restore(); inventoryUi.Restore(); if (!loadout.Active) blueHealthHud.Restore(); nextCharmScanAt = 0; }
            if (Run.IsActive && !loadout.Active)
            { Abort("Original loadout restored after a save-protection error. F7 returns to the hall."); return; }
            if (Run.IsActive && (HeroController.instance == null || PlayerData.instance == null || PlayerData.instance.health <= 0))
            { PlayerDied(); return; }
            if (Input.GetKeyDown(KeyCode.F7)) { ReturnToHall(); return; }
            if (victory.Active)
            {
                if (victory.Advance(Time.unscaledDeltaTime, !Ready, enemies.All(e => NativeBoss.DeathFinished(e.Corpse))))
                {
                    CleanupEnemies(); layouts?.Clear(); ClearBell(); loadout.Restore();
                    message = "The Hollow Trial conquered. Press Confirm to return to the hall.";
                    report?.Invoke("Native boss death sequence finished; victory screen awaits confirmation.");
                }
                var actions = InputHandler.Instance?.inputActions;
                if (Ready && (Input.GetKeyDown(KeyCode.Return) || (actions != null && actions.menuSubmit.WasPressed)) && victory.Confirm()) ReturnToHall();
                return;
            }
            if (Input.GetKeyDown(KeyCode.F6) && !Run.IsActive) HandleStart();
            if (countdown.Pending && !Run.IsActive)
            {
                if (countdown.Advance(Ready, Time.deltaTime)) BeginTrial();
                else if (Ready) message = "Trial begins in " + Mathf.CeilToInt(countdown.Remaining) + "...  F7 cancels and returns.";
            }
            UpdateCardFreeze();
            if (!Run.IsActive || GameManager.instance == null || GameManager.instance.isPaused || InventoryOpen) return;
            if (Run.Phase == RunPhase.Combat)
            {
                if (spawnedToken != Run.WaveToken) QueueCurrentWave();
                TickArrivals();
                foreach (var enemy in enemies)
                    if (!enemy.Killed && (enemy.Object == null || !enemy.Object.activeInHierarchy))
                    { Abort("An enemy disappeared. Trial stopped; F7 returns to the hall."); break; }
            }
            else if (Run.Phase == RunPhase.ChoosingCard)
            {
                if (controlledHero == null)
                {
                    controlledHero = HeroController.instance;
                    controlledHero.RelinquishControl(); selected = 0;
                }
                reward.Advance(Time.unscaledDeltaTime, false);
                if (!reward.CanChoose) return;
                var actions = InputHandler.Instance?.inputActions;
                int count = Run.Offer.Count;
                bool skip = selected == count;
                if (Input.GetKeyDown(KeyCode.LeftArrow) || (actions != null && actions.left.WasPressed)) selected = skip ? 0 : (selected + count - 1) % Math.Max(1, count);
                if (Input.GetKeyDown(KeyCode.RightArrow) || (actions != null && actions.right.WasPressed)) selected = skip ? 0 : (selected + 1) % Math.Max(1, count);
                if (Input.GetKeyDown(KeyCode.DownArrow) || (actions != null && actions.down.WasPressed)) selected = count;
                if (Input.GetKeyDown(KeyCode.UpArrow) || (actions != null && actions.up.WasPressed)) selected = 0;
                if (Input.GetKeyDown(KeyCode.Return) || (actions != null && actions.menuSubmit.WasPressed))
                { if (selected == count) SkipCard(); else SelectCard(selected); }
                else if (Input.GetKeyDown(KeyCode.Tab)) SkipCard();
            }
        }
        private void PrepareUi()
        {
            inventoryUi.Prepare(GameManager.instance?.inventoryFSM?.gameObject);
            var hudRoot = GameCameras.instance?.hudCanvas;
            inventoryUi.Prepare(hudRoot, healthOnly: true);
            blueHealthHud.Prepare(hudRoot);
        }
        private void UpdateCardFreeze() => cardFreeze.Apply(Run.Phase == RunPhase.ChoosingCard || InventoryOpen,
            GameManager.instance != null && GameManager.instance.isPaused, HeroController.instance?.gameObject);
        private void RestoreCardTime()
        {
            cardFreeze.Restore(GameManager.instance != null && GameManager.instance.isPaused);
            RestoreCardParticles();
        }
        private void LateUpdate()
        {
            if (!shuttingDown && ownsRoom && Run.Phase == RunPhase.ChoosingCard)
            {
                // Native HUD/foreground particles can be composited over IMGUI.
                // Hide renderers only: preserve frozen particles and their timers.
                foreach (var renderer in FindObjectsOfType<ParticleSystemRenderer>())
                    if (renderer.enabled)
                    { hiddenCardParticles.Add(renderer); renderer.enabled = false; }
            }
            else RestoreCardParticles();
        }
        private void RestoreCardParticles()
        {
            foreach (var renderer in hiddenCardParticles) if (renderer != null) renderer.enabled = true;
            hiddenCardParticles.Clear();
        }
        private void QueueCurrentWave()
        {
            if (arrivals.Count == 0) nextArrivalAt = Time.time + (enemies.Any(e => !e.Killed) ? 0.2f : 1.5f);
            spawnedToken = Run.WaveToken;
            waveNotice.Show(Run.CurrentWave.Number, Run.CurrentWave.Title);
            for (int i = 0; i < Run.CurrentWave.EnemyCount; i++)
                arrivals.Enqueue(new Arrival { Definition = Run.CurrentWave.Enemies[i], Index = i,
                    Token = Run.WaveToken, Wave = Run.CurrentWave.Number });
            // Keep the current geometry while fights overlap. Changing terrain
            // under active enemies can trap them or invalidate boss attacks.
            if (!enemies.Any(e => !e.Killed) && !warningPoint.HasValue)
                layouts.Apply(Run.CurrentWave.Number, left, right, floor, HeroController.instance.transform.position.x, report, PlayerData.instance.hazardRespawnLocation.x);
            message = Run.CurrentWave.Title + " · Strike the high bell to call another wave.";
        }
        internal void CallNextWave()
        {
            if (!Ready || !Run.CallNextWave()) return;
            QueueCurrentWave();
            report?.Invoke($"Bell called wave {Run.CurrentWave.Number}; {Run.EnemiesRemaining} foes active or arriving.");
        }
        private void CreateBell()
        {
            if (bell != null) return;
            bell = Instantiate(stageTemplates[PlatformKind.Bell]); bell.SetActive(false);
            bell.name = "Gauntlet.WaveBell";
            bell.transform.position = new Vector3(right - 1.3f, ceiling.Top - 1.8f, -1);
            bell.AddComponent<WaveBell>().Configure(this); bell.SetActive(true);
        }
        private void ClearBell()
        {
            if (bell != null) { bell.SetActive(false); Destroy(bell); }
            bell = null;
        }
        private void HandleStart()
        {
            if (UnitySceneManager.GetActiveScene().name == HallScene)
            {
                if (podium != null) podium.Toggle();
                else PodiumModeChanged(!armed);
                return;
            }
            if (ownsRoom && !countdown.Pending)
            {
                countdown.Queue();
                message = "Trial begins in 3 seconds. F7 returns to the hall.";
            }
        }
        private void BeginTrial()
        {
            if (!Ready) return;
            layouts?.Clear();
            if (!boundsReady) FindArenaBounds();
            var camera = GameCameras.instance?.mainCamera;
            if (camera == null) throw new InvalidOperationException("Gameplay camera unavailable for arena ceiling.");
            float depth = camera.WorldToViewportPoint(new Vector3((left + right) / 2, floor, 0)).z;
            float visibleTop = camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth)).y;
            ceiling.Ensure(left, right, floor, report, visibleTop);
            roofArt.Build(stageTemplates[PlatformKind.Wide], left, right, ceiling.Top);
            stage?.StartMusic();
            reward.Cancel(); victory.Cancel(); CleanupEnemies(); ClearBell(); CreateBell(); Run.Start(nextSeed++); PrepareUi(); loadout.Begin(Run); spawnedToken = -1; resetAt = Time.time;
            cards.PrepareIcons(report);
            report?.Invoke("Run loadout applied: 5 masks, Old Nail, movement, jump and Mantis Claw; original profile protected from saves.");
        }
        private void FindArenaBounds()
        {
            var entrance = GameObject.Find("Boss Scene Controller/door_dreamEnter");
            if (entrance == null) throw new InvalidOperationException("Could not locate the arena entrance.");
            // Start below the room's roof, near the native dream entrance;
            // casting from above the ceiling would mistake it for the floor.
            var center = entrance.transform.position;
            var ground = Physics2D.Raycast(new Vector2(center.x, center.y + 2f), Vector2.down, 10f, 1 << 8);
            if (ground.collider == null) throw new InvalidOperationException("Could not locate the Soul Tyrant arena floor.");
            floor = ground.point.y;
            var origin = new Vector2(center.x, floor + 2f);
            var l = Physics2D.Raycast(origin, Vector2.left, 30f, 1 << 8);
            var r = Physics2D.Raycast(origin, Vector2.right, 30f, 1 << 8);
            if (l.collider == null || r.collider == null || r.point.x - l.point.x < SpawnSafety.MinimumArenaWidth)
                throw new InvalidOperationException($"Could not locate the Soul Tyrant arena walls: left={l.collider?.name} at {l.point.x:F2}, right={r.collider?.name} at {r.point.x:F2}, floor={floor:F2}.");
            left = l.point.x; right = r.point.x; boundsReady = true;
            report?.Invoke($"Arena bounds: {left:F1}..{right:F1}; floor {floor:F1}");
        }
        private void TickArrivals()
        {
            if (arrivals.Count == 0 || Time.time < nextArrivalAt) return;
            var arrival = arrivals.Peek();
            var definition = arrival.Definition;
            if (!warningPoint.HasValue)
            {
                float x = SpawnSafety.Pick(left, right, arrival.Index);
                warningPoint = new Vector3(x, floor + (definition.Kind == EnemyKind.GruzMother ? 6f : definition.Kind == EnemyKind.WingedSentry || definition.Kind == EnemyKind.Aspid || definition.Kind == EnemyKind.PrimalAspid || definition.Kind == EnemyKind.Vengefly ? 5f : 1.2f), 0f);
                warningAt = Time.time;
                marker = new GameObject("Gauntlet.ArrivalCloud").AddComponent<ArrivalMarker>();
                marker.Configure(warningPoint.Value, arrivalPuff);
            }
            float elapsed = Time.time - warningAt;
            if (elapsed < SpawnSafety.WarningSeconds) return;
            SpawnEnemy(arrival, warningPoint.Value);
            arrivals.Dequeue(); ClearWarning(true); nextArrivalAt = Time.time + SpawnSafety.StaggerSeconds;
            message = Run.CurrentWave.Title + " · " + arrivals.Count + " arrivals remaining";
        }
        private void SpawnEnemy(Arrival arrival, Vector3 point)
        {
            var definition = arrival.Definition; int index = arrival.Index;
            var go = Instantiate(templates[definition.Kind]);
            go.SetActive(false); go.name = "Gauntlet." + definition.Kind + "." + index; go.transform.position = point;
            var enemy = new Enemy { Object = go, Health = go.GetComponent<HealthManager>() };
            enemies.Add(enemy);
            foreach (var component in go.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && (component.GetType().Name == "PersistentBoolItem" || component.GetType().Name == "PersistentIntItem"))
                { component.enabled = false; Destroy(component); }
            if (enemy.Health == null) throw new InvalidOperationException("Enemy has no HealthManager: " + definition.Kind);
            if (definition.Health > 0) enemy.Health.hp = definition.Health;
            enemy.Health.SetGeoSmall(0); enemy.Health.SetGeoMedium(0); enemy.Health.SetGeoLarge(0);
            long token = arrival.Token;
            enemy.Callback = () => EnemyDied(enemy, token, index);
            enemy.Health.OnDeath += enemy.Callback;
            if (definition.Kind == EnemyKind.GruzMother) enemy.Corpse = NativeBoss.Prepare(go);
            else if (definition.Kind == EnemyKind.BrokenVessel)
            {
                go.AddComponent<BrokenVessel>().Configure(left, right, floor);
                enemy.Corpse = NativeBoss.PrepareVesselCorpse(go);
            }
            else go.AddComponent<EnemyEngagement>().Configure(this, definition.Kind, right - left);
            // Use local box geometry even when a native intro temporarily
            // disables collision (Broken Vessel). Inactive bounds are empty.
            var body = go.GetComponent<Rigidbody2D>();
            var solids = go.GetComponentsInChildren<BoxCollider2D>(true)
                .Where(c => !c.isTrigger && c.attachedRigidbody == body).ToArray();
            if (solids.Length == 0) throw new InvalidOperationException("Enemy has no solid body: " + definition.Kind);
            var corners = solids.SelectMany(c => new[] {
                c.transform.TransformPoint(new Vector3(c.offset.x - c.size.x / 2, c.offset.y - c.size.y / 2, 0)),
                c.transform.TransformPoint(new Vector3(c.offset.x + c.size.x / 2, c.offset.y - c.size.y / 2, 0)),
                c.transform.TransformPoint(new Vector3(c.offset.x - c.size.x / 2, c.offset.y + c.size.y / 2, 0)),
                c.transform.TransformPoint(new Vector3(c.offset.x + c.size.x / 2, c.offset.y + c.size.y / 2, 0)) }).ToArray();
            var position = go.transform.position;
            position.y = SpawnSafety.FitHeight(position.y, corners.Min(c => c.y), corners.Max(c => c.y), floor, ceiling.Top);
            go.transform.position = position;
            go.SetActive(true);
            Physics2D.SyncTransforms();
            report?.Invoke($"Wave {arrival.Wave}: {definition.Kind} arrived at {position.x:F1}, {position.y:F1}; native HP {enemy.Health.hp}");
        }
        private void EnemyDied(Enemy enemy, long token, int index)
        {
            if (enemy.Killed) return;
            enemy.Killed = true;
            var previousPhase = Run.Phase;
            int cleared = Run.ClearedWaveCount;
            if (!Run.EnemyKilled(token, index)) return;
            if (Run.Phase == RunPhase.ChoosingCard) UpdateCardFreeze();
            if (Run.ClearedWaveCount != cleared) loadout.RepairFragileCharms(Run);
            if (previousPhase != RunPhase.ChoosingCard && Run.Phase == RunPhase.ChoosingCard)
            {
                UpdateCardFreeze(); reward.Begin(); selected = 0; ClearWarning();
                message = "Level " + Run.RewardLevel + " reached.";
            }
            if (Run.Phase == RunPhase.Victory) BeginVictory();
        }
        private void BeginVictory()
        {
            if (victory.Active) return;
            RestoreCardTime();
            countdown.Cancel(); reward.Cancel(); ClearWarning(); PurgeProjectiles();
            controlledHero = HeroController.instance;
            if (controlledHero != null) controlledHero.RelinquishControl();
            victory.Begin();
            message = "The Broken Court defeated. The trial is complete.";
            report?.Invoke("Boss finale cleared; waiting for native death sequence before victory screen.");
        }
        private void SelectCard(int index)
        {
            if (Run.Phase != RunPhase.ChoosingCard || !reward.CanChoose || GameManager.instance.isPaused || InventoryOpen) return;
            if (Run.Offer.Count == 0) { SkipCard(); return; }
            if (index < 0 || index >= Run.Offer.Count) return;
            Run.Choose(Run.Offer[index].Id); if (Run.IsActive) loadout.Apply(Run); reward.Cancel();
            if (Run.Recovery > 0 && !Run.IsCharmEnabled(27)) HeroController.instance.AddHealth(Run.Recovery);
            FinishChoice();
        }
        private void FinishChoice()
        {
            UpdateCardFreeze();
            if (Run.Phase == RunPhase.ChoosingCard) { reward.Begin(); selected = 0; }
            else if (Run.Phase == RunPhase.Victory) BeginVictory();
            else ReleaseControl();
        }
        private void SkipCard()
        { if (Run.Phase == RunPhase.ChoosingCard && reward.CanChoose && !GameManager.instance.isPaused && !InventoryOpen) { Run.Skip(); reward.Cancel(); FinishChoice(); } }
        private void ReleaseControl()
        {
            if (controlledHero != null && controlledHero == HeroController.instance && PlayerData.instance != null && PlayerData.instance.health > 0)
                controlledHero.RegainControl();
            controlledHero = null;
        }
        internal void MenuCharmUpdate(Action nativeUpdate)
        {
            if (ownsRoom && Run.IsActive && loadout != null) loadout.MenuCharmUpdate(Run, nativeUpdate);
            else nativeUpdate();
        }
        internal void KnightDamaged(int previousHealth, int previousBlue)
        {
            if (!ownsRoom || Run.Phase != RunPhase.Combat || !loadout.DamageTaken(previousHealth, previousBlue)) return;
            message = "Fragile charms broke. They repair when this wave is cleared.";
            report?.Invoke("Fragile charm break: Heart=" + PlayerData.instance.brokenCharm_23
                + "; Strength=" + PlayerData.instance.brokenCharm_25);
        }
        internal void PlayerDied()
        {
            countdown.Cancel(); reward.Cancel(); victory.Cancel();
            if (!Run.IsActive) return;
            // Keep the temporary profile during the native death sequence;
            // the save hook supplies original values, and scene exit restores
            // the live profile without reviving the dying Knight.
            Run.Defeat(); charmMenu.Restore(); inventoryUi?.Restore(); blueHealthHud.Restore(); RestoreCardTime(); controlledHero = null; CleanupEnemies(); layouts?.Clear(); ClearBell();
            message = "Defeat. Upgrades cleared.";
        }
        private void Abort(string reason)
        { countdown.Cancel(); reward.Cancel(); victory.Cancel(); Run.Abandon(); charmMenu.Restore(); inventoryUi?.Restore(); RestoreCardTime(); loadout?.Restore(); blueHealthHud.Restore(); ReleaseControl(); CleanupEnemies(); layouts?.Clear(); ClearBell(); message = reason; }
        private void ReturnToHall()
        {
            if (!ownsRoom) { armed = false; countdown.Cancel(); return; }
            if (!Ready) return;
            if (BossSceneController.Instance == null) throw new InvalidOperationException("The native dream-return controller is unavailable.");
            Abort("Returning to the Hall of Gods.");
            // This uses the statue's existing dream-return gate and bypasses
            // EndBossScene, so gauntlet wins don't mark Gruz Mother as defeated.
            if (BossSceneController.Instance != null) BossSceneController.Instance.DoDreamReturn();
        }
        private void ClearWarning(bool complete = false)
        {
            warningPoint = null;
            if (marker != null)
            {
                if (complete) marker.Complete();
                else Destroy(marker.gameObject);
            }
            marker = null;
        }
        private void PurgeProjectiles()
        {
            if (!ownsRoom) return;
            foreach (var damage in FindObjectsOfType<DamageHero>())
            {
                if (damage.gameObject.scene.name != ArenaScene || damage.GetComponentInParent<HealthManager>() != null) continue;
                var body = damage.GetComponentInParent<Rigidbody2D>();
                if (body != null && body.GetComponentInParent<HealthManager>() == null) Destroy(body.gameObject);
            }
        }
        private void CleanupEnemies()
        {
            arrivals.Clear(); ClearWarning(); PurgeProjectiles();
            foreach (var enemy in enemies)
            {
                if (enemy.Health != null && enemy.Callback != null) enemy.Health.OnDeath -= enemy.Callback;
                if (enemy.Corpse != null)
                {
                    if (enemy.Corpse.activeInHierarchy) NativeBoss.StopDeathEffects();
                    Destroy(enemy.Corpse);
                }
                if (enemy.Object != null) Destroy(enemy.Object);
            }
            enemies.Clear();
        }
        internal void Shutdown()
        {
            if (shuttingDown) return;
            shuttingDown = true;
            ModHooks.BeforeSceneLoadHook -= BeforeSceneLoad;
            UnitySceneManager.sceneLoaded -= SceneLoaded;
            UnitySceneManager.activeSceneChanged -= SceneChanged;
            Abort("Mod unloaded.");
            stage?.Dispose(); stage = null; roofArt.Dispose(); ceiling.Dispose();
            if (podium != null) { podium.Shutdown(); Destroy(podium); podium = null; }
            blueHealthHud.Restore(); inventoryUi?.Dispose(); cards?.Dispose(); loadout?.Dispose(); layouts?.Dispose();
        }
        private void OnDestroy() => Shutdown();
        private void OnGUI()
        {
            string scene = UnitySceneManager.GetActiveScene().name;
            if (shuttingDown || HeroController.instance == null || (scene != HallScene && !ownsRoom)) return;
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 960f, Screen.height / 540f, 1));
            try
            {
                if (ownsRoom) hud.DrawExperience(Run);
                if (victory.ShowingScreen)
                {
                    cards?.DrawVictory(Run.WaveCount, victory.Elapsed);
                    return;
                }
                bool fragileBroken = Run.Phase == RunPhase.Combat && (PlayerData.instance.brokenCharm_23 || PlayerData.instance.brokenCharm_25);
                string controls = scene == HallScene ? "Lever / F6: Swap idol"
                    : countdown.Pending ? "F7: Return to hall" : Run.IsActive ? "F7: Return to hall" : "F6: Retry | F7: Return";
                string notice = fragileBroken ? "Fragile charms broken until wave clear"
                    : !Run.IsActive || message.StartsWith("Next arrival") ? message : "";
                if (Run.Phase != RunPhase.ChoosingCard) hud.Draw(Run, controls, notice, fragileBroken);
                if (Run.IsActive && Time.time - resetAt < 1.5f)
                {
                    float age = Time.time - resetAt;
                    cards?.DrawReset(age);
                }
                if (Run.Phase == RunPhase.Combat && !InventoryOpen && !GameManager.instance.isPaused && Time.time - resetAt >= 1.5f)
                    hud.DrawWaveAnnouncement(waveNotice, Run.WaveCount);
                if (Run.Phase != RunPhase.ChoosingCard || cards == null) return;
                if (reward.ShowingLevelUp)
                {
                    cards.DrawLevelUp(Run.RewardLevel, Run.PendingRewards, reward.Elapsed);
                    return;
                }
                int action = cards.Draw(Run, ref selected, reward.CanChoose && !GameManager.instance.isPaused && !InventoryOpen, reward.CardProgress);
                if (action >= 0) SelectCard(action);
                else if (action == -2) SkipCard();
            }
            finally { GUI.matrix = previousMatrix; }
        }
    }
}
