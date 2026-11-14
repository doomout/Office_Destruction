using UnityEngine;
namespace OfficeRampage
{
    [CreateAssetMenu(menuName="Office Rampage/Character Catalog")]
    public sealed class OfficeCharacterCatalog : ScriptableObject
    {
        public GameObject[] male,female;
    }
}
