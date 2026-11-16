using System;
using System.Linq;
using System.Collections.Generic;
using Gauntlet.Core;

static class Program
{
    private static int checks;
    private static void Check(bool condition, string message)
    { checks++; if (!condition) throw new Exception(message); }
    private static void Throws(Action action)
    {
        try { action(); } catch (InvalidOperationException) { checks++; return; }
        throw new Exception("Expected an invalid operation to be rejected.");
    }
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
    private static void VerifyLevels()
    {
        Check(CharacterLevels.ForExperience(0) == 1 && CharacterLevels.ForExperience(9) == 1
            && CharacterLevels.ForExperience(10) == 2, "Starting level threshold is incorrect");
        for (int level = 2; level <= 20000; level++)
        {
            int threshold = checked((int)CharacterLevels.Threshold(level));
            Check(CharacterLevels.ForExperience(threshold - 1) == level - 1
                && CharacterLevels.ForExperience(threshold) == level
                && CharacterLevels.ForExperience(threshold + 1) == level, "Level threshold boundary rounded incorrectly");
            Check(CharacterLevels.Threshold(level + 1) - threshold == CharacterLevels.CostToNext(level), "Level costs do not follow the functional curve");
        }
        int maximum = CharacterLevels.ForExperience(int.MaxValue);
        Check(CharacterLevels.Threshold(maximum) <= int.MaxValue && CharacterLevels.Threshold(maximum + 1) > int.MaxValue,
            "Maximum integer XP overflowed level calculation");
        var run = new Run(); run.Start(3);
        int oneLevel = 0, previous = run.Level, choices = 0, kills = 0;
        for (int wave = 1; wave <= run.WaveCount; wave++)
        {
            Check(run.CurrentWave.Number == wave, "Automatic progression skipped a wave");
            long token = run.WaveToken; int count = run.CurrentWave.EnemyCount;
            for (int i = 0; i < count; i++)
            {
                Check(run.EnemyKilled(token, i), "Owned campaign kill rejected"); kills++;
                while (run.Phase == RunPhase.ChoosingCard)
                {
                    Check(run.Offer.All(c => c.Id != Upgrade.WallJump), "Starting Mantis Claw was offered as a card");
                    run.Choose(run.Offer[0].Id); choices++;
                }
            }
            int gained = run.Level - previous;
            Check(gained >= 0 && gained <= 2 && Math.Abs(run.Level - (wave + 1)) <= 1, "Unboosted campaign level balance changed");
            if (gained == 1) oneLevel++; previous = run.Level;
        }
        Check(oneLevel == 43 && run.Level == 48 && run.Experience == 11565 && kills == 317, "Wave roster or unboosted XP changed");
        Check(choices == 47 && run.Phase == RunPhase.Victory, "Each crossed level must award a choice, including finale XP");
        run.Start(4); Check(run.Level == 1 && run.Experience == 0 && run.Stack(Upgrade.WallJump) == 1, "Retry failed to reset XP or grant Mantis Claw");
    }
    private static void VerifyStacking()
    {
        var run = new Run(new[] {
            new Wave(1, "One", new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Vengefly)),
            new Wave(2, "Two", new EncounterEnemy(EnemyKind.GreatHopper), new EncounterEnemy(EnemyKind.Crawlid)),
            new Wave(3, "Final", new EncounterEnemy(EnemyKind.BrokenVessel)) });
        run.Start(1); long first = run.WaveToken;
        Check(run.CallNextWave(), "Bell did not advance during combat"); long second = run.WaveToken;
        Check(run.TemporaryXpBonusPercent == 40 && run.KillExperience(EnemyKind.Crawlid) == 7 && run.EnemiesRemaining == 4,
            "Bell did not add a global bonus or discarded living enemies");
        Check(run.EnemyKilled(first, 0) && run.Experience == 7 && run.Phase == RunPhase.Combat, "Existing mob missed the immediate bell boost");
        Check(!run.EnemyKilled(first, 0) && run.Experience == 7, "Duplicate kill granted boosted XP");
        Check(run.EnemyKilled(second, 0) && run.Experience == 91 && run.Phase == RunPhase.ChoosingCard && run.PendingRewards == 3
            && run.EnemiesRemaining == 2, "Mid-wave multi-level kill did not pause with one reward per level");
        Check(!run.CallNextWave(), "Bell ran while selecting cards");
        run.Skip(); Check(run.PermanentXpBonusPercent == 10 && run.TemporaryXpBonusPercent == 40 && run.PendingRewards == 2,
            "Skip did not add permanent XP or cleared temporary XP prematurely");
        run.Choose(run.Offer[0].Id); run.Choose(run.Offer[0].Id);
        Check(run.Phase == RunPhase.Combat && run.CurrentWave.Number == 2 && run.EnemiesRemaining == 2,
            "Reward consumption advanced an unfinished wave");
        Check(run.EnemyKilled(first, 1) && run.Experience == 106 && run.TemporaryXpBonusPercent == 0 && run.ClearedWaveCount == 1,
            "Clearing kill missed bonus or temporary bonus survived wave clear");
        Check(run.PermanentXpBonusPercent == 10 && run.KillExperience(EnemyKind.Crawlid) == 5, "Wave clear removed permanent XP");
        while (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer[0].Id);
        run.EnemyKilled(second, 1);
        Check(run.Experience == 111 && run.CurrentWave.Number == 3 && run.Phase == RunPhase.Combat, "Wave without a level granted a card or failed to advance");
        Check(!run.CallNextWave() && run.TemporaryXpBonusPercent == 0, "Unavailable bell granted a free bonus");
        run.EnemyKilled(run.WaveToken, 0);
        Check(run.Experience == 386 && run.Phase == RunPhase.ChoosingCard && run.PendingRewards == run.Level - 5,
            "Final kill lost queued level rewards");
        while (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer[0].Id);
        Check(run.Phase == RunPhase.Victory && run.TemporaryXpBonusPercent == 0 && run.PermanentXpBonusPercent == 0,
            "Final choice failed to enter victory or reset bonuses");
        run.Start(2); Check(!run.EnemyKilled(first, 1) && run.Experience == 0, "Restart accepted stale boosted kills");
        run.CallNextWave(); run.CallNextWave();
        Check(run.TemporaryXpBonusPercent == 80 && run.KillExperience(EnemyKind.Hornhead) == 27, "Bell bonuses did not stack additively");
        run.Defeat(); Check(!run.EnemyKilled(first, 0) && run.TemporaryXpBonusPercent == 0 && run.PermanentXpBonusPercent == 0,
            "Defeat accepted delayed kills or leaked XP boosts");
    }
    private static void VerifyImmediateRewards()
    {
        var run = new Run(new[] {
            new Wave(1, "Pressure", new EncounterEnemy(EnemyKind.BrokenVessel), new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Crawlid)),
            new Wave(2, "Reserve", new EncounterEnemy(EnemyKind.Crawlid)) });
        run.Start(1); long token = run.WaveToken;
        run.EnemyKilled(token, 0);
        Check(run.Level == 7 && run.PendingRewards == 6 && run.EnemiesRemaining == 2 && run.RewardLevel == 2,
            "Single kill did not queue all crossed levels mid-wave");
        run.Skip(); run.Skip();
        Check(run.PermanentXpBonusPercent == 20 && run.Experience == 250 && run.RewardLevel == 4,
            "Repeated skips failed to stack or changed already-earned XP");
        run.EnemyKilled(token, 1);
        Check(run.Experience == 256 && run.PendingRewards == 4 && run.Phase == RunPhase.ChoosingCard,
            "Same-frame death during reward pause was rejected or missed the new skip bonus");
        while (run.Phase == RunPhase.ChoosingCard) run.Choose(run.Offer[0].Id);
        run.EnemyKilled(token, 2);
        Check(run.Experience == 262 && run.PendingRewards == 0 && run.Phase == RunPhase.Combat
            && run.CurrentWave.Number == 2 && run.PermanentXpBonusPercent == 20,
            "Wave clear awarded a reward without a level or reset permanent bonus");
        run.Abandon(); run.Start(2);
        Check(run.TemporaryXpBonusPercent == 0 && run.PermanentXpBonusPercent == 0 && run.Level == 1,
            "Run reset retained XP boosts");
    }
    private static void VerifyWaveAnnouncements()
    {
        var notice = new WaveAnnouncement(); Check(!notice.Active, "Wave banner started unexpectedly");
        notice.Show(2, "Two"); notice.Advance(0.3f, false);
        Check(notice.Active && notice.Alpha == 1 && notice.Number == 2, "Wave banner did not become readable");
        float elapsed = notice.Elapsed; notice.Advance(5, true);
        Check(notice.Elapsed == elapsed, "Pause/card selection consumed the wave banner");
        notice.Show(6, "Stacked wave"); Check(notice.Number == 6 && notice.Elapsed == 0, "Rapid bell calls left an outdated wave banner");
        notice.Advance(WaveAnnouncement.Duration, false); Check(!notice.Active && notice.Alpha == 0, "Wave banner did not expire");
        notice.Show(7, "Seven"); notice.Cancel(); Check(!notice.Active && notice.Number == 0, "Wave banner survived run cleanup");
    }

    private static void VerifyThorns()
    {
        Check(Cards.All.Single(c => c.Id == Upgrade.Thorns).MaxStacks == 5, "Thorns must have five ranks");
        var run = new Run(new[] { new Wave(1, "Thorns ranks", Enumerable.Repeat(new EncounterEnemy(EnemyKind.BrokenVessel), 100).ToArray()) });
        run.Start(17);
        while (run.IsActive && run.Stack(Upgrade.Thorns) < 5)
        {
            Clear(run);
            while (run.Phase == RunPhase.ChoosingCard && run.Stack(Upgrade.Thorns) < 5)
            {
                if (run.Offer.Any(c => c.Id == Upgrade.Thorns))
                {
                    run.Choose(Upgrade.Thorns);
                    Check(run.ThornsBonus == 10 * (run.Stack(Upgrade.Thorns) - 1), "Thorns rank damage is wrong");
                }
                else run.Skip();
            }
        }
        Check(run.Stack(Upgrade.Thorns) == 5 && run.ThornsBonus == 40 && 5 + run.ThornsBonus >= 35,
            "Max thorns cannot kill a 35-HP Primal Aspid with a starter nail");
        Check(!run.Offer.Any(c => c.Id == Upgrade.Thorns), "Capped thorns remained in the draft");
        run.SetCharmEnabled(12, false); Check(run.ThornsBonus == 0, "Disabled thorns retained bonus damage");
        run.SetCharmEnabled(12, true); Check(run.ThornsBonus == 40, "Reequipping thorns lost its ranks");
        run.Abandon(); Check(run.ThornsBonus == 0, "Thorns damage leaked after run end");
    }

    private static void VerifySkipCampaign()
    {
        var run = new Run(); run.Start(23); int kills = 0, skips = 0;
        for (int wave = 1; wave <= run.WaveCount; wave++)
        {
            long token = run.WaveToken; var enemies = run.CurrentWave.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                int xp = run.Experience, award = run.KillExperience(enemies[i].Kind);
                Check(run.EnemyKilled(token, i) && run.Experience == xp + award, "All-skip campaign used spawn-time or retroactive XP"); kills++;
                while (run.Phase == RunPhase.ChoosingCard)
                {
                    int priorXp = run.Experience, bonus = run.PermanentXpBonusPercent;
                    run.Skip(); skips++;
                    Check(run.Experience == priorXp && (!run.IsActive || run.PermanentXpBonusPercent == bonus + 10),
                        "Skipping granted XP directly or failed to add its permanent bonus");
                }
            }
        }
        Check(run.Phase == RunPhase.Victory && kills == 317 && skips == run.Level - 1 && run.Experience > 11565,
            "Boosted campaign lost kills, rewards or victory");
        Console.WriteLine($"ALL-SKIP CAMPAIGN: {kills} kills, {skips} choices, level {run.Level}, XP {run.Experience}.");
    }
    private static void Main()
    {
        var celebration = new VictoryTransition();
        Check(!celebration.Advance(10, false, true) && !celebration.Confirm(), "Inactive victory can return");
        celebration.Begin();
        Check(celebration.Active && !celebration.ShowingScreen && !celebration.CanConfirm, "Victory hid the boss death immediately");
        Check(!celebration.Advance(20, false, false) && !celebration.Confirm(), "Native boss death was skipped by time or input");
        Check(!celebration.Advance(20, true, true) && !celebration.ShowingScreen, "Pause revealed victory");
        Check(celebration.Advance(0, false, true) && celebration.ShowingScreen && !celebration.CanConfirm, "Completed native death did not reveal victory");
        Check(!celebration.Advance(0.25f, false, true) && !celebration.Confirm(), "Victory can be dismissed during its reveal");
        Check(!celebration.Advance(10, true, true) && celebration.Elapsed == 0.25f, "Pause consumed reveal time");
        celebration.Advance(0.25f, false, true);
        Check(celebration.CanConfirm && celebration.Active, "Victory never enabled confirmation");
        celebration.Advance(10000, false, true);
        Check(celebration.Active && celebration.ShowingScreen, "Victory still auto-returns on a timer");
        Check(celebration.Confirm() && !celebration.Confirm(), "Victory did not consume input once");
        celebration.Begin(); celebration.Cancel();
        Check(!celebration.Active && !celebration.Confirm(), "Exit left victory input active");
        Check(RewardBuckets.All.Count == Cards.All.Count && RewardBuckets.All.Select(b => b.Id).Distinct().Count() == Cards.All.Count, "Upgrade family has duplicate buckets");
        Check(RewardBuckets.All.Single(b => b.Id == Upgrade.Nail).RankCount == 8 && RewardBuckets.All.Single(b => b.Id == Upgrade.Hiveblood).RankCount == 1, "Rank counts were not retained inside buckets");
        for (int wave = 1; wave <= 26; wave++)
        {
            var layout = StageLayouts.ForWave(wave);
            Check(layout.Platforms.All(p => p.Top >= 3.5f && p.Top <= 4.5f && Math.Abs(p.Offset) <= 7), "Layout exceeds basic-jump/central-space limits");
            if (wave <= 5 || wave % 5 == 0 || wave == 26) Check(layout.Platforms.Count == 0, "Tutorial, elite gate or boss layout blocks the open floor");
        }
        Check(Enumerable.Range(6, 5).Select(w => StageLayouts.ForWave(w).Name).Distinct().Count() == 5, "Stage layouts do not vary between waves");
        var reward = new RewardTransition();
        Check(!reward.CanChoose && !reward.ShowingLevelUp, "Inactive reward can be selected");
        Check(RewardTransition.LevelUpSeconds == 1f && RewardTransition.CardRiseSeconds == 0.5f && RewardTransition.TotalSeconds == 1.5f, "Reward transition exceeds requested timing");
        reward.Begin();
        Check(reward.ShowingLevelUp && reward.CardProgress == 0 && !reward.CanChoose, "Level-up cue missing");
        reward.Advance(0.25f, false);
        Check(reward.ShowingLevelUp && !reward.CanChoose, "Cue ended early");
        reward.Advance(10, true);
        Check(reward.Elapsed == 0.25f, "Pause advanced reward animation");
        reward.Advance(0.25f, false);
        Check(reward.ShowingLevelUp && !reward.CanChoose, "Level-up cue must remain visible for a full second");
        reward.Advance(0.5f, false);
        Check(!reward.ShowingLevelUp && reward.CardProgress == 0 && !reward.CanChoose, "Cards must begin after cue");
        reward.Advance(0.25f, false);
        Check(reward.CardProgress == 0.5f && !reward.CanChoose, "Cards selectable mid-animation");
        reward.Advance(0.25f, false);
        Check(reward.CardProgress == 1 && reward.CanChoose, "Cards not ready at 1.5 seconds");
        reward.Advance(10, false);
        Check(reward.Elapsed == 1.5f, "Transition extended past 1.5 seconds");
        reward.Cancel();
        Check(!reward.CanChoose && !reward.ShowingLevelUp && reward.CardProgress == 0, "End/exit leaked reward state");
        reward.Begin();
        Check(reward.Elapsed == 0 && !reward.CanChoose, "Next level reused unlocked input");
        var live = new Dictionary<string, object>
        {
            ["health"] = 9, ["maxHealthBase"] = 9, ["heartPieces"] = 3,
            ["nailDamage"] = 21, ["equippedCharm_32"] = true,
            ["equippedCharms"] = new List<int> { 23, 32 }, ["completionPercent"] = 112f,
            ["unrelatedQuest"] = false
        };
        var backup = new LoadoutSnapshot(live.Where(pair => pair.Key != "unrelatedQuest"));
        live["health"] = 5; live["maxHealthBase"] = 5; live["nailDamage"] = 5;
        live["equippedCharm_32"] = false; live["unrelatedQuest"] = true;
        ((List<int>)live["equippedCharms"]).Clear();
        ((List<int>)live["equippedCharms"]).Add(18);
        var saveCopy = new Dictionary<string, object>(live);
        backup.Apply((name, value) => saveCopy[name] = value);
        Check((int)saveCopy["health"] == 9 && (int)saveCopy["nailDamage"] == 21 && (int)saveCopy["heartPieces"] == 3, "Save copy lost original progression");
        Check((int)live["health"] == 5 && (int)live["nailDamage"] == 5, "Saving interrupted the active run loadout");
        Check(((List<int>)saveCopy["equippedCharms"]).SequenceEqual(new[] { 23, 32 }), "Snapshot aliased the live charm list");
        Check((bool)saveCopy["equippedCharm_32"] && (float)saveCopy["completionPercent"] == 112f, "Save copy lost original charm/completion state");
        Check((bool)saveCopy["unrelatedQuest"], "Save protection rolled back unrelated progress");
        ((List<int>)saveCopy["equippedCharms"]).Add(19);
        backup.Apply((name, value) => live[name] = value);
        Check(((List<int>)live["equippedCharms"]).SequenceEqual(new[] { 23, 32 }), "Save-copy mutation leaked into restore");
        Check((int)live["health"] == 9 && (int)live["maxHealthBase"] == 9 && (bool)live["unrelatedQuest"], "End-of-run restoration failed");
        var charms = Cards.All.Where(card => card.CharmId != 0).ToArray();
        Check(charms.Length >= 25 && charms.Any(card => card.Id == Upgrade.ShamanStone && card.MaxStacks == 5), "Invalid starter charm pool");
        Check(charms.Select(card => card.CharmId).Distinct().Count() == charms.Length, "Duplicate charm IDs");
        var progression = new Run(Enumerable.Range(1, 250).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel))));
        progression.Start(22);
        Check(progression.MaximumMasks == 5 && progression.BaseNailDamage == 5 && progression.NailTier == 0 && progression.CharmIds.Count == 0, "Run did not start from a basic Knight");
        int rewardsTaken = 0;
        while (progression.IsActive)
        {
            Clear(progression);
            if (!progression.IsActive) break;
            if (progression.Offer.Count > 0)
            { progression.Choose(progression.Offer.OrderBy(c => c.Id == Upgrade.Joni ? 1 : 0).First().Id); rewardsTaken++; }
            else
            {
                Check(progression.MaximumMasks == 9 && progression.BaseNailDamage == 37 && progression.NailTier == 8, "Progression caps do not match native upgrades");
                Check(progression.CharmIds.Count == charms.Length, "Not all selected charms are active");
                progression.Skip();
            }
        }
        Check(rewardsTaken == Cards.All.Sum(card => card.MaxStacks), "Cards were awarded after their cap");
        Check(progression.CharmIds.Count == 0, "Charm boons leaked after victory");
        progression.Start(22);
        Check(progression.MaximumMasks == 5 && progression.BaseNailDamage == 5 && progression.CharmIds.Count == 0, "Restart retained prior build");
        var firstDraft = new Run(); firstDraft.Start(99); Clear(firstDraft);
        Check(firstDraft.Offer.Select(c => c.Id).SequenceEqual(new[] { Upgrade.Focus, Upgrade.Dash, Upgrade.Fireball }), "Opening needs a dependable core ability choice");
        Check(!firstDraft.CanFocus, "Baseline Knight can heal");
        firstDraft.Choose(Upgrade.Focus); Check(firstDraft.CanFocus, "Focus card did not enable healing");
        var countdown = new TrialCountdown();
        Check(!countdown.Advance(true, 10), "Inactive countdown started a run");
        countdown.Queue();
        Check(!countdown.Advance(false, 20) && countdown.Remaining == 3, "Loading or pause consumed countdown");
        Check(!countdown.Advance(true, 1) && countdown.Remaining == 2, "Countdown advanced incorrectly");
        Check(!countdown.Advance(false, 20) && countdown.Remaining == 2, "Pause consumed remaining time");
        Check(!countdown.Advance(true, 1) && countdown.Remaining == 1, "Countdown skipped a second");
        Check(countdown.Advance(true, 1) && !countdown.Pending, "Countdown did not start trial after three ready seconds");
        Check(!countdown.Advance(true, 10), "Countdown retriggered start");
        countdown.Queue(); countdown.Cancel();
        Check(!countdown.Advance(true, 10), "Cancelled/left scene countdown still started");
        countdown.Queue();
        Check(countdown.Advance(true, 4), "Retry countdown cannot complete");
        // Crowded entrances must continue emitting arrivals.
        for (int i = 0; i < 100; i++)
        {
            float x = SpawnSafety.Pick(3, 37, i);
            Check(x == (i % 2 == 0 ? 7 : 33), "Arrivals must alternate even when portals are occupied");
        }
        Check(SpawnSafety.FitHeight(29.2f, 27.2f, 31.2f, 28, 38) > 30,
            "Large enemy still intersects the floor");
        Check(SpawnSafety.FitHeight(37, 35, 39, 28, 38) < 36,
            "Flying enemy intersects the lowered roof");
        int[] baseSizes = { 2, 2, 2, 3, 3, 2, 3, 3, 4, 3, 4, 4, 4, 5, 4, 5, 5, 5, 6, 4, 5, 6, 6, 6, 5 };
        var campaign = TrialWaves.Create();
        Check(campaign.Count == 47, "Boss finale missing");
        for (int i = 0; i < baseSizes.Length; i++)
        {
            Check(campaign[i].EnemyCount == (baseSizes[i] * 3 + 1) / 2 + (i == 9 ? 1 : 0), "Wave did not gain 50% rounded-up reinforcements");
            Check(campaign[i].Enemies.All(e => e.Kind != EnemyKind.GruzMother && e.Health == 0), "Regular wave gained a boss or non-native health");
        }
        Check(campaign[9].Enemies.Count(e => e.Kind == EnemyKind.GreatHopper) == 1
            && campaign[9].Enemies.Count(e => e.Kind == EnemyKind.Hopper) == 3
            && campaign[9].Enemies.Count(e => e.Kind == EnemyKind.Aspid) == 2, "Wave 10 must have one elite, three Hoppers and two Aspids");
        Check(campaign.Take(16).SelectMany(w => w.Enemies).All(e => e.Kind != EnemyKind.PrimalAspid), "Primal Aspids appeared before wave 17");
        Check(campaign[16].Enemies.Count(e => e.Kind == EnemyKind.PrimalAspid) == 5
            && campaign[16].Enemies.All(e => e.Kind != EnemyKind.Aspid), "Wave 17 must replace all five Aspids with Primals");
        foreach (var wave in campaign.Skip(17).Take(8))
        {
            var aspids = wave.Enemies.Where(e => e.Kind == EnemyKind.Aspid || e.Kind == EnemyKind.PrimalAspid).ToArray();
            Check(aspids.Length > 0 && aspids.Where((e, index) => e.Kind != (index % 2 == 0 ? EnemyKind.PrimalAspid : EnemyKind.Aspid)).Count() == 0, "Late waves should alternate Primal and regular Aspids");
        }
        Check(campaign[25].Enemies.Count == 3 && campaign[25].Enemies.All(e => e.Kind == EnemyKind.GruzMother && e.Health == 0), "Wave 26 must contain three native-health Gruz Mothers");
        Check(campaign.Skip(26).Take(20).All(w => w.Enemies.Any(e => e.Kind == EnemyKind.MantisWarrior || e.Kind == EnemyKind.WingedSentry || e.Kind == EnemyKind.HeavySentry)), "New chapter lacks advanced enemies");
        Check(campaign[46].EnemyCount == 2 && campaign[46].Enemies.All(e => e.Kind == EnemyKind.BrokenVessel), "Fallback finale must contain two Broken Vessels");
        Check(StageLayouts.SpikesForWave(26).Length == 0 && StageLayouts.SpikesForWave(27).Length == 0 && StageLayouts.SpikesForWave(46).Length == 0 && StageLayouts.SpikesForWave(47).Length == 0, "Spikes leaked into the opening chapter or boss finale");
        VerifyStacking(); VerifyLevels(); VerifyImmediateRewards(); VerifySkipCampaign(); VerifyThorns(); VerifyWaveAnnouncements();
        var roster = new Run();
        roster.Start(1);
        Check(roster.CurrentWave.Enemies.All(e => e.Kind == EnemyKind.Crawlid), "Opening wave needs varied threats");
        Check(TrialWaves.Create().Take(5).SelectMany(w => w.Enemies).All(e => e.Kind == EnemyKind.Crawlid || e.Kind == EnemyKind.Vengefly), "Tutorial contains advanced enemies");
        Check(TrialWaves.Create()[5].Title == "Broken Vanguard" && TrialWaves.Create()[24].Title == "The Heavy Crown", "Original formations were not retained after tutorial");
        Check(roster.WaveCount == 47, "Default trial must have 47 waves");
        // Full roster progression is verified kill-by-kill in VerifyLevels.
        Check(SpawnSafety.WarningSeconds == 0.2f, "Arrival warning must match the requested 200 ms");
        var run = new Run(); run.Start(42);
        Throws(() => run.Start(42));
        Throws(() => run.Choose(Upgrade.Nail));
        Check(!run.EnemyKilled(run.WaveToken, 99), "Unknown enemy counted");
        long oldToken = run.WaveToken;
        Check(run.EnemyKilled(oldToken, 0), "First kill missing");
        Check(!run.EnemyKilled(oldToken, 0), "Duplicate kill counted");
        run.EnemyKilled(oldToken, 1);
        Check(run.Phase == RunPhase.ChoosingCard && run.EnemiesRemaining == 1, "Level-up must pause before the wave is cleared");
        run.EnemyKilled(oldToken, 2);
        Check(run.Phase == RunPhase.ChoosingCard, "Expected draft");
        Check(run.Offer.Count == 3 && run.Offer.Select(c => c.Id).Distinct().Count() == 3, "Bad draft");
        var outsideOffer = Cards.All.First(c => !run.Offer.Any(o => o.Id == c.Id));
        Throws(() => run.Choose(outsideOffer.Id));
        Upgrade chosen = run.Offer[0].Id; run.Choose(chosen);
        Check(run.Stack(chosen) == 1, "Upgrade not applied");
        Check(!run.EnemyKilled(oldToken, 0), "Stale wave event counted");
        while (run.IsActive)
        {
            if (run.Phase == RunPhase.Combat) Clear(run);
            else run.Choose(run.Offer[0].Id);
        }
        Check(run.Phase == RunPhase.Victory, "Final wave did not win");
        Check(Cards.All.All(c => run.Stack(c.Id) == 0), "Victory leaked upgrades");
        Check(run.NailBonus(1) == 0 && run.SpellBonus == 0 && run.SoulBonus == 0, "Inactive bonuses leaked");
        long previousRunToken = run.WaveToken;
        run.Start(42);
        Check(!run.EnemyKilled(previousRunToken, 0), "Stale run event counted");
        run.Defeat(); Check(run.Phase == RunPhase.Defeat, "Death not terminal");
        run.Start(42); run.Abandon(); Check(run.Phase == RunPhase.Abandoned, "Abort not terminal");
        run.Start(42); Clear(run); run.Defeat();
        Check(run.Offer.Count == 0, "Death left a draft open");

        // Broad seeded runs exercise caps, repeatability, all effects and exhaustion.
        for (int seed = -100; seed < 100; seed++)
        {
            var a = new Run(Enumerable.Range(1, 25).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel))));
            var b = new Run(Enumerable.Range(1, 25).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel))));
            a.Start(seed); b.Start(seed);
            while (a.IsActive)
            {
                Clear(a); Clear(b);
                if (!a.IsActive) break;
                Check(a.Offer.Select(c => c.Id).SequenceEqual(b.Offer.Select(c => c.Id)), "Seed not repeatable");
                Check(a.Offer.All(c => a.Stack(c.Id) < c.MaxStacks), "Capped card offered");
                if (a.Offer.Count == 0) { a.Skip(); b.Skip(); continue; }
                var card = a.Offer[0]; a.Choose(card.Id); b.Choose(card.Id);
                if (!a.IsActive) continue;
                Check(a.BaseNailDamage == 5 + 4 * a.Stack(Upgrade.Nail) && a.NailTier == a.Stack(Upgrade.Nail), "Native nail tier stacking wrong");
                Check(a.NailBonus(3) == 0, "Nail tier is being counted twice");
                Check(a.MaximumMasks == 5 + a.Stack(Upgrade.Mask), "Mask vessel stacking wrong");
                Check(a.CharmIds.All(id => Cards.All.Any(c => c.CharmId == id && a.Stack(c.Id) >= 1)), "Unselected charm boon active");
                Check(a.NailBonus(1) == a.NailBonus(3) + 15 * a.Stack(Upgrade.LastStand), "Last stand wrong");
                Check(a.SoulBonus == 3 * a.Stack(Upgrade.Soul), "Soul stacking wrong");
                Check(a.SpellBonus == 5 * a.Stack(Upgrade.Spell) + 10 * Math.Max(0, a.Stack(Upgrade.ShamanStone) - 1), "Spell stacking wrong");
                Check(a.Recovery == a.Stack(Upgrade.Recovery), "Recovery stacking wrong");
                int shell = a.Stack(Upgrade.Shell);
                if (shell > 0)
                {
                    // Fresh run per interval so acquisition history doesn't affect the counter.
                    var shield = new Run(Enumerable.Range(1, 20).Select(i => new Wave(i, "Reward fixture", new EncounterEnemy(EnemyKind.BrokenVessel))));
                    shield.Start(seed);
                    while (shield.IsActive && shield.Stack(Upgrade.Shell) < shell)
                    {
                        Clear(shield);
                        if (shield.Phase != RunPhase.ChoosingCard) break;
                        if (shield.Offer.Any(c => c.Id == Upgrade.Shell)) shield.Choose(Upgrade.Shell);
                        else shield.Skip();
                    }
                    while (shield.Phase == RunPhase.ChoosingCard)
                    {
                        var other = shield.Offer.FirstOrDefault(c => c.Id != Upgrade.Shell);
                        if (other == null) shield.Skip(); else shield.Choose(other.Id);
                    }
                    if (shield.Phase == RunPhase.Combat && shield.Stack(Upgrade.Shell) == shell)
                    {
                        Check(shield.IncomingDamage(0) == 0, "Zero damage counted");
                        for (int hit = 1; hit <= 2 * (7 - shell); hit++)
                            Check(shield.IncomingDamage(2) == (hit % (7 - shell) == 0 ? 0 : 2), "Shell interval wrong");
                        shield.Abandon();
                        Check(shield.IncomingDamage(2) == 2, "Shell leaked after abort");
                    }
                }
            }
        }
        ProgressionBalance.Verify(Check);
        Console.WriteLine($"PASS: {checks:N0} assertions (lifecycle, drafts, caps, effects, seeded runs, roster, safe arrivals).");
    }
}
