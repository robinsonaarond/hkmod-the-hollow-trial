using System;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class ArenaCeiling : IDisposable
    {
        internal const float ScreenInset = 0.35f;
        internal float Top { get; private set; }
        private const int TerrainMask = 1 << 8;
        private GameObject fallback;

        internal void Ensure(float left, float right, float floor, Action<string> report, float visibleTop)
        {
            // Sample the real gameplay viewport rather than guessing its height.
            Top = visibleTop - ScreenInset;
            if (Top <= floor + 6f) throw new InvalidOperationException("Gameplay camera has insufficient arena headroom.");
            if (fallback == null)
            {
                fallback = new GameObject("Gauntlet.Ceiling"); fallback.layer = 8;
                fallback.transform.position = new Vector3((left + right) * 0.5f, Top + 1f, 0f);
                var collider = fallback.AddComponent<BoxCollider2D>();
                collider.isTrigger = false; collider.size = new Vector2(right - left + 4f, 2f);
            }
            fallback.transform.position = new Vector3((left + right) * 0.5f, Top + 1f, 0f);
            Physics2D.SyncTransforms();
            if (!Covered(left, right, floor, out float low, out float high) || Math.Abs(low - Top) > 0.1f || Math.Abs(high - Top) > 0.1f)
                throw new InvalidOperationException("The lowered arena ceiling could not be verified.");
            report?.Invoke($"Arena ceiling verified across {right - left:F1} units; solid underside y={low:F1}..{high:F1}.");
        }
        private static bool Covered(float left, float right, float floor, out float low, out float high)
        {
            low = float.PositiveInfinity; high = float.NegativeInfinity;
            int steps = Math.Max(1, Mathf.CeilToInt((right - left) / 0.5f));
            for (int i = 0; i <= steps; i++)
            {
                float x = left + 0.5f + (right - left - 1f) * i / steps;
                float roof = float.PositiveInfinity;
                // RaycastAll lets us reject triggers without changing global
                // physics settings. A ceiling must present a downward solid face.
                foreach (var hit in Physics2D.RaycastAll(new Vector2(x, floor + 4f), Vector2.up, 32f, TerrainMask))
                    if (hit.collider != null && !hit.collider.isTrigger && hit.normal.y < -0.25f && hit.point.y > floor + 6f)
                        roof = Math.Min(roof, hit.point.y);
                if (float.IsPositiveInfinity(roof)) return false;
                low = Math.Min(low, roof); high = Math.Max(high, roof);
            }
            return true;
        }
        public void Dispose()
        { if (fallback != null) UnityEngine.Object.Destroy(fallback); fallback = null; }
    }
}
