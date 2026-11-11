using System;
namespace OfficeRampage
{
    public static class OfficeCombatRules
    {
        public static readonly string[] Weapons = { "야구빠따", "망치", "권총", "기관총", "채찍", "샷건", "로켓런처", "BFG 18" };
        public static readonly float[] Cooldowns = { .32f, .65f, .28f, .085f, .55f, 1.1f, 1.65f, 3f };
        public static readonly float[] Ranges = { 2.7f, 2.5f, 24f, 24f, 9f, 20f, 65f, 100f };
        public static bool Rage(float hp, float maximum) { return hp <= maximum * .5f; }
        public static bool Gift(float hp, float maximum) { return hp <= maximum * .2f; }
        public static float PhaseTwoHealth(float hp, float maximum) { return Math.Min(maximum, hp + maximum * .5f); }
        public static float DamageResult(float hp, float damage, float maximum, bool phaseTwo)
        { return Math.Max(phaseTwo ? 0 : maximum * .2f, hp - damage); }
    }
}
