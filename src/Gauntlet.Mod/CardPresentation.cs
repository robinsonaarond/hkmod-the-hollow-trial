using System;
using System.IO;
using System.Linq;
using Gauntlet.Core;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class CardPresentation : IDisposable
    {
        private readonly Texture2D frame;
        private readonly CardIconCache icons = new CardIconCache();
        private readonly InventoryCardIcons inventoryIcons = new InventoryCardIcons();
        private Font font;
        private float opacity = 1f;
        private static readonly Color Ivory = new Color(0.93f, 0.91f, 0.85f);
        private static readonly Color Gold = new Color(0.88f, 0.73f, 0.45f);
        internal CardPresentation()
        {
            using (var source = typeof(CardPresentation).Assembly.GetManifestResourceStream("Gauntlet.card-frame.png"))
            using (var bytes = new MemoryStream())
            {
                if (source == null) throw new InvalidOperationException("Embedded card artwork missing.");
                source.CopyTo(bytes);
                frame = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!frame.LoadImage(bytes.ToArray())) throw new InvalidOperationException("Could not decode card artwork.");
                frame.filterMode = FilterMode.Bilinear;
            }
        }
        internal void PrepareIcons(Action<string> report)
        {
            inventoryIcons.Refresh();
            int ready = 0, missing = 0;
            if (CharmIconList.Instance != null)
                foreach (int id in Cards.All.Where(card => card.CharmId != 0).Select(card => card.CharmId).Distinct())
                {
                    if (icons.Get(CharmIconList.Instance.GetSprite(id)) != null) ready++;
                    else missing++;
                }
            report?.Invoke($"Card artwork: {ready} native charm icons ready; {missing} using fallback emblems.");
            ready = 0;
            foreach (var sprite in inventoryIcons.All(report))
            {
                if (icons.Get(sprite) != null) ready++;
            }
            report?.Invoke($"Card artwork: {ready} native inventory icons ready.");
        }
        // Returns selected card index, -2 to skip, or -1 for no action.
        internal int Draw(Run run, ref int selected, bool allowInput, float progress)
        {
            var priorColor = GUI.color;
            bool priorEnabled = GUI.enabled;
            try
            {
                float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(progress), 3f);
                opacity = 1f;
                Fill(new Rect(0, 0, 960, 540), new Color(0.015f, 0.02f, 0.035f, 0.25f + 0.69f * eased));
                opacity = eased;
                if (font == null) font = Resources.FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name.IndexOf("perpetua", StringComparison.OrdinalIgnoreCase) >= 0);
                Text(new Rect(150, 24, 660, 40), "A GIFT FROM THE VOID", 29, Ivory);
                string subtitle = $"Level {run.RewardLevel} · XP +{run.TemporaryXpBonusPercent}% temporary / +{run.PermanentXpBonusPercent}% permanent";
                Text(new Rect(150, 65, 660, 25), subtitle, 16, Gold);
                GUI.enabled = priorEnabled && allowInput;
                for (int i = 0; i < run.Offer.Count; i++)
                {
                    var card = run.Offer[i];
                    bool focused = i == selected;
                    var rect = new Rect(80 + i * 275, (focused ? 104 : 112) + (1f - eased) * 130f, 250, 375);
                    if (focused)
                    {
                        Fill(new Rect(rect.x - 4, rect.y - 4, rect.width + 8, rect.height + 8), new Color(0.7f, 0.58f, 0.34f, 0.65f));
                        Fill(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), new Color(0.95f, 0.88f, 0.65f, 0.85f));
                    }
                    GUI.color = Tint(focused ? Color.white : new Color(0.78f, 0.8f, 0.86f));
                    GUI.DrawTexture(rect, frame, ScaleMode.StretchToFill);
                    GUI.color = Tint(Color.white);
                    string title = InventoryCardIcons.Title(card, run.Stack(card.Id));
                    Text(new Rect(rect.x + 20, rect.y + 85, 210, 40), title, title.Length > 15 ? 20 : 24, Ivory);
                    var iconRect = card.Id == Upgrade.Nail ? new Rect(rect.center.x - 40, rect.y + 128, 80, 80)
                        : new Rect(rect.center.x - 32, rect.y + 133, 64, 64);
                    if (!DrawIcon(card, run.Stack(card.Id), iconRect))
                        DrawEmblem(card.Id, new Vector2(rect.center.x, rect.y + 165), focused ? Gold : Ivory);
                    Text(new Rect(rect.x + 28, rect.y + 210, 194, 73), card.Description, 18, Ivory);
                    string stacks = string.Join(card.MaxStacks > 5 ? " " : "   ", Enumerable.Range(1, card.MaxStacks)
                        .Select(n => n <= run.Stack(card.Id) + 1 ? "◆" : "◇").ToArray());
                    Text(new Rect(rect.x + 25, rect.y + 290, 200, 27), stacks, 17, Gold);
                    Text(new Rect(rect.x + 25, rect.y + 320, 200, 25), "RANK " + (run.Stack(card.Id) + 1) + " / " + card.MaxStacks, 13, Ivory);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) return i;
                }
                bool skipFocused = selected == run.Offer.Count;
                if (skipFocused) Fill(new Rect(325, 489, 310, 29), new Color(0.4f, 0.32f, 0.17f, 0.95f));
                var skipStyle = Style(skipFocused ? 18 : 15, skipFocused ? Ivory : Gold);
                if (GUI.Button(new Rect(345, 491, 270, 27), "SKIP  ·  +10% XP  ·  TAB", skipStyle)) return -2;
                Text(new Rect(100, 519, 760, 20), "Left / Right: cards   ·   Down: skip   ·   Up: cards   ·   Confirm: accept   ·   F7 to return to the hall", 12, Ivory);
                return -1;
            }
            finally { opacity = 1f; GUI.color = priorColor; GUI.enabled = priorEnabled; }
        }
        internal void DrawVictory(int waveCount, float elapsed)
        {
            var priorColor = GUI.color;
            try
            {
                if (font == null) font = Resources.FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name.IndexOf("perpetua", StringComparison.OrdinalIgnoreCase) >= 0);
                float arrival = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 0.5f));
                Fill(new Rect(0, 0, 960, 540), new Color(0.015f, 0.02f, 0.035f, 0.94f * arrival));
                opacity = arrival;
                float extent = Mathf.Lerp(90, 285, arrival);
                Line(new Vector2(480 - extent, 175), new Vector2(480 + extent, 175), Gold, 1);
                Circle(new Vector2(480, 130), 16, Gold);
                Text(new Rect(120, 198, 720, 68), "TRIAL CONQUERED", 44, Ivory);
                Text(new Rect(160, 270, 640, 40), waveCount + " WAVES CLEARED", 24, Gold);
                Text(new Rect(160, 316, 640, 32), "Gruz Mother defeated · A crown earned in battle", 18, Ivory);
                Line(new Vector2(480 - extent, 370), new Vector2(480 + extent, 370), Gold, 1);
                Text(new Rect(160, 402, 640, 32), "Press Confirm / Enter to return to the Hall of Gods", 18, Gold);
            }
            finally { opacity = 1; GUI.color = priorColor; }
        }
        internal void DrawReset(float age)
        {
            var priorColor = GUI.color;
            try
            {
                if (font == null) font = Resources.FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name.IndexOf("perpetua", StringComparison.OrdinalIgnoreCase) >= 0);
                Fill(new Rect(0, 0, 960, 540), new Color(0.9f, 0.95f, 1f, Mathf.Max(0, 1f - age / 0.25f) * 0.55f));
                opacity = Mathf.Clamp01((1.5f - age) / 0.3f);
                Fill(new Rect(140, 202, 680, 108), new Color(0.015f, 0.02f, 0.035f, 0.8f));
                Line(new Vector2(265, 202), new Vector2(695, 202), Gold, 1);
                Text(new Rect(160, 211, 640, 50), "ALL UPGRADES RESET", 32, Ivory);
                Text(new Rect(160, 265, 640, 30), "5 masks · Old Nail · Mantis Claw", 18, Gold);
                Line(new Vector2(265, 310), new Vector2(695, 310), Gold, 1);
            }
            finally { opacity = 1; GUI.color = priorColor; }
        }
        internal void DrawLevelUp(int number, int pending, float elapsed)
        {
            var previousColor = GUI.color;
            try
            {
                if (font == null) font = Resources.FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name.IndexOf("perpetua", StringComparison.OrdinalIgnoreCase) >= 0);
                float p = Mathf.Clamp01(elapsed / RewardTransition.LevelUpSeconds);
                opacity = Mathf.Clamp01(p / 0.12f) * Mathf.Clamp01((1f - p) / 0.2f);
                Fill(new Rect(0, 0, 960, 540), new Color(0.015f, 0.02f, 0.035f, 0.28f));
                float extent = Mathf.Lerp(85, 235, Mathf.SmoothStep(0, 1, p));
                Line(new Vector2(480 - extent, 218), new Vector2(480 + extent, 218), Gold, 1);
                Text(new Rect(130, 225, 700, 58), "LEVEL " + number + " REACHED", 36, Ivory);
                Text(new Rect(170, 284, 620, 28), "Choose an upgrade or skip for +10% permanent XP", 18, Gold);
                Line(new Vector2(480 - extent, 326), new Vector2(480 + extent, 326), Gold, 1);
            }
            finally { opacity = 1f; GUI.color = previousColor; }
        }
        private bool DrawIcon(Card card, int currentRank, Rect rect)
        {
            var sprite = card.CharmId != 0 && CharmIconList.Instance != null
                ? CharmIconList.Instance.GetSprite(card.CharmId) : inventoryIcons.Get(card.Id, currentRank);
            var texture = icons.Get(sprite);
            if (texture == null) return false;
            GUI.color = Tint(Color.white);
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            return true;
        }
        private GUIStyle Style(int size, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            if (font != null) style.font = font;
            style.normal.textColor = color;
            return style;
        }
        private Color Tint(Color color) => new Color(color.r, color.g, color.b, color.a * opacity);
        private void Text(Rect rect, string text, int size, Color color)
        { GUI.color = Tint(Color.white); GUI.Label(rect, text, Style(size, color)); }
        private void Fill(Rect rect, Color color)
        { GUI.color = Tint(color); GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Tint(Color.white); }
        private void Line(Vector2 a, Vector2 b, Color color, float width = 2)
        {
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg, a);
            Fill(new Rect(a.x, a.y - width * 0.5f, Vector2.Distance(a, b), width), color);
            GUI.matrix = matrix;
        }
        private void Circle(Vector2 c, float radius, Color color)
        {
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI / 16f, b = (i + 1) * Mathf.PI / 16f;
                Line(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    c + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, color, 1.5f);
            }
        }
        private void DrawEmblem(Upgrade id, Vector2 c, Color color)
        {
            Circle(c, 30, new Color(color.r, color.g, color.b, 0.2f));
            switch (id)
            {
                case Upgrade.Nail:
                    Line(c + new Vector2(-12, 21), c + new Vector2(15, -23), color, 5);
                    Line(c + new Vector2(-17, 7), c + new Vector2(0, 17), color);
                    break;
                case Upgrade.Soul:
                    Circle(c, 16, color); Circle(c + new Vector2(-6, -2), 3, color); Circle(c + new Vector2(6, -2), 3, color);
                    Line(c + new Vector2(-12, -10), c + new Vector2(-19, -23), color);
                    Line(c + new Vector2(12, -10), c + new Vector2(19, -23), color);
                    break;
                case Upgrade.Spell:
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4;
                        Line(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 8,
                            c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (i % 2 == 0 ? 27 : 18), color);
                    }
                    Circle(c, 6, color); break;
                case Upgrade.Recovery:
                    Line(c + new Vector2(0, 24), c + new Vector2(0, -22), color);
                    for (int i = -1; i <= 1; i++)
                    {
                        float y = i * 12;
                        Line(c + new Vector2(0, y + 8), c + new Vector2(-15, y - 4), color);
                        Line(c + new Vector2(-15, y - 4), c + new Vector2(0, y - 1), color);
                        Line(c + new Vector2(0, y + 8), c + new Vector2(15, y - 4), color);
                        Line(c + new Vector2(15, y - 4), c + new Vector2(0, y - 1), color);
                    }
                    break;
                case Upgrade.Shell:
                    Circle(c, 23, color);
                    for (int i = -1; i <= 1; i++) Line(c + new Vector2(i * 12, -18), c + new Vector2(i * 6, 18), color);
                    break;
                case Upgrade.Mask:
                    Circle(c, 21, color);
                    Circle(c + new Vector2(-8, -3), 4, color);
                    Circle(c + new Vector2(8, -3), 4, color);
                    Line(c + new Vector2(-12, -16), c + new Vector2(-21, -28), color);
                    Line(c + new Vector2(12, -16), c + new Vector2(21, -28), color);
                    break;
                case Upgrade.LastStand:
                    Line(c + new Vector2(0, -27), c + new Vector2(-16, 11), color);
                    Line(c + new Vector2(-16, 11), c + new Vector2(0, 23), color);
                    Line(c + new Vector2(0, 23), c + new Vector2(16, 11), color);
                    Line(c + new Vector2(16, 11), c + new Vector2(0, -27), color);
                    Circle(c + new Vector2(0, 9), 5, color); break;
                default:
                    Circle(c, 20, color); Circle(c, 10, color);
                    Line(c + new Vector2(0, -20), c + new Vector2(0, -30), color);
                    break;
            }
        }
        public void Dispose() { icons.Dispose(); if (frame != null) UnityEngine.Object.Destroy(frame); }
    }
}
