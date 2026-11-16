using System;
using System.Collections.Generic;
using Gauntlet.Core;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class InventoryCardIcons
    {
        // These are the actual sprites referenced by the native inventory's
        // spell FSMs, InvNailSprite tiers, and nail-art displays. Resolve loaded
        // assets without opening the inventory or changing the player's gear.
        private static readonly Dictionary<Upgrade, string[]> Names = new Dictionary<Upgrade, string[]>
        {
            [Upgrade.Fireball] = new[] { "Inv_0025_spell_fireball_01", "Inv_0025_spell_fireball_01_level2" },
            [Upgrade.Quake] = new[] { "Inv_0026_spell_quake_01", "Inv_0026_spell_quake_01_level2" },
            [Upgrade.Scream] = new[] { "Inv_0024_spell_scream_01", "Inv_0024_spell_scream_01_level2" },
            [Upgrade.Focus] = new[] { "Inv_0029_spell_core" },
            [Upgrade.Cyclone] = new[] { "Inv_0023_inv_whirlwind_skill" },
            [Upgrade.DashSlash] = new[] { "Inv_0021_inv_dash_strike_skill" },
            [Upgrade.GreatSlash] = new[] { "Inv_0022_inv_uppercut_skill" },
            // A card previews the NEXT rank. Old Nail is already the baseline;
            // ranks beyond the native four upgrades retain Pure Nail artwork.
            [Upgrade.Nail] = new[] { "nail_upgrade_0002_sharpened_nail", "nail_upgrade_0001_channel-nail",
                "nail_upgrade_03_coil_nail", "nail_upgrade_0000_pure-nail" }
        };
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        internal void Refresh()
        {
            sprites.Clear();
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                if (sprite != null) sprites[sprite.name] = sprite;
        }

        internal Sprite Get(Upgrade upgrade, int currentRank)
        {
            if (!Names.TryGetValue(upgrade, out var names)) return null;
            var name = names[Math.Max(0, Math.Min(currentRank, names.Length - 1))];
            return sprites.TryGetValue(name, out var sprite) ? sprite : null;
        }

        internal IEnumerable<Sprite> All(Action<string> report)
        {
            foreach (var entry in Names)
                foreach (string name in entry.Value)
                    if (sprites.TryGetValue(name, out var sprite)) yield return sprite;
                    else report?.Invoke("Inventory card artwork missing: " + name);
        }

        internal static string Title(Card card, int currentRank)
        {
            if (currentRank > 0)
                switch (card.Id)
                {
                    case Upgrade.Fireball: return "Shade Soul";
                    case Upgrade.Quake: return "Descending Dark";
                    case Upgrade.Scream: return "Abyss Shriek";
                }
            return card.Title;
        }
    }
}
