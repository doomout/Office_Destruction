using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private OfficeFloorPrefab activeFloor;
        private Room BuildPrefabRoom(int number,float start,OfficeFloorPrefab asset)
        {
            activeFloor=Instantiate(asset,world);activeFloor.name=number==4?"CEO arena":"Office "+number;
            activeFloor.transform.localPosition=Vector3.forward*start;
            var root=activeFloor.transform;var data=activeFloor.ReadLayout();var layout=data.CreateLayout();
            var room=new Room{number=number,start=start,end=start+data.length,width=data.width,layoutName=data.name};rooms.Add(room);
            foreach(var preview in activeFloor.previewRoots)if(preview!=null)preview.gameObject.SetActive(false);
            foreach(var prop in root.GetComponentsInChildren<OfficeProp>()){prop.game=this;prop.room=number;room.total++;}
            room.required=room.total;
            foreach(var potion in root.GetComponentsInChildren<OfficeHealingPotion>()){potion.game=this;potion.room=number;}
            foreach(var door in root.GetComponentsInChildren<OfficeSlidingDoor>())door.game=this;
            if(number<4){room.door=activeFloor.elevator;room.gate=room.door.gameObject;room.gateRenderer=room.door.indicators[0];}
            else {dogRoomDoor=activeFloor.dogDoor;guardRoomDoor=activeFloor.guardDoor;}
            SetupDefenders(room,layout,root);
            int index=0;
            foreach(var unit in defenders)if(unit.room==number&&!unit.boss){var marker=activeFloor.enemySpawns[index++];unit.Teleport(marker.position);unit.transform.rotation=marker.rotation;}
            ApplyKoreanWorldFont(root);return room;
        }
        private void ApplyFloorEntry()
        {
            if(activeFloor==null || activeFloor.playerSpawn==null)return;
            controller.enabled=false;player.position=activeFloor.playerSpawn.position;controller.enabled=true;
            yaw=activeFloor.playerSpawn.eulerAngles.y;avatar.rotation=Quaternion.Euler(0,yaw,0);vertical=0;
        }
        private bool InsideElevator()
        {
            var local=current.door.transform.InverseTransformPoint(player.position);
            return local.z>.7f&&local.z<3.4f&&Mathf.Abs(local.x)<1.45f;
        }
    }
}
