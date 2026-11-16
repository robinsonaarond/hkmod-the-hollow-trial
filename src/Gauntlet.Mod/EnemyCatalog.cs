using System.Collections.Generic;
using Gauntlet.Core;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal static class EnemyCatalog
    {
        // Paths verified against the user's 1.5.78.11833 scene assets.
        private static readonly (EnemyKind kind, string scene, string path)[] Entries =
        {
            (EnemyKind.Crawlid, "Tutorial_01", "_Enemies/Crawler 1"),
            (EnemyKind.Vengefly, "Tutorial_01", "_Enemies/Buzzer"),
            (EnemyKind.Hornhead, "Crossroads_04", "_Enemies/Zombie Hornhead"),
            (EnemyKind.Aspid, "Crossroads_03", "Spitter"),
            (EnemyKind.PrimalAspid, "Deepnest_East_06", "Super Spitter"),
            (EnemyKind.Hopper, "Deepnest_East_06", "Hopper"),
            (EnemyKind.GreatHopper, "Deepnest_East_14b", "Giant Hopper"),
            (EnemyKind.GruzMother, "GG_Gruz_Mother", "_Enemies/Giant Fly"),
            (EnemyKind.MantisWarrior, "Fungus2_14", "Mantis (1)"),
            (EnemyKind.WingedSentry, "Ruins1_05", "Ruins Flying Sentry"),
            (EnemyKind.HeavySentry, "Ruins1_05", "Battle Scene v2/Wave 1/Ruins Sentry FatB"),
            (EnemyKind.BrokenVessel, "GG_Broken_Vessel", "Infected Knight")
        };
        internal static List<(string, string)> Preloads()
        {
            var result = new List<(string, string)>();
            foreach (var entry in Entries) result.Add((entry.scene, entry.path));
            result.AddRange(ArenaLayouts.Preloads());
            return result;
        }
        internal static Dictionary<EnemyKind, GameObject> Resolve(Dictionary<string, Dictionary<string, GameObject>> preloads)
        {
            var result = new Dictionary<EnemyKind, GameObject>();
            foreach (var entry in Entries)
            {
                if (!preloads.TryGetValue(entry.scene, out var scene) ||
                    !scene.TryGetValue(entry.path, out var prefab) || prefab == null) return null;
                if (entry.kind == EnemyKind.GruzMother) NativeBoss.Validate(prefab);
                if (entry.kind == EnemyKind.BrokenVessel) BrokenVessel.Validate(prefab);
                result.Add(entry.kind, prefab);
            }
            return result;
        }
        internal static string Name(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.MantisWarrior: return "Mantis Warrior";
                case EnemyKind.WingedSentry: return "Winged Sentry";
                case EnemyKind.HeavySentry: return "Heavy Sentry";
                case EnemyKind.BrokenVessel: return "Broken Vessel";
                case EnemyKind.GruzMother: return "Gruz Mother";
                case EnemyKind.Crawlid: return "Crawlid";
                case EnemyKind.Vengefly: return "Vengefly";
                case EnemyKind.Hopper: return "Hopper";
                case EnemyKind.GreatHopper: return "Great Hopper";
                case EnemyKind.Aspid: return "Aspid Hunter";
                case EnemyKind.PrimalAspid: return "Primal Aspid";
                default: return "Husk Hornhead";
            }
        }
    }
}
