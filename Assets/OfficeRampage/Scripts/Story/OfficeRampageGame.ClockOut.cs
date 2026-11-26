using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool storySkipRequested,clockOutPlaying,clockOutApproved,clockOutPhoneShown;
        private float clockOutAge;
        private Transform clockOutRoot,clockOutSecretary,clockOutCoworker,clockOutPhone,clockOutLeft,clockOutRight;
        private TextMesh clockOutSign;
        private const float ClockOutDuration=11;
        private bool StoryCinematicActive { get {return !endlessMode&&RunActive&&current!=null&&current.number==4&&combatLocked;} }
        private float StoryCinematicDelta { get {return StoryCinematicActive&&storySkipRequested?100:Time.deltaTime;} }
        private string ClockOutCaption { get {return clockOutAge<2?"마지막으로, 퇴근 버튼을 누른다.":clockOutAge<4?"퇴근이 승인되었습니다.":clockOutAge<7?"…월요일 오전 9시 긴급회의?":"오늘은 여기까지. 전화는 뒤집어 두자.";} }
        private void ResetClockOut()
        {
            storySkipRequested=clockOutPlaying=clockOutApproved=clockOutPhoneShown=false;clockOutAge=0;
            clockOutRoot=clockOutSecretary=clockOutCoworker=clockOutPhone=clockOutLeft=clockOutRight=null;clockOutSign=null;
        }
        private void TickStoryCinematicInput(float dt)
        {
            if(!StoryCinematicActive){storySkipRequested=false;return;}
            if(Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)storySkipRequested=true;
            storyLineLeft=Mathf.Max(0,storyLineLeft-dt);
        }
        private void DrawStorySkipButton()
        {
            if(!StoryCinematicActive)return;
            if(MenuButton(new Rect(920,112,330,36),storySkipRequested?"장면 마무리 중…":"Space · 연출 건너뛰기"))storySkipRequested=true;
        }
        private void DrawStoryCinematicScreen()
        {
            if(!StoryCinematicActive)return;
            EnsureMenuStyles();Matrix4x4 before=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2),Quaternion.identity,Vector3.one*scale);
            if(storyLineLeft>0)
            {
                Box(new Rect(40,24,1200,60),new Color(.015f,.025f,.045f,.8f));
                GUI.Label(new Rect(60,33,1160,44),storyLine,menuText);
            }
            DrawStorySkipButton();GUI.matrix=before;
        }
        private IEnumerator ClockOutScene()
        {
            clockOutPlaying=true;clockOutAge=0;combatLocked=true;finaleCamera=false;mushroomCamera=false;flash=0;
            foreach(Transform child in world)
                if(child!=player&&child!=view.transform&&child.GetComponent<Light>()==null)child.gameObject.SetActive(false);
            controller.enabled=false;weapon.gameObject.SetActive(false);
            player.position=new Vector3(0,.08f,3);avatar.localRotation=Quaternion.identity;avatar.localScale=Vector3.one;
            clockOutRoot=new GameObject("Clock out ending").transform;clockOutRoot.SetParent(world,false);
            RenderSettings.fog=false;RenderSettings.ambientLight=new Color(.7f,.75f,.8f);view.backgroundColor=new Color(.56f,.76f,.86f);view.farClipPlane=120;
            Shape(clockOutRoot,"Exit walkway",new Vector3(0,-.2f,9),new Vector3(16,.4f,30),new Color(.35f,.42f,.46f));
            for(int side=-1;side<=1;side+=2)
            {
                Shape(clockOutRoot,"Office wall",new Vector3(side*5,2.5f,8),new Vector3(6,5,.4f),Navy);
                Shape(clockOutRoot,"Door post",new Vector3(side*1.8f,1.6f,8),new Vector3(.2f,3.2f,.4f),Cyan);
                for(int i=0;i<4;i++)Shape(clockOutRoot,"Morning city",new Vector3(side*(12+i*4),3+i,25+i*3),new Vector3(3,6+i*2,4),new Color(.23f,.36f,.46f),false);
            }
            clockOutLeft=Shape(clockOutRoot,"Exit left",new Vector3(-.85f,1.5f,8),new Vector3(1.7f,3,.16f),new Color(.2f,.5f,.6f)).transform;
            clockOutRight=Shape(clockOutRoot,"Exit right",new Vector3(.85f,1.5f,8),new Vector3(1.7f,3,.16f),new Color(.2f,.5f,.6f)).transform;
            Shape(clockOutRoot,"Clock out button",new Vector3(.75f,1.15f,5),new Vector3(.4f,.35f,.3f),Cyan,false);
            clockOutSign=StoryLabel(clockOutRoot,"퇴근 승인 대기",new Vector3(0,3.8f,8),Color.white,.11f);
            clockOutSecretary=CreateClockOutCompanion("퇴근하는 비서",true,1);
            if(storyRescued)clockOutCoworker=CreateClockOutCompanion("구출한 동료",false,2);
            clockOutPhone=Shape(player,"Phone face down",new Vector3(.4f,1.05f,.35f),new Vector3(.2f,.025f,.35f),Navy,false).transform;
            Shape(clockOutPhone,"Phone screen",new Vector3(0,.6f,0),new Vector3(.8f,.08f,.85f),Cyan,false);
            clockOutPhone.gameObject.SetActive(false);
            while(clockOutAge<ClockOutDuration)
            {
                if(RunActive)
                {
                    clockOutAge=Mathf.Min(ClockOutDuration,clockOutAge+StoryCinematicDelta);
                    PoseClockOut();UpdateClockOutCamera();
                }
                yield return null;
            }
            clockOutPlaying=false;endingPlaying=false;combatLocked=false;storySkipRequested=false;End(true);
        }
        private Transform CreateClockOutCompanion(string name,bool female,int seed)
        {
            var root=new GameObject(name).transform;root.SetParent(clockOutRoot,false);BuildPerson(root,false);
            root.GetComponent<OfficeCharacterVisual>()?.SetIdentity(female,seed,0);
            StoryLabel(root,name,Vector3.up*2.2f,Cyan,.05f);return root;
        }
        private void PoseClockOut()
        {
            if(clockOutAge>=2&&!clockOutApproved)
            {clockOutApproved=true;clockOutSign.text="퇴근이 승인되었습니다.";clockOutSign.color=Cyan;audioBus.Play("door",.45f);}
            if(clockOutAge>=4&&!clockOutPhoneShown){clockOutPhoneShown=true;audioBus.Play("combo",.25f);}
            float open=Mathf.Clamp01((clockOutAge-2)/1.2f);
            clockOutLeft.localPosition=new Vector3(-.85f-open*1.7f,1.5f,8);clockOutRight.localPosition=new Vector3(.85f+open*1.7f,1.5f,8);
            Vector3 old=player.position;
            float z=clockOutAge<2?Mathf.Lerp(3,5,clockOutAge/2):Mathf.Lerp(5,17,Mathf.Clamp01((clockOutAge-7)/4));
            player.position=new Vector3(0,.08f,z);avatar.rotation=Quaternion.identity;
            AnimatePlayerRun(Time.deltaTime,player.position-old);
            clockOutPhone.gameObject.SetActive(clockOutAge>=4&&clockOutAge<9);
            clockOutPhone.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(0,180,Mathf.Clamp01((clockOutAge-6)/1)));
            clockOutSecretary.position=player.position+new Vector3(1.3f,0,-1.3f);
            if(clockOutCoworker!=null)clockOutCoworker.position=player.position+new Vector3(-1.3f,0,-2.2f);
            foreach(var label in clockOutRoot.GetComponentsInChildren<TextMesh>())label.transform.rotation=view.transform.rotation;
        }
        private void UpdateClockOutCamera()
        {
            view.fieldOfView=58;
            view.transform.position=Vector3.Lerp(new Vector3(-7,5,-2),player.position+new Vector3(-6,5,-4),Mathf.Clamp01((clockOutAge-7)/2));
            view.transform.LookAt(new Vector3(0,1.3f,Mathf.Max(7,player.position.z)));
        }
    }
}
