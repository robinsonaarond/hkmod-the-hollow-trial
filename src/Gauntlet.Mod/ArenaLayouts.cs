using System;
using System.Collections.Generic;
using Gauntlet.Core;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class ArenaLayouts : IDisposable
    {
        private readonly Dictionary<PlatformKind, GameObject> templates;
        private readonly List<GameObject> placed = new List<GameObject>();
        private static readonly (PlatformKind kind, string path)[] Entries =
        {
            (PlatformKind.Small, "gg_plat_float_small"),
            (PlatformKind.Wide, "gg_plat_float_wider")
        };
        internal static List<(string, string)> Preloads()
        {
            var names = new List<(string, string)>();
            foreach (var entry in Entries) names.Add(("GG_Uumuu", entry.path));
            names.Add(("Room_Town_Stag_Station", "Station Bell/Bell"));
            return names;
        }
        internal static Dictionary<PlatformKind, GameObject> Resolve(Dictionary<string, Dictionary<string, GameObject>> preloads)
        {
            if (!preloads.TryGetValue("GG_Uumuu", out var scene)) return null;
            var result = new Dictionary<PlatformKind, GameObject>();
            foreach (var entry in Entries)
            {
                if (!scene.TryGetValue(entry.path, out var prefab) || prefab == null || prefab.GetComponent<BoxCollider2D>() == null) return null;
                result.Add(entry.kind, prefab);
            }
            if (!preloads.TryGetValue("Room_Town_Stag_Station", out var bells) ||
                !bells.TryGetValue("Station Bell/Bell", out var bell)) return null;
            result.Add(PlatformKind.Bell, bell);
            return result;
        }
        internal ArenaLayouts(Dictionary<PlatformKind, GameObject> prefabs) { templates = prefabs; }
        internal string Apply(int wave, float left, float right, float floor, float heroX, Action<string> report, float? respawnX = null)
        {
            Clear();
            var layout = StageLayouts.ForWave(wave);
            foreach (var slot in layout.Platforms)
            {
                var prefab = templates[slot.Kind]; var box = prefab.GetComponent<BoxCollider2D>();
                var scale = prefab.transform.lossyScale;
                float width = box.size.x * Mathf.Abs(scale.x);
                float x = (left + right) * 0.5f + slot.Offset;
                // Don't materialize a solid platform across the Knight or an
                // entrance seal. Omitting one for this wave is safer than moving him.
                if (x - width / 2 < left + 6 || x + width / 2 > right - 6 || Mathf.Abs(x - heroX) < width / 2 + 1.5f) continue;
                var go = UnityEngine.Object.Instantiate(prefab); go.SetActive(false);
                go.name = "Gauntlet.Platform." + placed.Count;
                go.transform.position = new Vector3(x - box.offset.x * scale.x,
                    floor + slot.Top - box.offset.y * scale.y - box.size.y * Mathf.Abs(scale.y) / 2, 0);
                placed.Add(go); go.SetActive(true);
            }
            Physics2D.SyncTransforms();
            report?.Invoke($"Wave {wave} stage: {layout.Name}; {placed.Count} safe platforms placed.");
            return layout.Name;
        }
        internal void Clear()
        {
            foreach (var go in placed)
                if (go != null) { go.SetActive(false); UnityEngine.Object.Destroy(go); }
            placed.Clear();
        }
        public void Dispose() => Clear();
    }
}
