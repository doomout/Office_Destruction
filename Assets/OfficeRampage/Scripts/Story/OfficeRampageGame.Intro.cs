using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool submissionPlaying,submissionEnded;
        private string submissionDialogue="";
        private static readonly string[] OvertimeLines={"감히 어제 퇴근을 해?", "라떼는 말이야~ 사무실에서 살았어!", "주말이 왜 네 시간이야?", "이 정도는 열정으로 해야지!", "우린 가족 같은 회사잖아!", "야근 수당? 좋은 경험이잖아!", "그래도 내일은 정상 출근이야!", "월급은 동결인 거 알지?"};
        private void BeginIntro()
        {
            endlessMode=false;
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
            controller.enabled=false; player.position=new Vector3(0,.08f,(current.start+current.end)/2); avatar.rotation=Quaternion.identity;
            weapon.gameObject.SetActive(false); StartCoroutine(SubmissionScene());
        }
        private IEnumerator SubmissionScene()
        {
            int total=0;
            foreach(var rank in runCompany.ranks) total+=rank.people.Length;
            var crowd=new Transform[total]; var ranks=new int[total];
            int index=0;
            for(int rank=0;rank<runCompany.ranks.Length;rank++)
            for(int person=0;person<runCompany.ranks[rank].people.Length;person++,index++)
            {
                var actor=new GameObject("Overtime supervisor · "+runCompany.UnitName(rank,person)).transform; actor.SetParent(world);
                int ring=index<12?0:index<28?1:2;
                int slot=ring==0?index:ring==1?index-12:index-28;
                int count=ring==0?Mathf.Min(12,total):ring==1?Mathf.Min(16,total-12):total-28;
                float angle=slot*Mathf.PI*2/count+ring*.19f;
                actor.position=player.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(4.8f+ring*3.1f);
                actor.LookAt(player.position); crowd[index]=actor; ranks[index]=rank;
                var model=new GameObject("Supervisor appearance").transform; model.SetParent(actor,false);
                BuildPerson(model,rank==OfficeCompanySettings.RepresentativeRank);
                // Reuse the saved per-person appearance without enabling combat AI during this scripted ending.
                var unit=actor.gameObject.AddComponent<OfficeDefender>(); unit.enabled=false; unit.game=this; unit.visual=model;
                unit.label=new GameObject("Supervisor identity").AddComponent<TextMesh>(); unit.label.transform.SetParent(actor,false);
                ApplyCompanyIdentity(unit,rank,person); unit.label.gameObject.SetActive(false);
                Shape(model,rank==OfficeCompanySettings.RepresentativeRank?"Baton":"A4 hand",new Vector3(.48f,1,.5f),rank==OfficeCompanySettings.RepresentativeRank?new Vector3(.1f,.1f,1):new Vector3(.4f,.03f,.55f),rank==OfficeCompanySettings.RepresentativeRank?Orange:Color.white,false);
            }
            for(int wave=0;wave<15;wave++)
            {
                submissionDialogue=OvertimeLines[Mathf.Min(wave/2,OvertimeLines.Length-1)];
                for(int i=0;i<crowd.Length;i++)
                {
                    crowd[i].Find("Supervisor appearance").GetComponent<OfficeCharacterVisual>()?.PlayAction("attack-melee-right",true);
                    var actor=crowd[i]; Vector3 origin=actor.position+Vector3.up*1.3f;
                    Vector3 direction=(player.position+Vector3.up-origin).normalized;
                    foreach(Transform part in actor.Find("Supervisor appearance")) if(part.name=="Sleeve") part.localRotation=Quaternion.Euler(-100,0,0);
                    int attack=ranks[i]==OfficeCompanySettings.RepresentativeRank?wave%3:ranks[i]<=2?1:ranks[i]<=5?2:1+wave%2;
                    if(attack==0) Trail(origin,player.position+Vector3.up,Orange);
                    else if(attack==1)
                        for(int n=-2;n<=2;n++) Fx("Overtime sonic wave",origin,Vector3.one*.22f,Cyan,.5f,Quaternion.Euler(0,n*8,0)*direction*Vector3.Distance(origin,player.position+Vector3.up)/.5f,.4f,PrimitiveType.Sphere);
                    else Fx("Overtime A4",origin,new Vector3(.4f,.03f,.55f),Color.white,.5f,direction*Vector3.Distance(origin,player.position+Vector3.up)/.5f);
                }
                hp=Mathf.Max(0,100-(wave+1)*6); audioBus.Play("thud",.5f);
                avatar.localRotation=Quaternion.Euler(0,0,wave%2==0?12:-12);
                yield return SceneWait(.8f);
                foreach(var actor in crowd) foreach(Transform part in actor.Find("Supervisor appearance")) if(part.name=="Sleeve") part.localRotation=Quaternion.identity;
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
            view.transform.position=target+new Vector3(-14,16,-18); view.transform.LookAt(target);
        }
        private void DrawSubmission()
        {
            if(submissionPlaying && !paused)
            {
                Box(new Rect(150,22,980,76),new Color(.12f,.025f,.025f,.94f));
                GUI.Label(new Rect(180,33,920,56),"상사: “"+submissionDialogue+"”",menuHeading);
            }
            if(!submissionEnded) return;
            Box(new Rect(190,500,900,170),Navy);
            GUI.Label(new Rect(220,505,840,75),submissionEnded?"업무 스트레스로 사망":paused?"일시정지 · Esc로 계속":"상사들의 주말 업무 지시가 쏟아진다…",submissionEnded?menuTitle:menuHeading);
            if(submissionEnded)
            {
                if(MenuButton(new Rect(250,590,340,55),"첫 메뉴로")) ReturnToMainMenu();
                if(MenuButton(new Rect(660,590,350,55),"다시 선택하기",true)) BeginIntro();
            }
        }
    }
}
