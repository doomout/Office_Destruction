using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private readonly List<OfficeDefender> executiveActors=new List<OfficeDefender>();
        private readonly Dictionary<OfficeDefender,Transform> executiveShields=new Dictionary<OfficeDefender,Transform>();
        private int executiveWave;
        private bool executiveEncounterBuilt,executiveAccessGranted;
        private float executiveIntermission;
        private int ExecutiveWaveLiving { get {int n=0;foreach(var u in executiveActors)if(u!=null&&u.Alive&&u.storyWave==executiveWave)n++;return n;} }
        private void ResetExecutiveEncounter()
        {
            executiveActors.Clear();executiveShields.Clear();executiveWave=0;executiveIntermission=0;
            executiveEncounterBuilt=executiveAccessGranted=false;
        }
        private void SetupExecutiveEncounter()
        {
            if(endlessMode||executiveEncounterBuilt||current.number!=3)return;
            executiveEncounterBuilt=true;
            var root=new GameObject("Executive encounter waves").transform;root.SetParent(world,false);
            var navigation=new OfficeNavigation(OfficeFixedFloor.Load(3).CreateLayout());
            for(int wave=0;wave<3;wave++)
            {
                for(int i=0;i<2;i++)
                {
                    var unit=requiredStoryEnemies[wave*2+i];unit.ConfigureExecutive(wave+1,wave,180+wave*40);
                    executiveActors.Add(unit);
                    var shield=new GameObject("Approval shield").transform;shield.SetParent(unit.transform,false);shield.localPosition=Vector3.up*.8f;
                    DrawExecutiveRing(shield,1.05f,Cyan);executiveShields.Add(unit,shield);
                }
                for(int i=0;i<4;i++)
                {
                    int role=i<2?4:i==2?5:6;
                    string title=role==4?"운전기사 · 수행원":role==5?"수행 비서":"결재 담당";
                    var unit=CreateDefender(root,current,navigation,new Vector3(0,.08f,current.start+8),title+" "+(wave+1)+(role==4?"-"+(i+1):""),role==4?80:role==5?65:90,false,false);
                    unit.ConfigureExecutive(role,wave,role==4?80:role==5?65:90);
                    unit.label.color=role==6?Cyan:Color.white;
                    if(role==4)Shape(unit.visual,"Driver baton",new Vector3(.5f,.9f,.4f),new Vector3(.12f,.12f,.8f),Orange,false);
                    if(role==5)Shape(unit.visual,"Document stack",new Vector3(.45f,1,.4f),new Vector3(.4f,.18f,.55f),Color.white,false);
                    if(role==6)Shape(unit.visual,"Approval device",new Vector3(.45f,1.2f,.3f),new Vector3(.4f,.5f,.12f),Cyan,false);
                    executiveActors.Add(unit);
                }
            }
            foreach(var unit in executiveActors)unit.gameObject.SetActive(false);
            ActivateExecutiveWave();
        }
        private void ActivateExecutiveWave()
        {
            int index=0;
            float center=Mathf.Clamp(player.position.z+11,current.start+9,current.end-7);
            if(center<player.position.z+5)center=Mathf.Max(current.start+8,player.position.z-10);
            foreach(var unit in executiveActors)
            {
                if(unit==null||!unit.Alive||unit.storyWave!=executiveWave)continue;
                Vector3 at=new Vector3((index%3-1)*5,.08f,center+(index/3)*3);
                at.x=Mathf.Clamp(at.x,-current.width*.5f+2,current.width*.5f-2);
                at.z=Mathf.Clamp(at.z,current.start+3,current.end-3);
                ClearStorySpace(at,1.5f);unit.Teleport(at);unit.gameObject.SetActive(true);index++;
            }
            string[] lines={"보고서 담당: ‘양식이 바뀌었네. 다시 해.’ · 결재 담당부터 처치하자.","정신력 담당: ‘힘들다는 건 마음가짐 문제야.’ · 원형 음파를 피하자.","책임 회피 담당: ‘내가 결정한 건 아니고…’ · 결재 보호막을 해제하자."};
            StoryLine(lines[executiveWave],7);executiveIntermission=2;
        }
        public bool ExecutiveShielded(OfficeDefender unit)
        {
            if(endlessMode||unit.storyRole<1||unit.storyRole>3||!executiveEncounterBuilt)return false;
            foreach(var provider in executiveActors)
                if(provider!=null&&provider.Alive&&provider.gameObject.activeInHierarchy&&provider.storyRole==6&&provider.storyWave==unit.storyWave)return true;
            return false;
        }
        private void TickExecutiveEncounter(float dt)
        {
            if(endlessMode||!RunActive||CombatLocked||current.number!=3||!executiveEncounterBuilt||executiveAccessGranted)return;
            foreach(var pair in executiveShields)
            {
                if(pair.Key==null||pair.Value==null)continue;
                bool shielded=ExecutiveShielded(pair.Key);pair.Value.gameObject.SetActive(shielded);
                pair.Value.Rotate(Vector3.up,60*dt);
                if(pair.Key.label!=null)pair.Key.label.text=pair.Key.displayName+"  "+Mathf.CeilToInt(pair.Key.Health)+" / "+pair.Key.MaxHealth+(shielded?"\n결재 담당 먼저 처치":"");
            }
            if(ExecutiveWaveLiving>0)return;
            executiveIntermission-=dt;if(executiveIntermission>0)return;
            // Do not carry a defeated wave's paper projectiles into the next encounter.
            foreach(var projectile in world.GetComponentsInChildren<OfficeProjectile>())
                if(projectile.owner!=null&&projectile.owner.GetComponent<OfficeDefender>()!=null){projectile.gameObject.SetActive(false);Destroy(projectile.gameObject);}
            if(executiveWave<2){executiveWave++;ActivateExecutiveWave();return;}
            executiveAccessGranted=true;
            StoryLine("‘주말 수당 지급 불가. 임원 골프는 근무 인정.’ · 대표실 결재 권한 획득!",10);
            RefreshExit(current);
        }
        private void ExecutiveDefeated(OfficeDefender unit)
        {
            if(endlessMode||unit.storyRole==0)return;
            if(unit.storyRole==6){StoryLine("결재 담당 처치 · 임원 보호막 해제!",4);return;}
            if(unit.storyRole<=3)
            {
                string[] lines={"이건 상무님 지시야.","전무님이 추진한 건데?","대표님 뜻이지, 내 뜻은 아니야."};
                StoryLine("임원: ‘"+lines[unit.storyWave]+"’",4);
            }
        }
        private void DrawExecutiveRing(Transform root,float radius,Color color)
        {
            for(int i=0;i<32;i++)
            {
                float angle=i*Mathf.PI*2/32;
                var part=Shape(root,"Ring segment",new Vector3(Mathf.Sin(angle)*radius,0,Mathf.Cos(angle)*radius),new Vector3(radius*.2f,.06f,.08f),color,false);
                part.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
            }
        }
        private void DrawPaperFanWarning(Transform root,float range,float angle)
        {
            for(int i=0;i<=12;i++)
            {
                float yaw=Mathf.Lerp(-angle*.5f,angle*.5f,i/12f);
                Vector3 ray=Quaternion.Euler(0,yaw,0)*Vector3.forward;
                var stripe=Shape(root,"Paper fan warning",ray*range*.5f,new Vector3(.06f,.03f,range),new Color(1,.8f,.15f),false);
                stripe.transform.localRotation=Quaternion.Euler(0,yaw,0);
            }
        }
        private void ReleasePaperFan(OfficeDefender unit,Vector3 direction,float range)
        {
            int count=unit.storyRole==1?7:5;float angle=unit.storyRole==1?70:60;
            Vector3 origin=unit.AimPoint+direction*.65f;
            for(int i=0;i<count;i++)
            {
                Vector3 ray=Quaternion.Euler(0,Mathf.Lerp(-angle*.5f,angle*.5f,i/(float)(count-1)),0)*direction;
                SpawnProjectile(origin,ray,unit.transform,false,false,10,range,unit.storyRole==1?12:8,1.3f);
            }
            audioBus.EnemyAttack("paper",origin,.3f);
        }
        private IEnumerator ExecutiveSonicWave(OfficeDefender unit)
        {
            Vector3 center=unit.transform.position;
            var ring=new GameObject("Expanding executive sonic wave").transform;ring.SetParent(world,false);ring.position=center+Vector3.up*.35f;
            DrawExecutiveRing(ring,1,Cyan);float radius=.5f;bool hit=false;
            audioBus.EnemyAttack("sonic",center,.3f);
            while(radius<8&&unit!=null&&unit.Alive&&current.number==3)
            {
                if(RunActive&&!CombatLocked)
                {
                    float previous=radius;radius=Mathf.Min(8,radius+Time.deltaTime*6);ring.localScale=new Vector3(radius,1,radius);
                    Vector3 delta=player.position-center;float distance=new Vector2(delta.x,delta.z).magnitude;
                    if(!hit&&distance>=previous-.6f&&distance<=radius+.6f&&Mathf.Abs(delta.y)<.7f&&ClearDefenseSight(center+Vector3.up,player.position+Vector3.up,unit.transform,null))
                    {hit=true;DefenderContact(15);}
                }
                yield return null;
            }
            if(ring!=null)Destroy(ring.gameObject);
        }
    }
}
