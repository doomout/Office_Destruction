using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool securityWaveStarted, securityWavePending;
        private IEnumerator RevealSecurity()
        {
            // Leave the hit/area-damage iteration before adding members to defenders.
            yield return null;
            while(!RunActive) yield return null;
            if(current==null || current.number!=4 || ended || bossUnit==null) { securityWavePending=false; yield break; }
            securityWaveStarted=true;
            var layout=activeFloor!=null?activeFloor.ReadLayout().CreateLayout():OfficeFixedFloor.Load(4).CreateLayout();
            var navigation=new OfficeNavigation(layout);
            var points=OfficeFixedFloor.Load(4).EnemyPoints();
            for(int i=0;i<5;i++)
            {
                int kind=i<2?0:i<4?3:4;
                string label=kind==0?"몽둥이":kind==3?"권총":"장거리 샷건";
                var pos=activeFloor!=null?activeFloor.guardSpawns[i].position:new Vector3(-8+i*4,.08f,current.start+26+(i%2)*4);
                var at=new OfficeLayout.Point(pos.x,pos.z-current.start);
                var unit=CreateDefender(bossUnit.transform.parent,current,navigation,new Vector3(at.x,.08f,current.start+at.z),"경비 · "+label,kind==4?160:kind==3?120:140,false,false);
                unit.guard=true;unit.securityWeapon=kind;unit.patrolStart=current.start+.7f;unit.patrolEnd=guardRoomDoor.transform.position.z-1;
                unit.visual.GetComponent<OfficeCharacterVisual>()?.SetIdentity(false,i,0);
                foreach(Transform child in unit.visual)
                    if(child.name=="A4 hand" || (kind!=0 && child.name=="Baton")) child.gameObject.SetActive(false);
                if(kind!=0)
                {
                    var gun=new GameObject("Security weapon").transform;gun.SetParent(unit.visual,false);gun.localPosition=new Vector3(.42f,1.15f,.4f);
                    Shape(gun,"Receiver",Vector3.zero,new Vector3(.18f,.2f,kind==4?.65f:.3f),Navy,false);
                    Shape(gun,"Barrel",new Vector3(0,.02f,.4f),new Vector3(.08f,.08f,kind==4?.7f:.35f),Color.gray,false);
                    Shape(gun,"Grip",new Vector3(0,-.16f,0),new Vector3(.1f,.25f,.15f),Navy,false);
                    if(kind==4) Shape(gun,"Second barrel",new Vector3(.1f,.02f,.4f),new Vector3(.08f,.08f,.7f),Color.gray,false);
                }
                Impact(unit.AimPoint,Cyan,Vector3.up,8);
            }
            spawnedEnemies[4]+=5;securityWavePending=false;
            StoryLine("대표: 일단 진정하고, 방망이부터 내려놓게.",7);
            Notify("보디가드실 개방 · 5명을 모두 처치하면 대표실이 열립니다.",5);
        }
        private void FireSecurityGun(OfficeDefender unit,int kind,Vector3 direction)
        {
            bool shotgun=kind==4;float range=shotgun?22:12;
            int count=shotgun?5:1;
            for(int i=0;i<count;i++)
            {
                Vector3 ray=Quaternion.Euler(0,shotgun?(i-2)*4:0,0)*direction;
                var obj=Shape(world,"Security bullet",unit.AimPoint+ray*.55f,new Vector3(.06f,.06f,.45f),Color.yellow,false);
                obj.transform.rotation=Quaternion.LookRotation(ray);
                var bullet=obj.AddComponent<OfficeProjectile>();bullet.game=this;bullet.owner=unit.transform;
                bullet.direction=ray;bullet.speed=shotgun?23:20;bullet.range=range;bullet.damage=shotgun?5:10;bullet.radius=.045f;
            }
            audioBus.EnemyAttack(shotgun?"shotgun":"shot",unit.AimPoint,.45f);
        }
    }
}
