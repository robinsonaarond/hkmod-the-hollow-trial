using System;

namespace Gauntlet.Core
{
    public static class EnemyExperience
    {
        public static int For(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Crawlid: return 5;
                case EnemyKind.Vengefly: return 10;
                case EnemyKind.Hornhead: return 15;
                case EnemyKind.Aspid: return 20;
                case EnemyKind.Hopper: return 25;
                case EnemyKind.PrimalAspid: return 35;
                case EnemyKind.MantisWarrior: return 40;
                case EnemyKind.WingedSentry: return 45;
                case EnemyKind.HeavySentry: return 60;
                case EnemyKind.GreatHopper: return 60;
                case EnemyKind.GruzMother: return 100;
                case EnemyKind.BrokenVessel: return 250;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
