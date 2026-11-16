using System;

namespace Gauntlet.Core
{
    public sealed class RewardTransition
    {
        public const float LevelUpSeconds = 1f;
        public const float CardRiseSeconds = 0.5f;
        public const float TotalSeconds = LevelUpSeconds + CardRiseSeconds;
        public bool Active { get; private set; }
        public float Elapsed { get; private set; }
        public bool ShowingLevelUp => Active && Elapsed < LevelUpSeconds;
        public float CardProgress => !Active ? 0f : Math.Max(0, Math.Min(1, (Elapsed - LevelUpSeconds) / CardRiseSeconds));
        public bool CanChoose => Active && Elapsed >= TotalSeconds;
        public void Begin() { Elapsed = 0; Active = true; }
        public void Cancel() { Elapsed = 0; Active = false; }
        public void Advance(float seconds, bool paused)
        {
            if (!Active || paused) return;
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            Elapsed = Math.Min(TotalSeconds, Elapsed + seconds);
        }
    }
}
