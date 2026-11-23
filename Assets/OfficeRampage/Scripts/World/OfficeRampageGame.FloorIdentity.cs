using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private OfficeSlidingDoor dogRoomDoor,guardRoomDoor;
        private Transform waitingSecretary;
        private void FloorSign(Transform parent,string text,Vector3 at,float size=.18f)
        {
            var sign=new GameObject(text).AddComponent<TextMesh>(); OfficeGameFont.Apply(sign);sign.transform.SetParent(parent,false);
            sign.transform.localPosition=at;sign.anchor=TextAnchor.MiddleCenter;sign.characterSize=size;sign.fontSize=48;sign.color=Cyan;sign.text=text;
        }
        private OfficeSlidingDoor SuiteDoor(Transform root,float z,string title)
        {
            var slot=new Room{end=z};CreateSlidingDoor(slot,root);slot.gate.name=title;
            slot.door.openingText=title+" 열리는 중";slot.door.sign.text=title+" · 잠김";
            return slot.door;
        }
        private void BuildFloorIdentity(Room room,Transform root)
        {
            Color accent=room.number==1?new Color(.7f,.62f,.45f):room.number==2?new Color(.12f,.35f,.45f):room.number==3?new Color(.4f,.22f,.15f):new Color(.4f,.12f,.15f);
            FloorSign(root,room.number+"층 · "+room.layoutName,new Vector3(0,4,room.end-.3f));
            if(room.number==1)
            {
                Shape(root,"Lobby stone inlay",new Vector3(0,.008f,14),new Vector3(9,.01f,22),accent,false);
                for(int s=-1;s<=1;s+=2)
                    Shape(root,"Lobby wall accent",new Vector3(s*12.8f,3,14),new Vector3(.12f,5,8),accent,false);
                FloorSign(root,"OFFICE / 로비",new Vector3(0,5,27.7f),.35f);
            }
            if(room.number==2 || room.number==3)
            {
                foreach(var prop in root.GetComponentsInChildren<OfficeProp>())
                    if(prop.kind<=1)
                    {
                        var chair=new GameObject("Desk chair").transform;chair.SetParent(prop.transform,false);chair.localPosition=new Vector3(0,0,-1.2f);
                        Shape(chair,"Seat",new Vector3(0,.5f,0),new Vector3(.7f,.15f,.7f),accent,false);
                        Shape(chair,"Back",new Vector3(0,.95f,-.3f),new Vector3(.7f,.8f,.12f),accent,false);
                        Shape(chair,"Pedestal",new Vector3(0,.25f,0),new Vector3(.13f,.5f,.13f),Navy,false);
                    }
                if(room.number==3)
                    foreach(float z in new[]{7f,21f,36f})foreach(int side in new[]{-1,1})
                        FloorSign(root,"임원실",new Vector3(side*8,2.8f,z+3.6f),.11f);
            }
            if(room.number==4)
            {
                dogRoomDoor=SuiteDoor(root,room.start+18,"보디가드실");
                guardRoomDoor=SuiteDoor(root,room.start+36,"대표실");
                FloorSign(root,"경비견 구역",new Vector3(0,4,17.7f));
                FloorSign(root,"보디가드 구역",new Vector3(0,4,35.7f));
                FloorSign(root,"비서실",new Vector3(9.5f,3.8f,42.2f));
                Shape(root,"CEO carpet",new Vector3(-3,.012f,48),new Vector3(13,.014f,21),accent,false);
            }
        }
        private void BuildWaitingSecretary(Transform root,Room room)
        {
            waitingSecretary=new GameObject("Waiting secretary").transform;waitingSecretary.SetParent(root,false);
            waitingSecretary.position=activeFloor!=null?activeFloor.secretarySpawn.position:new Vector3(10,.08f,room.start+48);
            BuildPerson(waitingSecretary,false);waitingSecretary.GetComponent<OfficeCharacterVisual>()?.SetIdentity(true,0,0);
            waitingSecretary.gameObject.AddComponent<OfficeSecretaryIdle>().game=this;
            FloorSign(waitingSecretary,"비서",new Vector3(0,2.3f,0),.1f);
        }
    }
}
