using System;
using System.Collections.Generic;
using System.Linq;
using Gauntlet.Core;

internal static class ProgressionBalance
{
    internal static void Verify(Action<bool, string> check)
    {
        Simulate("Nail + masks", new[] { Upgrade.Nail, Upgrade.Mask, Upgrade.QuickSlash, Upgrade.Recovery, Upgrade.Longnail, Upgrade.Shell, Upgrade.Soul, Upgrade.Spell, Upgrade.ShamanStone, Upgrade.SpellTwister, Upgrade.SoulCatcher, Upgrade.QuickFocus, Upgrade.SteadyBody, Upgrade.Grubsong, Upgrade.LastStand }, check);
        Simulate("Spells + Soul", new[] { Upgrade.Spell, Upgrade.Soul, Upgrade.ShamanStone, Upgrade.SpellTwister, Upgrade.QuickFocus, Upgrade.Recovery, Upgrade.Mask, Upgrade.Shell, Upgrade.Nail, Upgrade.SoulCatcher, Upgrade.Grubsong, Upgrade.QuickSlash, Upgrade.Longnail, Upgrade.SteadyBody, Upgrade.LastStand }, check);
        // With the same eligibility, a one-rank bucket and an eight-rank bucket
        // should have comparable offer frequency. No rank is an extra lottery entry.
        int ore = 0, hive = 0;
        for (int seed = 0; seed < 10000; seed++)
        {
            var run = new Run(); run.Start(seed); Clear(run); run.Choose(Upgrade.Focus); Clear(run);
            if (run.Offer.Any(c => c.Id == Upgrade.Nail)) ore++;
            if (run.Offer.Any(c => c.Id == Upgrade.Hiveblood)) hive++;
        }
        Console.WriteLine($"BUCKET FREQUENCY at level 3, 10,000 seeds: Pale Ore {ore}; Hiveblood {hive}.");
        check(Math.Abs(ore - hive) < 150 && ore > 500 && hive > 500, "Rank count is biasing family frequency");
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
    private static void Simulate(string name, Upgrade[] priority, Action<bool, string> check)
    {
        const int seeds = 1000;
        int one = 0, two = 0, varied = 0;
        var nailRanks = new List<int>(); var spellRanks = new List<int>(); var breadth = new List<int>();
        for (int seed = 0; seed < seeds; seed++)
        {
            var run = new Run(); run.Start(seed);
            var previous = new HashSet<Upgrade>();
            int drafts = 0;
            while (drafts < 25)
            {
                int fresh = run.EligibleBuckets.Count(bucket => !previous.Contains(bucket.Id));
                Clear(run);
                check(run.Phase == RunPhase.ChoosingCard && run.Offer.Count == 3, "Full-length run lost a reward choice");
                check(run.Offer.Select(card => card.Id).Distinct().Count() == 3 && run.Offer.All(card => run.Stack(card.Id) < card.MaxStacks), "Draft contains duplicate or capped buckets");
                if (fresh >= 3 && drafts > 0) check(run.Offer.All(card => !previous.Contains(card.Id)), "A bucket repeated in consecutive offers despite alternatives");
                previous = new HashSet<Upgrade>(run.Offer.Select(c => c.Id));
                var essentials = name.StartsWith("Spells") ? new[] { Upgrade.Fireball, Upgrade.Focus, Upgrade.Dash } : new[] { Upgrade.Focus, Upgrade.Dash };
                var essential = run.Offer.FirstOrDefault(card => essentials.Contains(card.Id) && run.Stack(card.Id) == 0);
                var choice = essential ?? run.Offer.OrderBy(card => Array.IndexOf(priority, card.Id) < 0 ? 1000 : Array.IndexOf(priority, card.Id)).First();
                run.Choose(choice.Id); drafts++;
            }
            check(drafts == 25, "Boss finale should follow 25 choices");
            int capped = priority.Take(2).Count(id => run.Stack(id) == Cards.All.First(c => c.Id == id).MaxStacks);
            if (capped > 0) one++; if (capped == 2) two++;
            int distinct = Cards.All.Count(c => run.Stack(c.Id) > 0); breadth.Add(distinct);
            if (distinct >= 12) varied++;
            nailRanks.Add(run.Stack(Upgrade.Nail)); spellRanks.Add(run.Stack(Upgrade.Spell));
            run.Abandon(); check(!run.IsActive && run.CharmIds.Count == 0, "Bucket run did not clean up after first chapter");
        }
        int Median(List<int> values) => values.OrderBy(v => v).ElementAt(values.Count / 2);
        Console.WriteLine($"PROGRESSION {name}: {one}/{seeds} cap one target; {two}/{seeds} cap both. Median distinct families {Median(breadth)}, nail rank {Median(nailRanks)}, spell bonus rank {Median(spellRanks)}. {varied}/{seeds} choose at least 12 families.");
        check(varied >= 950, name + ": bucket drafts do not produce varied builds");
        check(nailRanks.Count(rank => rank >= 4) < 300, name + ": nail progression still crowds out variety");
    }
}
