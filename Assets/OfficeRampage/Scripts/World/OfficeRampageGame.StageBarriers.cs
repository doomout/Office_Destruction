using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void RemoveStageBarrier(OfficeSlidingDoor door)
        {
            if(door==null)return;
            float z=door.transform.position.z;
            var root=activeFloor!=null?activeFloor.transform:world;
            foreach(var wall in root.GetComponentsInChildren<Transform>())
                if(wall.name.StartsWith("Indestructible partition")&&Mathf.Abs(wall.position.z-z)<1)wall.gameObject.SetActive(false);
            door.RemoveBarrier();audioBus.Play("door",.6f);
        }
    }
}
