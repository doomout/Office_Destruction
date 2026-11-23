using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeEndlessPickup:MonoBehaviour
    {
        public int kind,weaponId; // 0 weapon, 1 armor, 2 health, 3 double damage
        public string DisplayName;
    }
}
