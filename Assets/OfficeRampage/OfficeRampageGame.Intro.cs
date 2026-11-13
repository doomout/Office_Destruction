using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool submissionPlaying,submissionEnded;
        private void BeginIntro()
        {
            menuPage=MenuPage.Intro; paused=true; Time.timeScale=0; LockCursor(false);
            audioBus.SetPaused(true);
        }
        private void DrawIntro()
        {
            GUI.Label(new Rect(110,235,1060,180),"어제도 빡치게 야근했다.\n심지어 오늘은 주말인데 출근하라고 한다.\n도저히 참을 수 없다.",menuHeading);
            if(MenuButton(new Rect(110,480,480,75),"닥치고 출근하기")) BeginSubmission();
            if(MenuButton(new Rect(650,480,520,75),"사무실 파괴하기",true)) ResetRun();
            if(MenuButton(new Rect(460,590,360,48),"뒤로")) ReturnToMainMenu();
        }
        private void BeginSubmission()
        {
            ResetRun(); submissionPlaying=true; combatLocked=true; ClearDecoy(); LockCursor(false);
            foreach(var unit in defenders) if(unit!=null) { unit.gameObject.SetActive(false); Destroy(unit.gameObject); }
            defenders.Clear();
            foreach(var prop in world.GetComponentsInChildren<OfficeProp>()) { prop.gameObject.SetActive(false); Destroy(prop.gameObject); }
            foreach(var potion in world.GetComponentsInChildren<OfficeHealingPotion>()) Destroy(potion.gameObject);
            controller.enabled=false; player.position=new Vector3(0,.08f,10); avatar.rotation=Quaternion.identity;
            weapon.gameObject.SetActive(false); StartCoroutine(SubmissionScene());
        }
        private IEnumerator SubmissionScene()
        {
            var crowd=new Transform[6];
            for(int i=0;i<crowd.Length;i++)
            {
                var actor=new GameObject("Overtime supervisor").transform; actor.SetParent(world);
                float angle=i*Mathf.PI/3; actor.position=player.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*3.5f;
                actor.LookAt(player.position); BuildPerson(actor,i>=4); crowd[i]=actor;
                Shape(actor,i%3==0?"Baton":"A4 hand",new Vector3(.48f,1,.5f),i%3==0?new Vector3(.1f,.1f,1):new Vector3(.4f,.03f,.55f),i%3==0?Orange:Color.white,false);
            }
            for(int wave=0;wave<6;wave++)
            {
                for(int i=0;i<crowd.Length;i++)
                {
                    var actor=crowd[i]; Vector3 origin=actor.position+Vector3.up*1.3f;
                    Vector3 direction=(player.position+Vector3.up-origin).normalized;
                    foreach(Transform part in actor) if(part.name=="Sleeve") part.localRotation=Quaternion.Euler(-100,0,0);
                    if(i%3==0) Trail(origin,player.position+Vector3.up,Orange);
                    else if(i%3==1)
                        for(int n=-2;n<=2;n++) Fx("Overtime sonic wave",origin,Vector3.one*.22f,Cyan,.5f,Quaternion.Euler(0,n*8,0)*direction*7,.4f,PrimitiveType.Sphere);
                    else Fx("Overtime A4",origin,new Vector3(.4f,.03f,.55f),Color.white,.5f,direction*7);
                }
                hp=Mathf.Max(0,100-(wave+1)*16); audioBus.Play("thud",.5f);
                avatar.localRotation=Quaternion.Euler(0,0,wave%2==0?12:-12);
                yield return SceneWait(.8f);
                foreach(var actor in crowd) foreach(Transform part in actor) if(part.name=="Sleeve") part.localRotation=Quaternion.identity;
            }
            hp=0;
            for(float t=0;t<1;)
            { if(RunActive) { t=Mathf.Min(1,t+Time.deltaTime); avatar.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(0,88,t)); } yield return null; }
            submissionPlaying=false; submissionEnded=true; combatLocked=false; ended=true; won=false;
            audioBus.Play("lose",.6f); LockCursor(false);
        }
        private void UpdateSubmissionCamera()
        {
            Vector3 target=player.position+Vector3.up*.8f; view.fieldOfView=60;
            view.transform.position=target+new Vector3(-6,5,-8); view.transform.LookAt(target);
        }
        private void DrawSubmission()
        {
            Box(new Rect(190,500,900,170),Navy);
            GUI.Label(new Rect(220,515,840,45),submissionEnded?"게임 끝 · 오늘도 고분고분하게 출근했다.":paused?"일시정지 · Esc로 계속":"상사들의 주말 업무 지시가 쏟아진다…",menuHeading);
            if(submissionEnded)
            {
                if(MenuButton(new Rect(250,590,340,55),"첫 메뉴로")) ReturnToMainMenu();
                if(MenuButton(new Rect(660,590,350,55),"다시 선택하기",true)) BeginIntro();
            }
        }
    }
}
