using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private static readonly string[] weaponModelNames={"Bat","Hammer","Pistol","Machinegun","","Shotgun","Rocket","BFG"};
        private readonly GameObject[] weaponModelPrefabs=new GameObject[8];
        private bool TryBuildWeaponModel()
        {
            if(weaponIndex<0||weaponIndex>=weaponModelNames.Length||weaponIndex==4)return false;
            var prefab=weaponModelPrefabs[weaponIndex];
            if(prefab==null)prefab=weaponModelPrefabs[weaponIndex]=Resources.Load<GameObject>("OfficeWeapons/"+weaponModelNames[weaponIndex]);
            if(prefab==null)return false; // Retain the original procedural model if an asset is unavailable.
            var model=Instantiate(prefab,weapon,false);model.name="Weapon model";
            // The prefabs are authored around their right-hand grip, facing local +Z.
            var grip=new GameObject("Grip").transform;grip.SetParent(weapon,false);
            return true;
        }
    }
}
