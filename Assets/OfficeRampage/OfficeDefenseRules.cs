using System;

namespace OfficeRampage
{
    public static class OfficeDefenseRules
    {
        public const int BossHealth = 500;
        public static int Count(int room) { return room == 1 ? 1 : room == 5 ? 5 : 3; }
        public static int Health(int room, int index, Random random)
        {
            if (room == 1) return 60;
            if (room == 5) return random.Next(100, 201);
            return ((room - 2) * 3 + index + 1) * 20;
        }
        public static string Name(int room, int index)
        {
            if (room == 1) return "경비원";
            if (room == 5) return "경비견 " + (index + 1);
            string[] ranks = { "사원", "주임", "대리", "과장", "차장", "부장", "이사", "상무", "전무" };
            return ranks[(room - 2) * 3 + index];
        }
        public static bool CanWakeBoss(int room, int livingDogs, bool alreadyAwake)
        { return room == 5 && livingDogs == 0 && !alreadyAwake; }
    }
}
