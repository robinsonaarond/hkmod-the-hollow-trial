using System;
using System.Collections.Generic;
using System.Linq;
using Gauntlet.Core;
using Gauntlet.Mod;
using Modding;

static class Program
{
    private static int checks;
    private static void Check(bool condition, string message)
    { checks++; if (!condition) throw new Exception(message); }
    private sealed class KillCursor { public long Token = -1; public int Index; }
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Run, KillCursor> cursors = new System.Runtime.CompilerServices.ConditionalWeakTable<Run, KillCursor>();
    private static void Clear(Run run)
    {
        var cursor = cursors.GetOrCreateValue(run);
        while (run.IsActive && run.Phase == RunPhase.Combat)
        {
            if (cursor.Token != run.WaveToken) { cursor.Token = run.WaveToken; cursor.Index = 0; }
            if (!run.EnemyKilled(cursor.Token, cursor.Index++)) throw new Exception("Owned kill rejected by test harness");
        }
    }
    static void Main()
    {
        var pd = PlayerData.instance;
        pd.health = 9; pd.maxHealthBase = 9; pd.maxHealth = 11; pd.maxHealthCap = 9;
        pd.healthBlue = 4; pd.heartPieces = 3; pd.nailDamage = 21; pd.nailSmithUpgrades = 4;
        pd.completionPercent = 112; pd.MPCharge = 77; pd.MPReserve = 33;
        pd.hasCharm = true; pd.charmsOwned = 40; pd.charmSlots = 11; pd.charmSlotsFilled = 5;
        pd.gotCharm_23 = pd.gotCharm_32 = pd.equippedCharm_23 = pd.equippedCharm_32 = true;
        pd.equippedCharms = new List<int> { 23, 32 };
        pd.hasDash = pd.canDash = pd.hasWalljump = pd.hasDoubleJump = pd.hasSuperDash = pd.hasShadowDash = true;
        pd.hasDreamNail = pd.hasDreamGate = pd.hasSpell = pd.hasNailArt = pd.hasCyclone = true;
        pd.fireballLevel = pd.quakeLevel = pd.screamLevel = 2;
        pd.MPReserveMax = pd.MPReserveCap = 99; pd.vesselFragments = 2;
        pd.royalCharmState = 4; pd.grimmChildLevel = 5; pd.brokenCharm_25 = true;
        var protectedNames = new[] { "hasDash", "canDash", "hasWalljump", "hasDoubleJump", "hasSuperDash", "hasShadowDash", "hasDreamNail", "hasDreamGate", "hasSpell", "hasNailArt", "hasCyclone", "fireballLevel", "quakeLevel", "screamLevel", "grimmChildLevel", "brokenCharm_25", "MPReserveMax", "MPReserveCap", "vesselFragments", "royalCharmState" };
        var originalAbilities = protectedNames.ToDictionary(name => name, name => typeof(PlayerData).GetField(name).GetValue(pd));
        var run = new Run(Enumerable.Range(1, 250).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel))));
        run.Start(12);
        int shownHealth = -1, shownMax = -1;
        using (var loadout = new RunLoadout(() => { shownHealth = PlayerData.instance.health; shownMax = PlayerData.instance.maxHealth; }))
        {
            loadout.Begin(run);
            Check(pd.health == 5 && pd.maxHealthBase == 5 && pd.maxHealth == 5 && pd.healthBlue == 0, "Starting health not reset");
            Check(shownHealth == 5 && shownMax == 5, "HUD was refreshed before the baseline health was applied");
            Check(pd.nailDamage == 5 && pd.nailSmithUpgrades == 0 && pd.heartPieces == 0, "Original upgrade progression leaked");
            Check(pd.equippedCharms.Count == 0 && !pd.equippedCharm_23 && !pd.equippedCharm_32 && !pd.hasCharm && !pd.gotCharm_32, "Original charms remained active/available");
            Check(pd.MPCharge == 0 && pd.MPReserve == 0 && pd.MPReserveMax == 0, "Run did not start with empty Soul");
            Check(ModHooks.SubscriberCount == 1, "Save protection missing");
            Check(!pd.hasDash && !pd.canDash && pd.hasWalljump && pd.canWallJump && !pd.hasDoubleJump && !pd.hasSuperDash && !pd.hasShadowDash && !pd.hasDreamNail && !pd.hasDreamGate && !pd.hasNailArt && !pd.hasSpell, "Inherited abilities leaked into baseline");
            Check(pd.fireballLevel == 0 && pd.quakeLevel == 0 && pd.screamLevel == 0 && !run.CanFocus, "Inherited spells or healing leaked");
            Check(pd.royalCharmState == 0 && !pd.equippedCharm_36, "Inherited Kingsoul/Void Heart variant leaked");
            pd.unrelatedQuest = 72;
            while (run.IsActive && run.CharmIds.Count < Cards.All.Count(c => c.CharmId != 0))
            {
                pd.health = 2;
                int previousMax = pd.maxHealthBase;
                bool hadHeart = pd.equippedCharm_23;
                Clear(run); loadout.RepairFragileCharms(run);
                if (!run.IsActive) break;
                if (run.Offer.Count == 0) run.Skip(); else run.Choose(run.Offer.OrderBy(c => c.Id == Upgrade.Joni ? 1 : 0).First().Id);
                var indicatorHealth = new List<int>();
                PlayMakerFSM.OnBroadcast = name =>
                {
                    if (name == "CHARM INDICATOR CHECK") indicatorHealth.Add(pd.health);
                };
                loadout.Apply(run);
                PlayMakerFSM.OnBroadcast = null;
                Check(indicatorHealth.Count == 1 && indicatorHealth[0] == pd.health,
                    "Charm indicator observed temporary full health before wounds were restored");
                int expectedHealth = 2 + run.MaximumMasks - previousMax + (!hadHeart && pd.equippedCharm_23 ? 2 : 0);
                Check(pd.health == (pd.equippedCharm_27 ? 1 : expectedHealth), "Charm refresh healed wounds beyond explicit mask gains/Joni conversion");
                Check(shownHealth == pd.health && shownMax == pd.maxHealth, "HUD did not receive card-adjusted maximum health and wounds");
                Check(pd.maxHealth == (pd.equippedCharm_27 ? 1 : run.MaximumMasks + (pd.equippedCharm_23 ? 2 : 0)) && pd.nailDamage == run.BaseNailDamage && pd.nailSmithUpgrades == Math.Min(4, run.NailTier), "Selected progression not applied");
                Check(pd.equippedCharms.SequenceEqual(run.CharmIds) && pd.charmSlotsFilled == 0 && !pd.overcharmed, "Boon loadout or notch policy incorrect");
                foreach (int id in run.CharmIds)
                    Check((bool)typeof(PlayerData).GetField("equippedCharm_" + id).GetValue(pd), "Selected charm's native effect flag is missing");
                Check(pd.hasDash == (run.Stack(Upgrade.Dash) > 0) && pd.hasDoubleJump == (run.Stack(Upgrade.DoubleJump) > 0), "Movement cards not applied");
                Check(pd.fireballLevel == run.Stack(Upgrade.Fireball) && pd.quakeLevel == run.Stack(Upgrade.Quake) && pd.screamLevel == run.Stack(Upgrade.Scream), "Spell tiers not applied");
                Check(!pd.brokenCharm_25 && !pd.brokenCharm_23, "Fragile boons were not repaired");
                Check(pd.royalCharmState == (run.Stack(Upgrade.Kingsoul) > 0 ? 3 : 0), "Kingsoul card did not set the completed, non-Void variant");
                if (run.Stack(Upgrade.Kingsoul) > 0) Check(pd.equippedCharm_36 && PlayMakerFSM.Events.Contains("CHARM INDICATOR CHECK"), "Kingsoul native regeneration was not enabled/refreshed");
                var save = ModHooks.Save();
                foreach (var pair in originalAbilities) Check(Equals(typeof(PlayerData).GetField(pair.Key).GetValue(save.playerData), pair.Value), "Save lost original ability/fragile status: " + pair.Key);
                Check(!ReferenceEquals(save.playerData, pd), "Save used the temporary live PlayerData");
                Check(save.playerData.health == 9 && save.playerData.maxHealthBase == 9 && save.playerData.maxHealth == 11 && save.playerData.healthBlue == 4, "Save lost original health");
                Check(save.playerData.nailDamage == 21 && save.playerData.heartPieces == 3 && save.playerData.equippedCharms.SequenceEqual(new[] { 23, 32 }), "Save lost original upgrades/charms");
                Check(save.playerData.MPCharge == 77 && save.playerData.completionPercent == 112 && save.playerData.unrelatedQuest == 72, "Save copy lost Soul, completion or unrelated progress");
                Check(pd.nailDamage == run.BaseNailDamage && pd.maxHealthBase == run.MaximumMasks, "Saving interrupted the active build");
                save.playerData.equippedCharms.Add(19);
            }
            Check(run.CharmIds.Count == Cards.All.Count(c => c.CharmId != 0), "Native adapter test did not exercise all charm boons");
            Check(pd.equippedCharm_27 && !run.CanFocus && pd.joniHealthBlue == (int)((run.MaximumMasks + 2) * 1.4f), "Joni conversion or Focus restriction failed");
            pd.healthBlue = 1;
            int clears = run.ClearedWaveCount;
            while (run.IsActive && run.ClearedWaveCount == clears)
            {
                if (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer[0].Id);
                else Clear(run);
            }
            loadout.RepairFragileCharms(run);
            Check(pd.healthBlue == (run.ClearedWaveCount % 5 == 0 ? pd.joniHealthBlue - 1 : 1), "Joni did not follow five-wave refill milestones");
            Check(run.Stack(Upgrade.Kingsoul) == 1 && pd.equippedCharm_36, "Kingsoul did not appear as a one-rank boon");
            Check(run.OwnedCharmIds.All(id => (bool)typeof(PlayerData).GetField("gotCharm_" + id).GetValue(pd))
                && pd.hasCharm && pd.charmsOwned == run.OwnedCharmIds.Count, "Earned charms are missing from native inventory");
            pd.health = 1; pd.healthBlue = 1; pd.MPCharge = 44;
            pd.equippedCharm_22 = false; pd.equippedCharm_27 = false;
            int indicatorRed = -1, indicatorBlue = -1;
            PlayMakerFSM.OnBroadcast = name =>
            {
                if (name == "CHARM INDICATOR CHECK") { indicatorRed = pd.health; indicatorBlue = pd.healthBlue; }
            };
            loadout.MenuCharmUpdate(run, () => HeroController.instance.CharmUpdate());
            Check(!run.IsCharmEnabled(22) && !run.IsCharmEnabled(27) && pd.gotCharm_22 && pd.gotCharm_27,
                "Unequipping lost ownership or left native effects enabled");
            Check(pd.health == 2 && pd.healthBlue == 0 && pd.MPCharge == 44, "Menu toggle healed wounds or changed Soul");
            Check(indicatorRed == 2 && indicatorBlue == 0, "Menu toggle published health before restoring its budget");
            PlayMakerFSM.OnBroadcast = null;
            Check(!pd.equippedCharms.Contains(22) && !pd.equippedCharms.Contains(27), "Native equipped list diverged from toggle flags");
            int earned = run.OwnedCharmIds.Count;
            loadout.Apply(run);
            Check(!pd.equippedCharm_22 && !pd.equippedCharm_27 && pd.charmsOwned == earned, "Card refresh re-enabled a disabled charm");
            pd.health = 2; pd.equippedCharm_27 = true;
            int blueRowCapacity = -1;
            PlayMakerFSM.OnBroadcast = name =>
            {
                if (name == "UPDATE BLUE HEALTH") blueRowCapacity = pd.joniHealthBlue;
            };
            loadout.MenuCharmUpdate(run, () => HeroController.instance.CharmUpdate());
            PlayMakerFSM.OnBroadcast = null;
            Check(pd.health == 1 && pd.healthBlue == 1 && run.IsCharmEnabled(27), "Re-equipping Joni granted extra health");
            Check(blueRowCapacity == pd.joniHealthBlue && blueRowCapacity > 2,
                "Joni row rebuild changed its actual maximum capacity");
            loadout.MenuCharmUpdate(run, () => HeroController.instance.CharmUpdate());
            Check(pd.healthBlue == 1, "Repeated native menu refresh refilled blue masks");
            pd.equippedCharm_22 = true;
            loadout.MenuCharmUpdate(run, () => HeroController.instance.CharmUpdate());
            Check(run.IsCharmEnabled(22) && pd.equippedCharms.Contains(22), "Earned charm could not be re-equipped");
            int spell = run.SpellBonus;
            pd.equippedCharm_19 = false; loadout.MenuCharmUpdate(run, () => HeroController.instance.CharmUpdate());
            Check(run.SpellBonus == spell - 10 * Math.Max(0, run.Stack(Upgrade.ShamanStone) - 1), "Disabled Shaman Stone retained bonus ranks");
            Check(!run.SetCharmEnabled(24, true), "Menu enabled an unearned charm");
            var menuSave = ModHooks.Save();
            Check(menuSave.playerData.equippedCharms.SequenceEqual(new[] { 23, 32 }) && menuSave.playerData.gotCharm_32,
                "Menu changes leaked into the saved original loadout");
            run.Abandon(); loadout.Restore();
            foreach (var pair in originalAbilities) Check(Equals(typeof(PlayerData).GetField(pair.Key).GetValue(pd), pair.Value), "Exit lost ability/fragile status: " + pair.Key);
            Check(pd.health == 9 && pd.maxHealth == 11 && pd.healthBlue == 4, "Restore lost health or blue masks after native refresh");
            Check(shownHealth == 9 && shownMax == 11, "Original HUD maximum was not restored");
            Check(pd.equippedCharms.SequenceEqual(new[] { 23, 32 }) && pd.gotCharm_32 && pd.equippedCharm_23 && pd.hasCharm, "Restore lost original charm loadout");
            Check(pd.nailDamage == 21 && pd.nailSmithUpgrades == 4 && pd.heartPieces == 3 && pd.MPCharge == 77, "Restore lost original progression");
            Check(pd.unrelatedQuest == 72, "Restore rolled back unrelated progress");
            Check(ReferenceEquals(ModHooks.Save().playerData, pd), "Save protection remained active after restore");
            run.Start(12); loadout.Begin(run); pd.health = 0;
            int refreshes = HeroController.instance.RefreshCount;
            loadout.Restore();
            Check(pd.health == 0 && pd.maxHealthBase == 9 && pd.nailDamage == 21, "Death restoration revived the Knight or lost progression");
            Check(HeroController.instance.RefreshCount == refreshes, "Death restoration invoked native full heal");
            run.Defeat(); run.Start(12); pd.health = 9; loadout.Begin(run);
            var otherSave = new PlayerData { health = 7, maxHealthBase = 7, nailDamage = 17 };
            PlayerData.instance = otherSave;
            Check(ReferenceEquals(ModHooks.Save().playerData, otherSave), "Protection replaced a different save's player data");
            loadout.Restore();
            Check(otherSave.health == 7 && otherSave.nailDamage == 17, "Restoration changed another save");
            Check(pd.health == 9 && pd.nailDamage == 21, "Original owner was not restored after a profile switch");
            PlayerData.instance = pd;
        }
        Check(ModHooks.SubscriberCount == 0, "Unload left save hook installed");
        VerifyStartingClawRestoration(); VerifyFragileCharms(); VerifyJoniMilestones();
        Console.WriteLine($"PASS: {checks} real-adapter boundary assertions (temporary loadout, native healing, save clone, restoration, death, unload).");
    }
    private static void VerifyStartingClawRestoration()
    {
        var original = PlayerData.instance;
        var pd = new PlayerData { health = 5, maxHealth = 5, maxHealthBase = 5 };
        PlayerData.instance = pd;
        try
        {
            var run = new Run(); run.Start(1);
            using (var loadout = new RunLoadout())
            {
                loadout.Begin(run);
                Check(pd.hasWalljump && pd.canWallJump, "New trial did not grant Mantis Claw");
                var saved = ModHooks.Save().playerData;
                Check(!saved.hasWalljump && !saved.canWallJump, "Starting Mantis Claw leaked into an unearned original save");
                run.Abandon(); loadout.Restore();
                Check(!pd.hasWalljump && !pd.canWallJump, "Exit retained the trial's starting Mantis Claw");
            }
        }
        finally { PlayerData.instance = original; }
    }
    private static void VerifyJoniMilestones()
    {
        PlayerData.instance = new PlayerData { health = 5, maxHealth = 5, maxHealthBase = 5 };
        var pd = PlayerData.instance;
        var run = new Run(Enumerable.Range(1, 300).Select(i => new Wave(i, "Milestone", new EncounterEnemy(EnemyKind.BrokenVessel))));
        run.Start(12);
        using (var loadout = new RunLoadout())
        {
            loadout.Begin(run);
            while (run.Stack(Upgrade.Joni) == 0 && run.IsActive)
            {
                Clear(run); loadout.RepairFragileCharms(run);
                if (run.Phase != RunPhase.ChoosingCard) continue;
                var pick = run.Offer.OrderBy(c => c.Id == Upgrade.Joni ? 0 : 1).First();
                run.Choose(pick.Id); loadout.Apply(run);
            }
            Check(run.IsActive && pd.healthBlue == pd.joniHealthBlue - 1, "Acquiring Joni did not grant its initial conversion");
            while (run.Phase == RunPhase.ChoosingCard) { run.Choose(run.Offer[0].Id); loadout.Apply(run); }
            pd.healthBlue = 1;
            var stacked = new List<long> { run.WaveToken };
            for (int i = 0; i < 5; i++)
            { Check(run.CallNextWave(), "Five bell calls were not available in the refill fixture"); stacked.Add(run.WaveToken); }
            loadout.RepairFragileCharms(run); Check(pd.healthBlue == 1, "Calling waves refilled Joni without any clears");
            foreach (long token in stacked.AsEnumerable().Reverse())
            {
                int clearedBefore = run.ClearedWaveCount; pd.healthBlue = 1;
                Check(run.EnemyKilled(token, 0), "Stacked-wave callback rejected");
                loadout.RepairFragileCharms(run);
                Check(pd.healthBlue == (run.ClearedWaveCount / 5 > clearedBefore / 5 ? pd.joniHealthBlue - 1 : 1),
                    "Out-of-order stacked clears bypassed the five-wave refill count");
                while (run.Phase == RunPhase.ChoosingCard) { run.Choose(run.Offer[0].Id); loadout.Apply(run); }
            }
            for (int step = 0; step < 12; step++)
            {
                pd.healthBlue = 1;
                int cleared = run.ClearedWaveCount;
                while (run.ClearedWaveCount == cleared)
                {
                    if (run.Phase == RunPhase.ChoosingCard)
                    {
                        run.Choose(run.Offer[0].Id); loadout.Apply(run);
                        Check(pd.healthBlue == 1, "Card selection refilled Joni between milestones");
                    }
                    else Check(run.EnemyKilled(run.WaveToken, 0), "Milestone fixture kill rejected");
                }
                loadout.RepairFragileCharms(run);
                int expected = run.ClearedWaveCount % 5 == 0 ? pd.joniHealthBlue - 1 : 1;
                Check(pd.healthBlue == expected, "Joni refilled outside a multiple of five cleared waves");
                pd.healthBlue = 1; loadout.RepairFragileCharms(run);
                Check(pd.healthBlue == 1, "Repeated milestone repair granted a second refill");
            }
            run.Abandon();
        }
    }

    private static void VerifyFragileCharms()
    {
        var pd = PlayerData.instance;
        bool originalHeart = pd.brokenCharm_23, originalStrength = pd.brokenCharm_25;
        var run = new Run(Enumerable.Range(1, 250).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel)))); run.Start(12);
        using (var loadout = new RunLoadout())
        {
            loadout.Begin(run);
            while (run.IsActive && (run.Stack(Upgrade.Strength) == 0 || run.Stack(Upgrade.FragileHeart) == 0))
            {
                Clear(run); loadout.RepairFragileCharms(run);
                var pick = run.Offer.Where(c => c.Id != Upgrade.Joni)
                    .OrderBy(c => c.Id == Upgrade.Strength || c.Id == Upgrade.FragileHeart ? 0 : 1).FirstOrDefault();
                if (pick == null) run.Skip(); else run.Choose(pick.Id);
                loadout.Apply(run);
            }
            Check(run.IsActive && pd.equippedCharm_23 && pd.equippedCharm_25 && !pd.equippedCharm_27, "Fragile test failed to acquire both charms before Joni");
            while (run.Phase == RunPhase.ChoosingCard)
            { run.Choose(run.Offer.First(c => c.Id != Upgrade.Joni).Id); loadout.Apply(run); }
            int maximum = pd.maxHealthBase;
            pd.MPCharge = 42;
            Check(!loadout.DamageTaken(pd.health, pd.healthBlue) && !pd.brokenCharm_23 && !pd.brokenCharm_25,
                "A blocked/i-frame/zero-health-loss hit broke fragile charms");
            int previous = pd.health; pd.health--;
            int wounded = pd.health, refreshes = HeroController.instance.RefreshCount;
            int blueUpdates = PlayMakerFSM.Events.Count(e => e == "UPDATE BLUE HEALTH");
            Check(loadout.DamageTaken(previous, 0), "Actual mask damage did not break fragile charms");
            Check(pd.brokenCharm_23 && pd.brokenCharm_25 && pd.equippedCharm_23 && pd.equippedCharm_25,
                "Break did not set native broken flags or removed acquired boons");
            Check(pd.maxHealth == maximum && pd.health == Math.Min(wounded, maximum), "Fragile Heart retained bonus masks or healed the hit");
            Check(pd.MPCharge == 42 && HeroController.instance.RefreshCount == refreshes
                && PlayMakerFSM.Events.Count(e => e == "UPDATE BLUE HEALTH") == blueUpdates,
                "Breaking a charm refilled health/Soul or invoked native full healing");
            Check(PlayMakerFSM.Events.Contains("UPDATE NAIL DAMAGE"), "Strength's cached native nail damage was not refreshed");
            loadout.Apply(run);
            Check(pd.brokenCharm_23 && pd.brokenCharm_25 && pd.health == Math.Min(wounded, maximum),
                "Mid-wave level reward repaired fragile charms or healed the player");
            loadout.RepairFragileCharms(run);
            Check(pd.brokenCharm_23 && pd.brokenCharm_25, "Fragile charms repaired during combat");
            pd.health = 2;
            Check(!loadout.DamageTaken(3, 0) && pd.health == 2, "Repeated damage retriggered break effects");
            var save = ModHooks.Save();
            Check(save.playerData.brokenCharm_23 == originalHeart && save.playerData.brokenCharm_25 == originalStrength,
                "Trial charm breakage leaked into saved original charms");
            int completed = run.ClearedWaveCount;
            while (run.IsActive && run.ClearedWaveCount == completed)
            {
                if (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer.First(c => c.Id != Upgrade.Joni).Id);
                else Clear(run);
            }
            loadout.RepairFragileCharms(run);
            Check(!pd.brokenCharm_23 && !pd.brokenCharm_25,
                "Fragile charms were not repaired when the wave ended, before choosing a card");
            Check(pd.maxHealth == maximum + 2 && pd.health == 4, "Heart repair did not restore just its two bonus masks");
            refreshes = HeroController.instance.RefreshCount;
            loadout.RepairFragileCharms(run);
            Check(pd.health == 4 && HeroController.instance.RefreshCount == refreshes, "Repeated repair healed the player again");
            if (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer.First(c => c.Id != Upgrade.Joni).Id); loadout.Apply(run);
            Check(pd.health == 4, "Choosing/skipping after repair granted Heart's masks twice");
            while (run.IsActive && run.Stack(Upgrade.Joni) == 0)
            {
                Clear(run); loadout.RepairFragileCharms(run);
                var pick = run.Offer.OrderBy(c => c.Id == Upgrade.Joni ? 0 : 1).FirstOrDefault();
                if (pick == null) run.Skip(); else run.Choose(pick.Id);
                loadout.Apply(run);
            }
            Check(run.IsActive && pd.equippedCharm_27 && pd.healthBlue > 0, "Fragile/Joni test did not acquire blue masks");
            int blueBefore = pd.healthBlue; pd.healthBlue--;
            int blueAfterHit = pd.healthBlue;
            Check(loadout.DamageTaken(1, blueBefore), "Blue-mask damage did not break fragile charms");
            int joniMax = (int)(pd.maxHealthBase * 1.4f);
            loadout.Apply(run);
            Check(pd.brokenCharm_23 && pd.brokenCharm_25 && pd.healthBlue == Math.Min(blueAfterHit, joniMax - 1),
                "Mid-wave level reward refilled Joni or repaired broken charms");
            Check(pd.health == 1 && pd.maxHealth == 1 && pd.joniHealthBlue == joniMax
                && pd.healthBlue == Math.Min(blueAfterHit, joniMax - 1), "Heart break with Joni retained bonus blue capacity or healed damage");
            int completedBlue = run.ClearedWaveCount;
            while (run.IsActive && run.ClearedWaveCount == completedBlue)
            {
                if (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer.First(c => c.Id != Upgrade.Joni).Id);
                else Clear(run);
            }
            int beforeBlueRepair = pd.healthBlue;
            loadout.RepairFragileCharms(run);
            Check(!pd.brokenCharm_23 && !pd.brokenCharm_25
                && pd.healthBlue == (run.ClearedWaveCount % 5 == 0 ? pd.joniHealthBlue - 1 : beforeBlueRepair),
                "Heart repair bypassed the five-wave Joni refill schedule");
            pd.health = 0;
            Check(!loadout.DamageTaken(1, 0) && pd.health == 0, "Charm break handling revived a dying Knight");
            pd.brokenCharm_23 = pd.brokenCharm_25 = true;
            loadout.RepairFragileCharms(run);
            Check(pd.health == 0 && pd.brokenCharm_23 && pd.brokenCharm_25, "Simultaneous wave clear/death repaired and revived the Knight");
            pd.health = 1; run.Abandon(); loadout.Restore();
            Check(pd.brokenCharm_23 == originalHeart && pd.brokenCharm_25 == originalStrength,
                "Exit lost the original fragile charm state");
            Check(!loadout.DamageTaken(pd.health + 1, pd.healthBlue), "Break handling remained active outside a trial");
        }
        Check(ModHooks.SubscriberCount == 0, "Fragile testing leaked save protection");
    }
}
