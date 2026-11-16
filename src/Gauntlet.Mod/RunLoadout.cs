using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauntlet.Core;
using Modding;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Temporary edits to the live PlayerData; save writes receive a separate
    // shallow clone with the ORIGINAL loadout overlaid. Never switch the live
    // singleton, which is cached by the hero, UI and many native FSMs.
    internal sealed class RunLoadout : IDisposable
    {
        private static readonly MethodInfo Clone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Dictionary<string, FieldInfo> Fields = FieldNames().ToDictionary(name => name,
            name => typeof(PlayerData).GetField(name) ?? throw new InvalidOperationException("Missing PlayerData field: " + name));
        private PlayerData owner;
        private LoadoutSnapshot original;
        private bool restoring;
        private readonly Action refreshHud;
        internal bool Active => original != null;
        internal bool Refreshing { get; private set; }
        private int lastRepairedWaveCount;

        private static IEnumerable<string> FieldNames()
        {
            foreach (string name in new[] { "completionPercent", "health", "maxHealth", "maxHealthBase", "maxHealthCap", "healthBlue", "joniHealthBlue", "damagedBlue", "prevHealth", "heartPieces", "heartPieceCollected", "heartPieceMax", "nailDamage", "nailSmithUpgrades", "charmSlots", "charmSlotsFilled", "hasCharm", "charmsOwned", "canOvercharm", "overcharmed", "equippedCharms", "MPCharge", "MPReserve", "maxMP", "MPReserveMax", "MPReserveCap", "vesselFragments", "vesselFragmentCollected", "vesselFragmentMax", "focusMP_amount", "blockerHits" }) yield return name;
            foreach (string name in new[] { "hasDash", "hasWalljump", "hasSuperDash", "hasShadowDash", "hasAcidArmour", "hasDoubleJump", "hasDreamNail", "hasCyclone", "hasDashSlash", "hasUpwardSlash", "canDash", "canBackDash", "canWallJump", "canSuperDash", "canShadowDash", "hasSpell", "hasNailArt", "hasAllNailArts", "hasDreamGate", "dreamNailUpgraded", "brokenCharm_23", "brokenCharm_24", "brokenCharm_25", "fireballLevel", "quakeLevel", "screamLevel", "grimmChildLevel", "royalCharmState" }) yield return name;
            for (int id = 1; id <= 40; id++)
            { yield return "gotCharm_" + id; yield return "equippedCharm_" + id; }
        }
        internal RunLoadout(Action refreshHud = null)
        {
            this.refreshHud = refreshHud;
            if (Clone == null) throw new InvalidOperationException("PlayerData save cloning is unavailable.");
            // Force validation before any player data is changed.
            if (Fields.Count == 0) throw new InvalidOperationException("Loadout fields unavailable.");
            ModHooks.BeforeSavegameSaveHook += ProtectSave;
        }
        internal void Begin(Run run)
        {
            if (Active) Restore();
            lastRepairedWaveCount = 0;
            owner = PlayerData.instance ?? throw new InvalidOperationException("No Knight to configure.");
            original = new LoadoutSnapshot(Fields.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value.GetValue(owner))));
            try
            {
                owner.health = 5; owner.prevHealth = 5;
                owner.healthBlue = 0; owner.joniHealthBlue = 0; owner.damagedBlue = false;
                owner.MPCharge = 0; owner.MPReserve = 0;
                Apply(run, true);
            }
            catch { Restore(); throw; }
        }
        internal void Apply(Run run, bool starting = false, bool waveClear = false, bool refillJoni = false)
        {
            if (!Active || !ReferenceEquals(owner, PlayerData.instance)) throw new InvalidOperationException("The run loadout is not attached to this save.");
            bool wasJoni = owner.equippedCharm_27;
            int previousBlue = owner.healthBlue;
            int health = starting ? 5 : owner.health + Math.Max(0, run.MaximumMasks - owner.maxHealthBase)
                + (run.IsCharmEnabled(23) && (!owner.equippedCharm_23 || (waveClear && owner.brokenCharm_23)) ? 2 : 0);
            owner.maxHealthBase = run.MaximumMasks;
            owner.maxHealth = run.MaximumMasks;
            owner.maxHealthCap = 9;
            owner.maxMP = 99; owner.MPReserveMax = 33 * run.Stack(Upgrade.Vessel);
            owner.MPReserveCap = 99; owner.vesselFragments = 0; owner.vesselFragmentCollected = false;
            owner.vesselFragmentMax = owner.MPReserveMax == 99; owner.focusMP_amount = 33; owner.blockerHits = 4;
            owner.heartPieces = 0;
            owner.heartPieceCollected = false;
            owner.heartPieceMax = run.MaximumMasks >= 9;
            // Native inventory recognizes four tiers; damage may exceed Pure Nail.
            owner.nailSmithUpgrades = Math.Min(4, run.NailTier);
            owner.nailDamage = run.BaseNailDamage;
            // Only earned trial charms become available in the native menu.
            // Their ownership and effects remain separate when disabled.
            var owned = run.OwnedCharmIds;
            owner.hasCharm = owned.Count > 0; owner.charmsOwned = owned.Count;
            owner.charmSlots = 3; owner.charmSlotsFilled = 0;
            owner.canOvercharm = false; owner.overcharmed = false;
            for (int id = 1; id <= 40; id++)
            {
                Fields["gotCharm_" + id].SetValue(owner, owned.Contains(id));
                Fields["equippedCharm_" + id].SetValue(owner, run.IsCharmEnabled(id));
            }
            owner.equippedCharms = new List<int>(run.CharmIds);
            owner.hasDash = run.Stack(Upgrade.Dash) > 0;
            owner.hasWalljump = run.Stack(Upgrade.WallJump) > 0;
            owner.hasSuperDash = run.Stack(Upgrade.SuperDash) > 0;
            owner.hasShadowDash = run.Stack(Upgrade.ShadowDash) > 0;
            owner.hasAcidArmour = run.Stack(Upgrade.AcidArmour) > 0;
            owner.hasDoubleJump = run.Stack(Upgrade.DoubleJump) > 0;
            owner.hasDreamNail = run.Stack(Upgrade.DreamNail) > 0;
            owner.hasCyclone = run.Stack(Upgrade.Cyclone) > 0;
            owner.hasDashSlash = run.Stack(Upgrade.DashSlash) > 0;
            owner.hasUpwardSlash = run.Stack(Upgrade.GreatSlash) > 0;
            owner.canDash = owner.hasDash; owner.canBackDash = false;
            owner.canWallJump = owner.hasWalljump; owner.canSuperDash = owner.hasSuperDash;
            owner.canShadowDash = owner.hasShadowDash;
            owner.hasDreamGate = false; owner.dreamNailUpgraded = false;
            owner.fireballLevel = run.Stack(Upgrade.Fireball); owner.quakeLevel = run.Stack(Upgrade.Quake); owner.screamLevel = run.Stack(Upgrade.Scream);
            owner.hasSpell = owner.fireballLevel + owner.quakeLevel + owner.screamLevel > 0;
            owner.hasNailArt = owner.hasCyclone || owner.hasDashSlash || owner.hasUpwardSlash;
            owner.hasAllNailArts = owner.hasCyclone && owner.hasDashSlash && owner.hasUpwardSlash;
            if (starting || waveClear) owner.brokenCharm_23 = owner.brokenCharm_24 = owner.brokenCharm_25 = false;
            owner.grimmChildLevel = run.Stack(Upgrade.Grimmchild) > 0 ? 4 : 0;
            // State 3 is the completed Kingsoul; state 4 is Void Heart. Never
            // inherit the normal save's variant or change it permanently.
            owner.royalCharmState = run.Stack(Upgrade.Kingsoul) > 0 ? 3 : 0;
            Refresh();
            // Native CharmUpdate heals fully; preserve wounds, except for the
            // one mask gained from a mask card. Other healing remains explicit.
            if (run.IsCharmEnabled(27))
            {
                // Keep native Joni conversion and let UPDATE BLUE HEALTH build
                // its blue masks. Only acquisition and five-wave milestones refill it.
                owner.health = 1;
                if (wasJoni && !starting && !refillJoni)
                    owner.healthBlue = Math.Min(previousBlue, Math.Max(0, owner.joniHealthBlue - 1));
                else owner.healthBlue = Math.Max(0, owner.joniHealthBlue - 1);
            }
            else
            {
                owner.health = Math.Min(owner.maxHealth, Math.Max(0, health));
                owner.healthBlue = 0; owner.joniHealthBlue = 0;
            }
            if (run.IsCharmEnabled(27) && wasJoni && !starting && !refillJoni) PublishHealth();
            else RefreshHealth();
        }
        internal void MenuCharmUpdate(Run run, Action nativeUpdate)
        {
            if (!Active || Refreshing || restoring || !run.IsActive || !ReferenceEquals(owner, PlayerData.instance))
            { nativeUpdate(); return; }
            // Native CharmUpdate refills masks and Joni. Keep the remaining
            // health budget so repeated menu toggles cannot heal the Knight.
            bool wasJoni = run.IsCharmEnabled(27);
            int red = owner.health, blue = owner.healthBlue;
            int budget = wasJoni ? red + blue : red;
            foreach (int id in run.OwnedCharmIds)
                run.SetCharmEnabled(id, (bool)Fields["equippedCharm_" + id].GetValue(owner));
            owner.equippedCharms = new List<int>(run.CharmIds);
            owner.charmSlotsFilled = 0; owner.overcharmed = false;
            Refreshing = true;
            try
            {
                nativeUpdate();
                PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
            finally { Refreshing = false; }
            if (run.IsCharmEnabled(27))
            {
                owner.health = red <= 0 ? 0 : 1;
                owner.healthBlue = Math.Max(0, Math.Min(owner.joniHealthBlue - 1, budget - 1));
            }
            else
            {
                owner.health = Math.Max(0, Math.Min(owner.maxHealth, budget));
                owner.joniHealthBlue = 0;
                owner.healthBlue = wasJoni ? 0 : blue;
            }
            if (wasJoni != run.IsCharmEnabled(27)) RefreshHealth();
            else PublishHealth();
        }
        internal bool DamageTaken(int previousHealth, int previousBlue)
        {
            // The native damage hook runs before health is deducted. This is
            // called AFTER TakeDamage returns, so shields, i-frames and blocked
            // hits don't break a charm. Blue-mask damage counts too.
            if (!Active || !ReferenceEquals(owner, PlayerData.instance) || owner.health <= 0
                || owner.health + owner.healthBlue >= previousHealth + previousBlue) return false;
            bool heart = owner.equippedCharm_23 && !owner.brokenCharm_23;
            bool strength = owner.equippedCharm_25 && !owner.brokenCharm_25;
            if (!heart && !strength) return false;
            if (strength) owner.brokenCharm_25 = true;
            if (heart)
            {
                owner.brokenCharm_23 = true;
                if (owner.equippedCharm_27)
                {
                    owner.joniHealthBlue = (int)(owner.maxHealthBase * 1.4f);
                    owner.healthBlue = Math.Min(owner.healthBlue, Math.Max(0, owner.joniHealthBlue - 1));
                }
                else
                {
                    owner.maxHealth = owner.maxHealthBase;
                    owner.health = Math.Min(owner.health, owner.maxHealth);
                }
            }
            // Native nail damage checks equippedCharm_25 AND !brokenCharm_25.
            // Avoid CharmUpdate/UPDATE BLUE HEALTH here: both can heal/refill.
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            if (heart) PublishHealth();
            else refreshHud?.Invoke();
            return true;
        }
        internal void RepairFragileCharms(Run run)
        {
            if (!Active || !ReferenceEquals(owner, PlayerData.instance) || owner.health <= 0
                || !run.IsActive || run.ClearedWaveCount <= lastRepairedWaveCount) return;
            bool refillJoni = run.ClearedWaveCount / 5 > lastRepairedWaveCount / 5;
            lastRepairedWaveCount = run.ClearedWaveCount;
            Apply(run, waveClear: true, refillJoni: refillJoni);
        }
        private void Refresh()
        {
            if (HeroController.instance == null) return;
            Refreshing = true;
            try { HeroController.instance.CharmUpdate(); }
            finally { Refreshing = false; }
            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        }
        private void PublishHealth()
        {
            // Never broadcast while CharmUpdate's temporary full health is live.
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            refreshHud?.Invoke();
        }
        private void RefreshHealth()
        {
            // BlueHealthHud makes this a display-only rebuild. Remaining blue HP
            // is set explicitly above; opening inventory cannot refill Joni.
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");
            if (HeroController.instance != null) HeroController.instance.AddHealth(0);
            PublishHealth();
        }
        private void ProtectSave(SaveGameData save)
        {
            if (!Active || save == null || !ReferenceEquals(save.playerData, owner)) return;
            try
            {
                var copy = (PlayerData)Clone.Invoke(save.playerData, null);
                original.Apply((name, value) => Fields[name].SetValue(copy, value));
                save.playerData = copy;
            }
            catch
            {
                // If save-copy construction ever fails, restore the LIVE profile
                // before the loader handles the error and continues saving.
                Restore();
                throw;
            }
        }
        internal void Restore(bool preserveDeath = false)
        {
            if (!Active || restoring) return;
            preserveDeath = preserveDeath || owner.health <= 0;
            restoring = true;
            try
            {
                // Keep the snapshot active through restoration so a nested save
                // still receives the original profile, even during a UI refresh.
                original.Apply((name, value) => Fields[name].SetValue(owner, value));
                if (ReferenceEquals(owner, PlayerData.instance))
                {
                    if (preserveDeath) owner.health = 0;
                    else
                    {
                        Refresh();
                        // Refresh can change health/blue health for original charms.
                        original.Apply((name, value) => Fields[name].SetValue(owner, value));
                        RefreshHealth();
                    }
                }
                original = null; owner = null;
            }
            finally { restoring = false; }
        }
        public void Dispose()
        {
            Restore();
            ModHooks.BeforeSavegameSaveHook -= ProtectSave;
        }
    }
}
