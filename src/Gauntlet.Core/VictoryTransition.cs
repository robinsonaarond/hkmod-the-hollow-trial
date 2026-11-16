using System;

namespace Gauntlet.Core
{
    public sealed class VictoryTransition
    {
        public const float RevealSeconds = 0.5f;
        public bool Active { get; private set; }
        public bool ShowingScreen { get; private set; }
        public float Elapsed { get; private set; }
        public bool CanConfirm => Active && ShowingScreen && Elapsed >= RevealSeconds;
        public void Begin() { Active = true; ShowingScreen = false; Elapsed = 0; }
        public void Cancel() { Active = false; ShowingScreen = false; Elapsed = 0; }
        // Returns true once, when the native death sequence has finished.
        public bool Advance(float seconds, bool paused, bool deathFinished)
        {
            if (!Active || paused) return false;
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!ShowingScreen)
            {
                if (!deathFinished) return false;
                ShowingScreen = true; Elapsed = 0;
                return true;
            }
            Elapsed = Math.Min(RevealSeconds, Elapsed + seconds);
            return false;
        }
        public bool Confirm()
        {
            if (!CanConfirm) return false;
            Active = false;
            return true;
        }
    }
}
