using System;
using System.Collections.Generic;
using Gauntlet.Core;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;

namespace Gauntlet.Mod
{
    public sealed class GauntletMod : Modding.Mod, ITogglableMod
    {
        internal static readonly string SupportedVersion = "1.5.78.11833";
        // Read at runtime: direct use of the const would inline the build-time version.
        private static string CurrentGameVersion =>
            typeof(Constants).GetField("GAME_VERSION")?.GetRawConstantValue() as string ?? "unknown";
        private GameObject host;
        private ArenaController arena;
        public override string GetVersion() => "0.14.0-experimental";
        public override List<(string, string)> GetPreloadNames() =>
            CurrentGameVersion == SupportedVersion
                ? EnemyCatalog.Preloads()
                : new List<(string, string)>();

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloads)
        {
            if (CurrentGameVersion != SupportedVersion)
            {
                LogError("Gauntlet requires Hollow Knight " + SupportedVersion + ". No hooks installed.");
                return;
            }
            if (host != null) return;
            var templates = EnemyCatalog.Resolve(preloads);
            var platforms = ArenaLayouts.Resolve(preloads);
            if (templates == null || platforms == null)
            {
                LogError("An encounter preload is missing. Gauntlet disabled; check the mod log.");
                return;
            }
            host = new GameObject("Gauntlet.RunController");
            UnityEngine.Object.DontDestroyOnLoad(host);
            arena = host.AddComponent<ArenaController>();
            arena.Configure(templates, platforms, message => Log(message));
            On.HeroController.CanFocus += CanFocus;
            On.HeroController.CharmUpdate += CharmUpdate;
            On.HeroController.MaxHealth += MaxHealth;
            ModHooks.GetPlayerIntHook += CharmCost;
            On.HeroController.TakeDamage += TakeDamage;
            ModHooks.HitInstanceHook += ModifyHit;
            ModHooks.SoulGainHook += ModifySoul;
            ModHooks.AfterTakeDamageHook += ModifyDamage;
            ModHooks.BeforePlayerDeadHook += PlayerDied;
            Log("Experimental gauntlet ready. Use a backed-up test save; strike the lever beside Gruz Mother in the Hall of Gods and challenge The Hollow Trial on Attuned; the trial starts automatically after the arrival countdown.");
        }

        private int CharmCost(string name, int value)
        {
            if (arena == null || !arena.Run.IsActive) return value;
            return name == "charmSlotsFilled" || name.StartsWith("charmCost_", StringComparison.Ordinal) ? 0 : value;
        }
        private void CharmUpdate(On.HeroController.orig_CharmUpdate original, HeroController hero)
        {
            if (arena == null || hero != HeroController.instance) original(hero);
            else arena.MenuCharmUpdate(() => original(hero));
        }
        private void MaxHealth(On.HeroController.orig_MaxHealth original, HeroController hero)
        {
            // CharmUpdate invokes MaxHealth before the adapter restores wounds.
            // The native proxy broadcasts HERO HEALED FULL, which starts red
            // refill animations and DESTROYS blue masks. Keep its data work, but
            // publish the final HUD only after the loadout transaction commits.
            if (hero == HeroController.instance && arena != null && arena.UpdatingLoadoutHealth)
                PlayerData.instance.MaxHealth();
            else original(hero);
        }
        private bool CanFocus(On.HeroController.orig_CanFocus original, HeroController hero)
            => (arena == null || !arena.Run.IsActive || arena.Run.CanFocus) && original(hero);

        private void TakeDamage(On.HeroController.orig_TakeDamage original, HeroController hero, GameObject source,
            GlobalEnums.CollisionSide side, int damage, int hazardType)
        {
            var player = PlayerData.instance;
            int health = player?.health ?? 0, blue = player?.healthBlue ?? 0;
            original(hero, source, side, damage, hazardType);
            if (ReferenceEquals(player, PlayerData.instance)) arena?.KnightDamaged(health, blue);
        }

        private HitInstance ModifyHit(Fsm owner, HitInstance hit)
        {
            // Layer 17 is the game's player-attack layer. Do not buff enemy attacks.
            if (arena == null || !arena.Run.IsActive || hit.Source == null || hit.Source.layer != 17) return hit;
            if (hit.Source.name == "Thorn Hit" || (hit.Source.transform.parent != null && hit.Source.transform.parent.name == "Thorn Hit"))
                hit.DamageDealt += arena.Run.ThornsBonus;
            else if (hit.AttackType == AttackTypes.Nail)
                hit.DamageDealt += arena.Run.NailBonus(PlayerData.instance.health);
            else if (hit.AttackType == AttackTypes.Spell)
                hit.DamageDealt += arena.Run.SpellBonus;
            return hit;
        }
        private int ModifySoul(int amount) => amount <= 0 || arena == null ? amount : amount + arena.Run.SoulBonus;
        private int ModifyDamage(int hazardType, int amount) => arena == null ? amount : arena.ProtectingKnight ? 0 : arena.Run.IncomingDamage(amount);
        private void PlayerDied() => arena?.PlayerDied();
        public void Unload()
        {
            On.HeroController.CanFocus -= CanFocus;
            On.HeroController.CharmUpdate -= CharmUpdate;
            On.HeroController.MaxHealth -= MaxHealth;
            ModHooks.GetPlayerIntHook -= CharmCost;
            On.HeroController.TakeDamage -= TakeDamage;
            ModHooks.HitInstanceHook -= ModifyHit;
            ModHooks.SoulGainHook -= ModifySoul;
            ModHooks.AfterTakeDamageHook -= ModifyDamage;
            ModHooks.BeforePlayerDeadHook -= PlayerDied;
            if (arena != null) arena.Shutdown();
            if (host != null) UnityEngine.Object.Destroy(host);
            arena = null; host = null;
        }
    }
}
