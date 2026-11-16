using System;

namespace Gauntlet.Core
{
    public static class CharacterLevels
    {
        // Level 1 starts at zero XP. Each advance costs ten times the
        // current level, giving cumulative threshold 5 * level * (level - 1).
        public static long Threshold(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            return checked(5L * level * (level - 1));
        }
        public static long CostToNext(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            return 10L * level;
        }
        public static int ForExperience(int experience)
        {
            if (experience < 0) throw new ArgumentOutOfRangeException(nameof(experience));
            int level = (int)((1 + Math.Sqrt(1 + 0.8 * experience)) / 2);
            // Use integer thresholds at the boundary, independent of rounding.
            while (Threshold(level + 1) <= experience) level++;
            while (Threshold(level) > experience) level--;
            return level;
        }
    }
}
