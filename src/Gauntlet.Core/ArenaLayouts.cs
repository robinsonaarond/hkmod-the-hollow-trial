using System;
using System.Collections.Generic;

namespace Gauntlet.Core
{
    public enum PlatformKind { Small, Wide, Spike, Bell }
    public sealed class PlatformPlacement
    {
        public PlatformKind Kind { get; }
        public float Offset { get; }
        public float Top { get; }
        public PlatformPlacement(PlatformKind kind, float offset, float top)
        { Kind = kind; Offset = offset; Top = top; }
    }
    public sealed class ArenaLayout
    {
        public string Name { get; }
        public IReadOnlyList<PlatformPlacement> Platforms { get; }
        public ArenaLayout(string name, params PlatformPlacement[] platforms)
        { Name = name; Platforms = Array.AsReadOnly(platforms); }
    }
    public static class StageLayouts
    {
        private static readonly ArenaLayout Open = new ArenaLayout("Open Stage");
        private static readonly ArenaLayout[] Rotation =
        {
            new ArenaLayout("Center Dais", new PlatformPlacement(PlatformKind.Wide, 0, 3.8f)),
            new ArenaLayout("Split Steps", new PlatformPlacement(PlatformKind.Small, -7, 3.5f), new PlatformPlacement(PlatformKind.Small, 7, 3.5f)),
            new ArenaLayout("Staggered Steps", new PlatformPlacement(PlatformKind.Small, -7, 3.5f), new PlatformPlacement(PlatformKind.Small, 6, 4.5f)),
            new ArenaLayout("Twin Balconies", new PlatformPlacement(PlatformKind.Wide, -6, 4), new PlatformPlacement(PlatformKind.Wide, 6, 4)),
            Open
        };
        public static float[] SpikesForWave(int number)
        {
            return Array.Empty<float>(); // Floor hazards removed to keep arrivals clear.
        }
        public static ArenaLayout ForWave(int number)
        {
            if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
            // Baseline training, elite gates, and final boss retain an open floor.
            return number <= 5 || number == 26 || number >= 47 ? Open : Rotation[(number - 6) % Rotation.Length];
        }
    }
}
