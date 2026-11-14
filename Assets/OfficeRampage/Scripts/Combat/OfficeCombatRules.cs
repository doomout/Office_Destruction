using System;
namespace OfficeRampage
{
    public static class OfficeCombatRules
    {
        public static readonly string[] Weapons = { "야구빠따", "망치", "권총", "기관총", "미인계 더미", "샷건", "로켓런처", "비에프지 18" };
        public static readonly float[] Cooldowns = { .32f, .65f, .28f, .085f, .9f, .28f, 1.65f, 3f };
        public static readonly float[] Ranges = { 2.7f, 2.5f, 24f, 24f, 16f, 24f, 65f, 100f };
        public static bool Rage(float hp, float maximum) { return hp <= maximum * .5f; }
        public static bool Gift(float hp, float maximum) { return hp <= maximum * .2f; }
        public static float PhaseTwoHealth(float hp, float maximum) { return Math.Min(maximum * 2, (hp + maximum * .5f) * 2); }
        public static float DamageResult(float hp, float damage, float maximum, bool phaseTwo)
        { return Math.Max(phaseTwo ? 0 : maximum * .2f, hp - damage); }
    }
}
