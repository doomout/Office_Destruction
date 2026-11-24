using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform lobbyTerminal;
        private bool lobbyRequested,lobbyTerminalBroken;
        private int storyEncounterFloor;
        private readonly List<OfficeDefender> requiredStoryEnemies=new List<OfficeDefender>();
        private float storyMaxHealth=100;
        private int storySprintStacks;
        private bool storyRewardOpen;
        private int storyRewardFloor;
        private readonly bool[] storyRewardClaimed=new bool[4];
        private float StoryMaxHealth { get { return endlessMode?100:storyMaxHealth; } }
        private float StorySprintRecovery { get { return endlessMode?1:1+.25f*storySprintStacks; } }
        private int RequiredStoryEnemiesLeft
        { get { int n=0;foreach(var unit in requiredStoryEnemies)if(unit!=null&&unit.Alive)n++;return n; } }
        private void ResetStoryProgression()
        {
            ResetExecutiveEncounter();
            lobbyTerminal=null;lobbyRequested=lobbyTerminalBroken=storyRewardOpen=false;
            storyEncounterFloor=storyRewardFloor=storySprintStacks=0;storyMaxHealth=100;
            requiredStoryEnemies.Clear();System.Array.Clear(storyRewardClaimed,0,storyRewardClaimed.Length);
        }
        private void SetupStoryProgressionFloor()
        {
            if(endlessMode||current==null||storyEncounterFloor==current.number)return;
            storyEncounterFloor=current.number;requiredStoryEnemies.Clear();
            if(current.number==1)
            {
                foreach(var unit in defenders)if(unit!=null&&unit.room==1)
                {
                    unit.enabled=false;
                    if(requiredStoryEnemies.Count<3)
                    {
                        requiredStoryEnemies.Add(unit);unit.displayName="[보안 대응] "+unit.displayName;
                        unit.label.text=unit.displayName+"  "+Mathf.CeilToInt(unit.Health);unit.label.color=Orange;
                    }
                }
                lobbyTerminal=new GameObject("Lobby leave authorization terminal").transform;lobbyTerminal.SetParent(world,false);
                lobbyTerminal.position=new Vector3(3,0,current.start+7);
                var prop=lobbyTerminal.gameObject.AddComponent<OfficeProp>();prop.game=this;prop.room=1;prop.health=100;prop.kind=OfficeRules.Desk;prop.soundMaterial=2;prop.debrisColor=Cyan;
                lobbyTerminal.gameObject.AddComponent<OfficeStoryServer>().index=3;current.total++;
                Shape(lobbyTerminal,"Terminal stand",Vector3.up*.55f,new Vector3(.65f,1.1f,.65f),Navy);
                Shape(lobbyTerminal,"Terminal screen",new Vector3(0,1.5f,0),new Vector3(1.4f,.8f,.15f),Cyan);
                StoryLabel(lobbyTerminal,"퇴근 승인 단말기\nE · 퇴근 요청",Vector3.up*2.5f,Cyan,.065f);
                var poster=new GameObject("Happy company poster").transform;poster.SetParent(world,false);poster.position=new Vector3(-4,2.8f,current.start+.3f);
                StoryLabel(poster,"직원이 행복한 회사\n자유로운 퇴근은 사전 승인 후 가능합니다.",Vector3.zero,Cyan,.06f);
                StoryLine("퇴근 버튼부터 눌러 보자. 단말기에 다가가 E.",7);
            }
            if(current.number==3)
            {
                foreach(var unit in defenders)if(unit!=null&&unit.room==3&&!unit.dog&&!unit.guard)
                {
                    requiredStoryEnemies.Add(unit);unit.displayName="[지정 임원] "+unit.displayName;
                    unit.label.text=unit.displayName+"  "+Mathf.CeilToInt(unit.Health);unit.label.color=Orange;
                }
                SetupExecutiveEncounter();
            }
        }
        public void RequestLobbyLeave()
        {
            if(endlessMode||!RunActive||CombatLocked||current==null||current.number!=1||lobbyRequested)return;
            lobbyRequested=true;
            foreach(var unit in defenders)if(unit!=null&&unit.room==1)unit.enabled=true;
            if(lobbyTerminal!=null)
                foreach(var label in lobbyTerminal.GetComponentsInChildren<TextMesh>())label.text="퇴근 요청 거절\n단말기를 파괴하세요";
            StoryLine("퇴근 거부 · ‘근무 태도 이상자’ 등록. 보안 대응 인원과 단말기를 정리하자.",9);
            audioBus.EnemyAttack("sonic",player.position,.2f);
        }
        private void DestroyLobbyTerminal()
        {
            if(endlessMode||current==null||current.number!=1||lobbyTerminalBroken)return;
            RequestLobbyLeave();lobbyTerminalBroken=true;
            StoryLine("단말기 기록: ‘퇴근 승인 담당 — 2층 운영팀’",8);RefreshExit(current);
        }
        private bool StoryExitReady(int floor)
        {
            if(endlessMode||storyEncounterFloor!=floor)return false;
            if(floor==1)return lobbyRequested&&lobbyTerminalBroken&&RequiredStoryEnemiesLeft==0;
            if(floor==2)return StoryFloorReady;
            if(floor==3)return executiveAccessGranted&&RequiredStoryEnemiesLeft==0;
            return false;
        }
        private void TickStoryProgression()
        {
            if(current.number==1&&lobbyTerminal!=null)
            {
                foreach(var label in lobbyTerminal.GetComponentsInChildren<TextMesh>())label.transform.rotation=view.transform.rotation;
                if(!lobbyRequested&&Vector3.Distance(player.position,lobbyTerminal.position)<3&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame)RequestLobbyLeave();
            }
            TickExecutiveEncounter(Time.deltaTime);
            if(current.number<4)RefreshExit(current);
        }
        private void OpenElevatorReward(int floor)
        {
            if(endlessMode||floor<1||floor>3||storyRewardClaimed[floor])return;
            storyRewardFloor=floor;storyRewardOpen=true;Time.timeScale=0;LockCursor(false);audioBus.SetPaused(true);
        }
        private void SyncStoryPause()
        {
            bool stopped=paused||storyRewardOpen;
            Time.timeScale=stopped?0:1;LockCursor(!stopped);audioBus.SetPaused(stopped);
        }
        private bool ChooseElevatorReward(int choice)
        {
            if(!storyRewardOpen||!RunActive||endlessMode||choice<0||choice>2||storyRewardClaimed[storyRewardFloor])return false;
            storyRewardClaimed[storyRewardFloor]=true;
            if(choice==0)ResetAmmo();
            if(choice==1){storyMaxHealth=Mathf.Min(160,storyMaxHealth+20);hp=Mathf.Min(storyMaxHealth,hp+40);}
            if(choice==2)storySprintStacks=Mathf.Min(3,storySprintStacks+1);
            storyRewardOpen=false;Time.timeScale=1;LockCursor(true);audioBus.SetPaused(false);return true;
        }
        private void CancelElevatorReward()
        {
            if(!storyRewardOpen)return;
            storyRewardOpen=false;StopAllCoroutines();combatLocked=false;
        }
        private void DrawElevatorReward()
        {
            Box(new Rect(0,0,1280,720),new Color(.015f,.025f,.045f,.95f));
            GUI.Label(new Rect(100,110,1080,65),"엘리베이터 · "+storyRewardFloor+"층 → "+(storyRewardFloor+1)+"층",menuTitle);
            GUI.Label(new Rect(100,190,1080,45),"보상 하나를 선택하세요. 이번 플레이 동안 적용됩니다.",menuText);
            string[] titles={"법인카드","산재 처리","퇴근 본능"};
            string[] descriptions={"권총 200 · 샷건 100\n로켓 20발로 보충\n비밀 무기·더미 제외","최대 체력 +20\n체력 +40 회복\n최대 체력 상한 160","달리기 회복 속도 +25%\n반복 선택 시 합산\n최대 +75%"};
            for(int i=0;i<3;i++)
            {
                float x=100+i*370;Box(new Rect(x,270,340,260),Navy);
                GUI.Label(new Rect(x+20,288,300,48),titles[i],menuHeading);
                GUI.Label(new Rect(x+20,350,300,105),descriptions[i],menuText);
                if(MenuButton(new Rect(x+20,465,300,48),"선택",true)){ChooseElevatorReward(i);return;}
            }
            GUI.Label(new Rect(100,565,1080,36),"현재 최대 체력 "+storyMaxHealth+" · 달리기 회복 +"+(storySprintStacks*25)+"%  |  Esc: 일시정지",menuSmall);
        }
    }
}
