using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool endingPlaying;
        private string endingLine = "";
        private static readonly string[] EndingLines = {
            "그래도 자네 월급은 동결이야.",
            "정규직 시켜주려고 했는데…",
            "부순 집기는 자네 월급에서 공제하지.",
            "내일은 정상 출근인 거 알지?",
            "이 열정으로 일을 했으면 벌써 승진했겠군.",
            "퇴근 전에 보고서만 하나 올려줘.",
            "야근 수당은 마음으로 지급하겠네.",
            "자네, 연차는 승인 안 했는데?"
        };
        private void ResetEnding() { endingPlaying = false; endingLine = ""; ResetClockOut(); }
        private void BeginBossEnding(OfficeDefender unit)
        {
            if(endingPlaying || ended) return;
            ClearDecoy(); whirlwindLeft=whirlwindReady=0;
            storySkipRequested=false;
            endingPlaying=true; combatLocked=true;
            endingLine=EndingLines[Random.Range(0,EndingLines.Length)];
            if(unit.label!=null) unit.label.gameObject.SetActive(false);
            if(unit.flames!=null) unit.flames.gameObject.SetActive(false);
            var heart=unit.transform.Find("Attraction heart"); if(heart!=null) heart.gameObject.SetActive(false);
            var burn=unit.GetComponent<OfficeBurn>(); if(burn!=null) Destroy(burn);
            LockCursor(false); StartCoroutine(BossCollapse(unit));
        }
        private IEnumerator BossCollapse(OfficeDefender unit)
        {
            if(unit.PhaseTwo)
            {
                if(!bossEjected) { finaleCamera=true;yield return ShatterRobotAndEject(unit);finaleCamera=false; }
                UpdateEndingCamera();yield return SceneWait(1);
                yield return ClockOutScene();yield break;
            }
            Quaternion initial=unit.visual.localRotation; float elapsed=0;
            while(elapsed<1.6f)
            {
                if(RunActive)
                {
                    elapsed+=StoryCinematicDelta;
                    float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/1.6f));
                    unit.visual.localRotation=Quaternion.Slerp(initial,Quaternion.Euler(0,0,88),t);
                    UpdateEndingCamera();
                }
                yield return null;
            }
            yield return ClockOutScene();
        }
        private void UpdateEndingCamera()
        {
            if(clockOutRoot!=null){UpdateClockOutCamera();return;}
            if(bossUnit==null) return;
            Vector3 target=bossUnit.transform.position+Vector3.up*.7f;
            if(bossEjected) { view.fieldOfView=55;view.transform.position=target+new Vector3(-5,5,-7);view.transform.LookAt(target);return; }
            view.fieldOfView=55; view.transform.position=target+(bossUnit.PhaseTwo?new Vector3(-10,32,-14):new Vector3(-5,2.6f,-6));
            view.transform.LookAt(target);
        }
        private void ConfirmEnding() { ReturnToMainMenu(); }
        private void ReplayEnding() { ResetRun(); }
        private void DrawEndingScreen()
        {
            EnsureMenuStyles();
            if(endingPlaying)
            {
                Box(new Rect(100,26,1080,62),new Color(.015f,.025f,.045f,.9f));
                GUI.Label(new Rect(120,34,1040,48),paused?"일시정지 · Esc로 계속":clockOutPlaying?ClockOutCaption:"대표: “"+endingLine+"”",menuText);
                if(clockOutPlaying&&clockOutAge>=4&&clockOutAge<7)
                {
                    Box(new Rect(820,155,360,150),Navy);
                    GUI.Label(new Rect(845,175,310,105),"업무 메시지\n월요일 오전 9시 긴급회의.",menuText);
                }
                DrawStorySkipButton();return;
            }
            Box(new Rect(120,420,1040,245),new Color(.015f,.025f,.045f,.92f));
            GUI.Label(new Rect(150,435,980,48),"퇴근이 승인되었습니다.",menuHeading);
            GUI.Label(new Rect(150,493,980,50),"점수 "+score.ToString("N0")+(storyRescued?"  ·  동료와 비서도 함께 퇴근했습니다.":"  ·  비서와 함께 회사를 떠납니다."),menuText);
            if(MenuButton(new Rect(150,575,260,55),"확인"))ConfirmEnding();
            if(MenuButton(new Rect(440,575,260,55),"재게임",true))ReplayEnding();
            if(MenuButton(new Rect(730,575,400,55),"무한 파괴로 스트레스 더 풀기"))BeginEndless();
        }
        private void ApplyKoreanWorldFont(Transform root)
        {
            if(defenderFont==null) defenderFont=OfficeGameFont.Shared;
            foreach(var text in root.GetComponentsInChildren<TextMesh>(true))
            { OfficeGameFont.Apply(text); }
        }
    }
}
