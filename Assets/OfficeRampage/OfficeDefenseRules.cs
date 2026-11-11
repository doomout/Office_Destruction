using System;
namespace OfficeRampage
{
    public static class OfficeDefenseRules
    {
        public const int BossHealth = 500;
        public static int Count(int room) { return room == 1 ? 10 : room == 2 ? 22 : room == 3 ? 5 : room == 4 ? 3 : 5; }
        public static string Name(int room, int index)
        {
            if (room == 1) return "경비";
            if (room == 2) return index < 10 ? "사원" : index < 17 ? "주임" : "대리";
            if (room == 3) return index < 3 ? "차장" : "부장";
            if (room == 4) return new[] { "이사", "상무", "전무" }[index];
            return "경비견 " + (index + 1);
        }
        public static int Health(int room, int index, Random random)
        {
            if (room == 1) return 60;
            if (room == 2) return index < 10 ? 20 : index < 17 ? 40 : 60;
            if (room == 3) return index < 3 ? 100 : 120;
            if (room == 4) return 140 + index * 20;
            return random.Next(100, 201);
        }
        public static bool CanWakeBoss(int room, int livingDogs, bool alreadyAwake)
        { return room == 5 && livingDogs == 0 && !alreadyAwake; }
    }
}
