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
        private void ResetEnding() { endingPlaying = false; endingLine = ""; }
        private void BeginBossEnding(OfficeDefender unit)
        {
            if(endingPlaying || ended) return;
            ClearDecoy(); whirlwindLeft=whirlwindReady=0;
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
                endingPlaying=false;combatLocked=false;End(true);yield break;
            }
            Quaternion initial=unit.visual.localRotation; float elapsed=0;
            while(elapsed<1.6f)
            {
                if(RunActive)
                {
                    elapsed+=Time.deltaTime;
                    float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/1.6f));
                    unit.visual.localRotation=Quaternion.Slerp(initial,Quaternion.Euler(0,0,88),t);
                    UpdateEndingCamera();
                }
                yield return null;
            }
            endingPlaying=false; combatLocked=false; End(true);
        }
        private void UpdateEndingCamera()
        {
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
            Box(new Rect(180,380,920,270),new Color(.015f,.025f,.045f,.94f));
            GUI.Label(new Rect(220,398,840,45),paused?"일시정지 · Esc를 누르면 계속합니다.":endingPlaying?"대표의 마지막 한마디…":(bossUnit==null?"사무실 정리 완료!":"대표를 쓰러뜨렸습니다!"),menuHeading);
            GUI.Label(new Rect(220,458,840,55),(bossUnit==null?endingLine:bossUnit.displayName+": “"+endingLine+"”"),menuText);
            GUI.Label(new Rect(220,521,840,32),"최종 점수  "+score.ToString("N0"),menuText);
            if(!endingPlaying)
            {
                if(MenuButton(new Rect(355,572,250,55),"확인")) ConfirmEnding();
                if(MenuButton(new Rect(675,572,250,55),"재게임",true)) ReplayEnding();
            }
        }
        private void ApplyKoreanWorldFont(Transform root)
        {
            if(defenderFont==null) defenderFont=OfficeGameFont.Shared;
            foreach(var text in root.GetComponentsInChildren<TextMesh>(true))
            { OfficeGameFont.Apply(text); }
        }
    }
}
