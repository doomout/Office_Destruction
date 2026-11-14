using System;

namespace OfficeRampage
{
    public static class OfficeRules
    {
        public const int Desk = 0, Computer = 1, WaterCooler = 2, Copier = 3, Cabinet = 4, Plant = 5;
        public static bool IsElectronic(int kind) { return kind == Computer || kind == Copier; }
        public static float MovementSpeed(float baseSpeed, bool onOil) { return baseSpeed * (onOil ? .5f : 1); }
        public static float HealthAfterBreak(float health, int kind)
        {
            if (IsElectronic(kind)) return Math.Max(0, health - 5);
            if (kind == WaterCooler) return Math.Min(100, health + 5);
            return health;
        }
        public static bool OnOil(float x, float z, float feetHeight, float centerX, float centerZ, float radiusX, float radiusZ)
        {
            if (feetHeight < -.2f || feetHeight > .3f) return false;
            float dx = (x - centerX) / radiusX, dz = (z - centerZ) / radiusZ;
            return dx * dx + dz * dz <= 1;
        }
    }
}
