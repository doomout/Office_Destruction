using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void BuildElevatorCab(Transform root, Room room)
        {
            var cab=new GameObject("Elevator cabin").transform; cab.SetParent(root,false);
            float z=room.end+1.7f;
            Color metal=new Color(.25f,.31f,.36f);
            Shape(cab,"Elevator floor",new Vector3(0,-.2f,z),new Vector3(4,.4f,3.6f),metal);
            Shape(cab,"Elevator back",new Vector3(0,1.65f,z+1.8f),new Vector3(4,3.3f,.2f),metal);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(cab,"Elevator side",new Vector3(side*2,1.65f,z),new Vector3(.2f,3.3f,3.6f),metal);
                Shape(cab,"Elevator handrail",new Vector3(side*1.85f,1,z),new Vector3(.08f,.08f,3),Cyan,false);
            }
            Shape(cab,"Elevator ceiling",new Vector3(0,3.4f,z),new Vector3(4,.2f,3.6f),metal);
            Shape(cab,"Elevator light",new Vector3(0,3.25f,z),new Vector3(2,.08f,1),Color.white,false);
            Shape(cab,"Elevator buttons",new Vector3(1.25f,1.3f,z+1.67f),new Vector3(.25f,.5f,.04f),Cyan,false);
            var sign=new GameObject("Elevator floor indicator").AddComponent<TextMesh>(); sign.transform.SetParent(cab,false);
            sign.transform.localPosition=new Vector3(0,2.5f,z+1.65f); sign.anchor=TextAnchor.MiddleCenter;
            sign.characterSize=.16f; sign.fontSize=48; sign.color=Cyan; sign.text=room.number+"층 → "+(room.number+1)+"층";
        }
        private IEnumerator RideElevator()
        {
            if(combatLocked) yield break;
            combatLocked=true; ClearDecoy();
            int next=current.number+1; var door=current.door;
            controller.enabled=false; player.position=current.door.transform.TransformPoint(new Vector3(0,.08f,1.8f));
            yaw=180; pitch=8; avatar.rotation=Quaternion.Euler(0,yaw,0); vertical=0;
            cinematicText=next+"층으로 이동합니다";
            for(float t=0;t<1;)
            {
                if(RunActive)
                {
                    t=Mathf.Min(1,t+Time.deltaTime/1.2f);
                    float slide=Mathf.SmoothStep(1.8f,0,t);
                    door.left.localPosition=new Vector3(-.825f-slide,1.55f,0);
                    door.right.localPosition=new Vector3(.825f+slide,1.55f,0);
                }
                yield return null;
            }
            // Remove the entire previous floor, including enemies, projectiles, drops and transient FX.
            foreach(Transform child in world)
                if(child!=player && child!=view.transform && child.GetComponent<Light>()==null)
                { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            rooms.Clear(); defenders.Clear(); oilPuddles.Clear(); debris.Clear(); combatEffects.Clear();
            bossUnit=null; bossChair=null; finalRoomSealed=false;
            combo=0; comboLeft=0; whirlwindLeft=whirlwindReady=0; ResetElements();
            yield return null; // Destruction completes before creating the next floor.
            current=BuildRoom(next,0);
            player.position=new Vector3(0,.08f,3); yaw=0; pitch=13; avatar.rotation=Quaternion.identity;
            controller.enabled=true; ApplyFloorEntry(); Physics.SyncTransforms(); invulnerability=2;
            audioBus.ChangeRoom(next); cinematicText=""; combatLocked=false;
            Notify(next==4?"대표층 · 경비견을 모두 처치하세요. 시간 제한은 없습니다.":next+"층 도착 · 직원을 모두 처치하면 엘리베이터가 열립니다.",4);
            UpdateCamera();
        }
    }
}
