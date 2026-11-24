using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private readonly bool[] storyServers=new bool[3];
        private readonly List<OfficeDefender> storyGuards=new List<OfficeDefender>();
        private readonly List<OfficeDefender> storyReinforcements=new List<OfficeDefender>();
        private readonly Transform[] storyServerRoots=new Transform[3];
        private Transform storyRoot,storyCoworker,storyCellDoor;
        private TextMesh storyCoworkerLabel;
        private bool storyFloorBuilt,storyRescued,storyNextReward,storyFinalReward;
        private float storySpawnClock,storyPaperClock,storyLineLeft;
        private string storyLine="";
        private int StoryServerCount { get { int n=0;foreach(bool done in storyServers)if(done)n++;return n; } }
        private int StoryGuardsLeft { get { int n=0;foreach(var u in storyGuards)if(u!=null&&u.Alive)n++;return n; } }
        private bool StoryFloorReady { get { return storyFloorBuilt&&StoryServerCount==3&&StoryGuardsLeft==0; } }
        private void StoryLine(string text,float seconds=5){storyLine=text;storyLineLeft=seconds;}
        private void ResetStoryObjectives()
        {
            storyFloorBuilt=storyRescued=storyNextReward=storyFinalReward=false;
            storyGuards.Clear();storyReinforcements.Clear();System.Array.Clear(storyServers,0,3);System.Array.Clear(storyServerRoots,0,3);
            storyRoot=storyCoworker=storyCellDoor=null;storyCoworkerLabel=null;storyLine="";storyLineLeft=0;
        }
        private void SetupStoryFloor()
        {
            if(endlessMode||current==null)return;
            if(current.number==3&&storyRescued&&!storyNextReward)
            {
                storyNextReward=true;SpawnStoryHealing(player.position+Vector3.forward*2);
                StoryLine("구출한 동료가 회복 보급을 보내왔다.");
            }
            if(current.number!=2||storyFloorBuilt)return;
            storyFloorBuilt=true;storySpawnClock=9;storyPaperClock=3;
            storyRoot=new GameObject("Story objectives · 2층").transform;storyRoot.SetParent(world,false);
            float edge=current.width*.5f-2.2f;
            Vector3[] positions={new Vector3(-edge,0,current.start+5),new Vector3(edge,0,(current.start+current.end)*.5f),new Vector3(-edge,0,current.end-6)};
            string[] names={"업무 배정 서버","보고서 반려 서버","근태 조작 서버"};
            for(int i=0;i<3;i++)
            {
                ClearStorySpace(positions[i],2.4f);
                var rack=new GameObject(names[i]).transform;rack.SetParent(storyRoot,false);rack.position=positions[i];storyServerRoots[i]=rack;
                var prop=rack.gameObject.AddComponent<OfficeProp>();prop.game=this;prop.room=2;prop.health=100;prop.debrisColor=Cyan;prop.soundMaterial=2;prop.kind=OfficeRules.Desk;
                rack.gameObject.AddComponent<OfficeStoryServer>().index=i;current.total++;
                Shape(rack,"Server cabinet",new Vector3(0,1.25f,0),new Vector3(1.3f,2.5f,1),Navy);
                for(int n=0;n<6;n++)Shape(rack,"Active rack",new Vector3(0,.35f+n*.33f,-.515f),new Vector3(1.05f,.14f,.04f),i==1?Orange:Cyan,false);
                StoryLabel(rack,names[i],new Vector3(0,3,0),Cyan,.07f);
                Shape(rack,"Target beacon",new Vector3(0,3.5f,0),Vector3.one*.35f,Orange,false,PrimitiveType.Sphere);
            }
            // Three existing defenders become the required encounter. Additional staff and reinforcements are optional.
            foreach(var unit in defenders)
                if(unit!=null&&unit.room==2&&storyGuards.Count<3)
                {
                    int index=storyGuards.Count;storyGuards.Add(unit);
                    unit.displayName="[방어 담당] "+unit.displayName;
                    unit.Teleport(positions[index]+Vector3.forward*2);
                    if(unit.label!=null){unit.label.color=Orange;unit.label.text=unit.displayName+"  "+Mathf.CeilToInt(unit.Health)+" / "+unit.MaxHealth;}
                }
            Vector3 cell=new Vector3(edge-1,0,current.end-5);
            ClearStorySpace(cell,3.5f);
            var area=new GameObject("Coworker detention").transform;area.SetParent(storyRoot,false);area.position=cell;
            for(int side=-1;side<=1;side+=2)Shape(area,"Cell side",new Vector3(side*2,1.25f,0),new Vector3(.15f,2.5f,4),new Color(.18f,.3f,.35f));
            Shape(area,"Cell back",new Vector3(0,1.25f,2),new Vector3(4,2.5f,.15f),Navy);
            storyCellDoor=Shape(area,"Locked exit",new Vector3(0,1.25f,-2),new Vector3(4,2.5f,.12f),new Color(.45f,.12f,.06f)).transform;
            storyCoworker=new GameObject("Trapped coworker").transform;storyCoworker.SetParent(area,false);BuildPerson(storyCoworker,false);
            storyCoworkerLabel=StoryLabel(storyCoworker,"나도 나가고 싶은데…\n퇴근 버튼이 회색이야.",Vector3.up*2.6f,Cyan,.055f);
            StoryLine("서버가 업무와 공격을 반복시킨다. 서버 3개를 파괴하자.",7);
        }
        private void ClearStorySpace(Vector3 center,float radius)
        {
            foreach(var prop in world.GetComponentsInChildren<OfficeProp>())
                if(prop.GetComponent<OfficeStoryServer>()==null&&Vector2.Distance(new Vector2(prop.transform.position.x,prop.transform.position.z),new Vector2(center.x,center.z))<radius)
                {prop.gameObject.SetActive(false);Destroy(prop.gameObject);current.total--;}
        }
        private TextMesh StoryLabel(Transform parent,string text,Vector3 position,Color color,float size)
        {
            var label=new GameObject("Story label").AddComponent<TextMesh>();label.transform.SetParent(parent,false);label.transform.localPosition=position;
            OfficeGameFont.Apply(label);label.text=text;label.fontSize=48;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.color=color;return label;
        }
        public void StoryServerDestroyed(int index)
        {
            if(endlessMode||!storyFloorBuilt||current==null||current.number!=2||index<0||index>=3||storyServers[index])return;
            storyServers[index]=true;
            if(index==0)StoryLine("업무 배정 중단 · 더 이상 증원되지 않는다.");
            if(index==1)
            {
                foreach(var shot in world.GetComponentsInChildren<OfficeProjectile>())if(shot.owner==storyServerRoots[1])Destroy(shot.gameObject);
                StoryLine("자동 반려 중단 · 서류 공격 지원 장치 정지.");
            }
            if(index==2)
            {
                if(storyCellDoor!=null){storyCellDoor.gameObject.SetActive(false);Destroy(storyCellDoor.gameObject);}
                if(storyCoworkerLabel!=null)storyCoworkerLabel.text="출구가 열렸어!\n다가와서 E · 구출";
                StoryLine("근태 조작 해제 · 동료에게 다가가 E로 구출할 수 있다.");
            }
            if(StoryServerCount==3)StoryLine("발견한 결의서: ‘주말 출근 자동 승인 — 임원회의 결의’ · 3층으로 가자.",10);
            RefreshExit(current);
        }
        private void TickStoryObjectives(float dt)
        {
            if(endlessMode||!RunActive||CombatLocked)return;
            storyLineLeft=Mathf.Max(0,storyLineLeft-dt);
            if(storyRescued&&current.number==4&&bossFight&&!storyFinalReward&&hp<=70)
            {storyFinalReward=true;hp=Mathf.Min(100,hp+30);StoryLine("구출한 동료: 이번엔 내가 도울게! · 체력 +30");audioBus.Play("combo",.45f);}
            if(current.number!=2||!storyFloorBuilt)return;
            if(storyRoot!=null)foreach(var label in storyRoot.GetComponentsInChildren<TextMesh>())label.transform.rotation=view.transform.rotation;
            if(!storyServers[0])
            {
                storySpawnClock-=dt;
                if(storySpawnClock<=0)
                {
                    storySpawnClock=9;storyReinforcements.RemoveAll(u=>u==null||!u.Alive);
                    if(storyReinforcements.Count<2)
                    {
                        var navigation=new OfficeNavigation(OfficeFixedFloor.Load(2).CreateLayout());
                        var unit=CreateDefender(storyRoot,current,navigation,storyServerRoots[0].position+Vector3.forward*2,"재배정된 업무 담당",70,false,false);
                        storyReinforcements.Add(unit);
                    }
                }
            }
            if(!storyServers[1]&&storyServerRoots[1]!=null)
            {
                storyPaperClock-=dt;
                if(storyPaperClock<=0)
                {
                    storyPaperClock=3;
                    Vector3 origin=storyServerRoots[1].position+Vector3.up*1.5f;
                    Vector3 direction=(player.position+Vector3.up-origin).normalized;
                    for(int i=-1;i<=1;i++)SpawnProjectile(origin+direction,Quaternion.Euler(0,i*9,0)*direction,storyServerRoots[1],false,false,9,60,6,1);
                    audioBus.EnemyAttack("paper",origin,.2f);
                }
            }
            if(storyServers[2]&&!storyRescued&&storyCoworker!=null&&Vector3.Distance(player.position,storyCoworker.position)<3&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame)RescueStoryCoworker();
            RefreshExit(current);
        }
        private bool RescueStoryCoworker()
        {
            if(endlessMode||!RunActive||CombatLocked||current.number!=2||!storyServers[2]||storyRescued||storyCoworker==null||Vector3.Distance(player.position,storyCoworker.position)>=3)return false;
            storyRescued=true;storyCoworker.gameObject.SetActive(false);
            StoryLine("동료 구출! ‘다음 층에 회복 보급을 보내둘게.’",6);audioBus.Play("combo",.45f);return true;
        }
        private void SpawnStoryHealing(Vector3 at)
        {
            var potion=new GameObject("Rescued coworker healing supply").AddComponent<OfficeHealingPotion>();potion.transform.SetParent(world,false);potion.transform.position=at;potion.game=this;potion.room=current.number;
            potion.visual=new GameObject("Gift bottle").transform;potion.visual.SetParent(potion.transform,false);potion.visual.localPosition=Vector3.up*.65f;
            Shape(potion.visual,"Healing bottle",Vector3.zero,new Vector3(.4f,.6f,.4f),Color.green,false,PrimitiveType.Capsule);
            StoryLabel(potion.visual,"동료의 회복 보급",Vector3.up*.8f,Cyan,.06f);
        }
        private void DrawStoryObjectives()
        {
            if(endlessMode||paused||ended||submissionPlaying||submissionEnded)return;
            if(storyLineLeft>0)
            {
                Box(new Rect(300,213,950,39),new Color(.025f,.045f,.07f,.9f));
                GUI.Label(new Rect(315,219,920,28),storyLine,new GUIStyle(smallStyle){fontSize=17,alignment=TextAnchor.MiddleCenter});
            }
            if(current.number!=2||!storyFloorBuilt)return;
            Box(new Rect(22,213,258,52),Navy);
            for(int i=0;i<3;i++)
            {
                Color color=storyServers[i]?Cyan:Orange;float x=34+i*40;
                Box(new Rect(x,224,22,27),color);for(int j=0;j<3;j++)Box(new Rect(x+3,228+j*7,16,2),Navy);
            }
            GUI.Label(new Rect(154,224,45,30),StoryServerCount+"/3",smallStyle);
            HudIcon(new Rect(204,225,24,24),HudSymbol.Heart,storyRescued?Cyan:Color.gray);
            GUI.Label(new Rect(234,225,35,28),storyRescued?"1/1":"0/1",smallStyle);
            if(storyServers[2]&&!storyRescued&&storyCoworker!=null&&Vector3.Distance(player.position,storyCoworker.position)<3)
                GUI.Label(new Rect(470,550,340,35),"E · 동료 구출",centeredStyle);
        }
    }
}
