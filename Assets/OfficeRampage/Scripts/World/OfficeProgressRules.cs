using System;
namespace OfficeRampage
{
    public static class OfficeProgressRules
    {
        public static int Required(int total) { return (int)Math.Ceiling(total * .6); }
        public static bool ExitReady(int destroyed, int totalProps, int defeated, int totalEnemies)
        { return destroyed >= Required(totalProps) && defeated >= Required(totalEnemies); }
        public static bool TimerRuns(int room) { return room < 5; }
        public static float Heal(float health) { return Math.Min(100, health + 30); }
    }
}
