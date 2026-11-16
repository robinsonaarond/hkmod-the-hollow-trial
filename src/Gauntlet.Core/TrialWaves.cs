using System.Collections.Generic;
using System.Linq;

namespace Gauntlet.Core
{
    public static class TrialWaves
    {
        private static EncounterEnemy H() => new EncounterEnemy(EnemyKind.Hornhead);
        private static EncounterEnemy A() => new EncounterEnemy(EnemyKind.Aspid);
        private static EncounterEnemy P() => new EncounterEnemy(EnemyKind.Hopper);
        private static EncounterEnemy M() => new EncounterEnemy(EnemyKind.MantisWarrior);
        private static EncounterEnemy R() => new EncounterEnemy(EnemyKind.WingedSentry);
        private static EncounterEnemy S() => new EncounterEnemy(EnemyKind.HeavySentry);
        private static EncounterEnemy V() => new EncounterEnemy(EnemyKind.PrimalAspid);
        private static EncounterEnemy G() => new EncounterEnemy(EnemyKind.GreatHopper);

        public static IReadOnlyList<Wave> Create()
        {
            var formations = new[]
            {
                new Wave(1, "First Steps", new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Crawlid)),
                new Wave(2, "A Stirring Sky", new EncounterEnemy(EnemyKind.Vengefly), new EncounterEnemy(EnemyKind.Crawlid)),
                new Wave(3, "Little Wings", new EncounterEnemy(EnemyKind.Vengefly), new EncounterEnemy(EnemyKind.Vengefly)),
                new Wave(4, "Along the Ground", new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Vengefly)),
                new Wave(5, "Learning to Hunt", new EncounterEnemy(EnemyKind.Vengefly), new EncounterEnemy(EnemyKind.Crawlid), new EncounterEnemy(EnemyKind.Vengefly)),
                // Opening: establish the build before introducing the first elite.
                new Wave(6, "Broken Vanguard", H(), P()),
                new Wave(7, "Wings and Fangs", A(), H(), A()),
                new Wave(8, "Restless Ground", P(), P(), A()),
                new Wave(9, "Crossfire", H(), A(), P(), A()),
                new Wave(10, "The First Gate", P(), A(), P()),
                // Middle ranks: mixed ground pressure and ranged support.
                new Wave(11, "Iron Procession", P(), H(), A(), H()),
                new Wave(12, "A Watchful Sky", A(), P(), A(), H()),
                new Wave(13, "The Hunting Ground", H(), P(), H(), P()),
                new Wave(14, "Broken Formation", A(), P(), A(), H(), A()),
                new Wave(15, "Stone and Wings", G(), A(), P(), A()),
                // Veteran ranks: longer formations test the selected synergies.
                new Wave(16, "The Long March", H(), P(), A(), H(), P()),
                new Wave(17, "The Feathered Storm", A(), P(), A(), H(), A()),
                new Wave(18, "Restless Sentinels", P(), H(), P(), A(), H()),
                new Wave(19, "A Closing Circle", H(), A(), P(), A(), H(), P()),
                new Wave(20, "The Relentless Guard", G(), A(), H(), P()),
                // Final ranks: builds can approach their caps before the last trial.
                new Wave(21, "The Pale Advance", P(), A(), H(), P(), A()),
                new Wave(22, "The Gathering Host", A(), H(), P(), A(), H(), P()),
                new Wave(23, "No Quiet Ground", H(), P(), A(), H(), P(), A()),
                new Wave(24, "Before the Crown", A(), P(), H(), A(), P(), H()),
                new Wave(25, "The Heavy Crown", G(), A(), H(), P(), G())
            };
            // The first chapter retains its exact reinforced formations.
            return formations.Select(Reinforce).Concat(new[]
            {
                new Wave(26, "The Mother's Crown", new EncounterEnemy(EnemyKind.GruzMother), new EncounterEnemy(EnemyKind.GruzMother), new EncounterEnemy(EnemyKind.GruzMother)),
                new Wave(27, "The Mantis Vanguard", M(), M(), V(), P(), M(), V()),
                new Wave(28, "Blades Above", R(), M(), V(), R(), P(), M()),
                new Wave(29, "Iron Watch", S(), M(), V(), S(), R(), M()),
                new Wave(30, "The Thorn Gate", G(), M(), V(), R(), G(), S()),
                new Wave(31, "Crossed Blades", M(), R(), M(), V(), S(), R(), M()),
                new Wave(32, "A Cruel Sky", R(), V(), R(), M(), V(), R(), S()),
                new Wave(33, "Armoured March", S(), M(), S(), R(), M(), V(), S()),
                new Wave(34, "Thorns and Talons", R(), G(), V(), M(), R(), S(), V()),
                new Wave(35, "The Iron Gate", G(), S(), M(), R(), G(), S(), V()),
                new Wave(36, "No Safe Quarter", M(), R(), S(), V(), M(), R(), S(), V()),
                new Wave(37, "The Cutting Wind", R(), V(), R(), M(), S(), R(), V(), M()),
                new Wave(38, "Steel Procession", S(), M(), S(), R(), V(), S(), M(), R()),
                new Wave(39, "The Thorn Crown", G(), R(), V(), S(), M(), R(), V(), S()),
                new Wave(40, "The Crushing Gate", G(), S(), G(), R(), V(), S(), M(), G()),
                new Wave(41, "The Last Legion", S(), M(), R(), V(), S(), M(), R(), V(), S()),
                new Wave(42, "Storm of Blades", R(), V(), M(), R(), S(), V(), R(), M(), S()),
                new Wave(43, "The Closing Vice", S(), G(), R(), M(), V(), S(), R(), G(), M()),
                new Wave(44, "A Field of Thorns", R(), S(), V(), G(), M(), R(), S(), V(), G()),
                new Wave(45, "The Final Gate", G(), S(), R(), V(), G(), S(), M(), R(), G()),
                new Wave(46, "Before the Fallen", S(), R(), G(), V(), M(), S(), R(), G(), V(), S()),
                new Wave(47, "The Broken Court", new EncounterEnemy(EnemyKind.BrokenVessel), new EncounterEnemy(EnemyKind.BrokenVessel))
            }).ToArray();
        }
        private static Wave Reinforce(Wave wave)
        {
            var enemies = wave.Enemies.Concat(wave.Enemies.Take((wave.EnemyCount + 1) / 2)).ToList();
            // The first elite is a single final arrival, never duplicated by the
            // reinforcement rule: three Hoppers, two Aspids, one Great Hopper.
            if (wave.Number == 10) enemies.Add(G());
            int aspid = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (wave.Number < 17 || enemies[i].Kind != EnemyKind.Aspid) continue;
                // Full Primal introduction, then alternate variants in arrival
                // order. A wave with only one Aspid slot gets a Primal.
                if (wave.Number == 17 || aspid++ % 2 == 0)
                    enemies[i] = new EncounterEnemy(EnemyKind.PrimalAspid);
            }
            return new Wave(wave.Number, wave.Title, enemies.ToArray());
        }
    }
}
