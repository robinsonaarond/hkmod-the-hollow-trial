using System;

namespace Gauntlet.Core
{
    public sealed class WaveAnnouncement
    {
        public const float Duration = 2.6f;
        public int Number { get; private set; }
        public string Title { get; private set; } = "";
        public float Elapsed { get; private set; } = Duration;
        public bool Active => Elapsed < Duration;
        public float Alpha => Active ? Math.Min(1f, Math.Min(Elapsed / 0.15f, (Duration - Elapsed) / 0.45f)) : 0;
        public void Show(int number, string title) { Number = number; Title = title; Elapsed = 0; }
        public void Advance(float seconds, bool paused)
        {
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!paused) Elapsed = Math.Min(Duration, Elapsed + seconds);
        }
        public void Cancel() { Elapsed = Duration; Title = ""; Number = 0; }
    }
}
