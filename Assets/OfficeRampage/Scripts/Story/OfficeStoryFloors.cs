namespace OfficeRampage
{
    public static class OfficeStoryFloors
    {
        public const int Boss=6;
        public static bool IsExecutive(int floor)=>floor>=3&&floor<Boss;
    }
}
