namespace OfficeRampage
{
    public static class OfficeMenuRules
    {
        public static int Normalize(int difficulty) { return difficulty < 0 || difficulty > 2 ? 1 : difficulty; }
        public static int Duration(int difficulty)
        {
            switch (Normalize(difficulty)) { case 0: return 60; case 2: return 180; default: return 120; }
        }
        public static string Label(int difficulty)
        {
            switch (Normalize(difficulty))
            {
                case 0: return "개빡침(어려움)";
                case 2: return "스트레스(쉬움)";
                default: return "빡침(중간)";
            }
        }
    }
}
