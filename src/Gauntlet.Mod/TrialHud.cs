using System;
using System.Linq;
using Gauntlet.Core;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class TrialHud
    {
        private Font font;
        internal void Draw(Run run, string controls, string notice, bool fragileBroken)
        {
            // Logical 960x540 coordinates. Reserve the left 60% for native
            // Soul, masks and long Joni health bars, including reserve vessels.
            const float x = 586, width = 358;
            if (font == null) font = Resources.FindObjectsOfTypeAll<Font>()
                .FirstOrDefault(f => f.name.IndexOf("perpetua", StringComparison.OrdinalIgnoreCase) >= 0);
            var previous = GUI.color;
            try
            {
                string heading = run.IsActive ? $"{run.CurrentWave.Number} / {run.WaveCount}  ·  {run.CurrentWave.Title}" : "THE HOLLOW TRIAL";
                Label(new Rect(x, 5, width, 21), heading, 16, new Color(0.88f, 0.87f, 0.83f));
                // A quiet ivory rule replaces the large opaque debug box.
                GUI.color = new Color(0.75f, 0.73f, 0.65f, 0.28f);
                GUI.DrawTexture(new Rect(x + 80, 27, width - 80, 1), Texture2D.whiteTexture);
                string detail = run.IsActive
                    ? $"{run.EnemiesRemaining} foes  ·  Nail {run.BaseNailDamage}  ·  {run.CharmIds.Count} boons    ·    F7: Return"
                    : controls;
                Label(new Rect(x, 30, width, 18), detail, 12, new Color(0.65f, 0.67f, 0.7f));
                if (!string.IsNullOrEmpty(notice)) Label(new Rect(x, 49, width, 38), notice, 12,
                    fragileBroken ? new Color(0.78f, 0.62f, 0.42f) : new Color(0.65f, 0.67f, 0.7f));
            }
            finally { GUI.color = previous; }
        }
        internal void DrawWaveAnnouncement(WaveAnnouncement notice, int waveCount)
        {
            if (!notice.Active) return;
            var previous = GUI.color;
            float alpha = notice.Alpha;
            try
            {
                GUI.color = new Color(0.015f, 0.02f, 0.035f, 0.78f * alpha);
                GUI.DrawTexture(new Rect(230, 165, 500, 78), Texture2D.whiteTexture);
                GUI.color = new Color(0.88f, 0.73f, 0.45f, alpha);
                GUI.DrawTexture(new Rect(300, 165, 360, 1), Texture2D.whiteTexture);
                var style = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
                if (font != null) style.font = font;
                style.normal.textColor = new Color(0.94f, 0.9f, 0.8f, alpha);
                GUI.color = Color.white;
                GUI.Label(new Rect(230, 170, 500, 36), $"WAVE {notice.Number} / {waveCount}", style);
                style.fontSize = 18;
                style.normal.textColor = new Color(0.88f, 0.73f, 0.45f, alpha);
                GUI.Label(new Rect(230, 207, 500, 27), notice.Title, style);
            }
            finally { GUI.color = previous; }
        }
        internal void DrawExperience(Run run)
        {
            if (run.Phase == RunPhase.Idle) return;
            var xpStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.UpperLeft };
            if (font != null) xpStyle.font = font;
            xpStyle.normal.textColor = new Color(0.88f, 0.8f, 0.5f);
            var previous = GUI.color; GUI.color = Color.white;
            // Keep both counters below native masks and reserve vessels.
            GUI.Label(new Rect(90, 115, 105, 24), $"Level  {run.Level}", xpStyle);
            GUI.Label(new Rect(205, 115, 300, 24), $"XP  {run.Experience:N0}", xpStyle);
            xpStyle.fontSize = 13;
            xpStyle.normal.textColor = new Color(0.7f, 0.74f, 0.78f);
            GUI.Label(new Rect(90, 140, 175, 22), $"Temporary XP  +{run.TemporaryXpBonusPercent}%", xpStyle);
            GUI.Label(new Rect(275, 140, 200, 22), $"Permanent XP  +{run.PermanentXpBonusPercent}%", xpStyle);
            GUI.color = previous;
        }
        private void Label(Rect rect, string text, int size, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, alignment = TextAnchor.UpperRight, wordWrap = true };
            if (font != null) style.font = font;
            style.normal.textColor = color;
            GUI.color = Color.white;
            GUI.Label(rect, text, style);
        }
    }
}
