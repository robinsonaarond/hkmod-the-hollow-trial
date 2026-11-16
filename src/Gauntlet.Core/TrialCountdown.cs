using System;

namespace Gauntlet.Core
{
    // Only advances while the game adapter reports that arrival is complete,
    // the Knight is alive and the game is unpaused. Completion fires once.
    public sealed class TrialCountdown
    {
        public const float Duration = 3f;
        public bool Pending { get; private set; }
        public float Remaining { get; private set; }
        public void Queue() { Remaining = Duration; Pending = true; }
        public void Cancel() { Remaining = 0; Pending = false; }
        public bool Advance(bool ready, float elapsedSeconds)
        {
            if (!Pending || !ready) return false;
            if (elapsedSeconds < 0 || float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds))
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            Remaining = Math.Max(0, Remaining - elapsedSeconds);
            if (Remaining > 0) return false;
            Pending = false;
            return true;
        }
    }
}
