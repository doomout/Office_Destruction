using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private string endlessRunId,rankMessage="";
        private OfficeEndlessRecord pendingRank;
        private OfficeEndlessLeaderboard rankTable;
        private bool rankSubmitted;
        private GUIStyle rankInput,rankText,rankName;
        private void ResetEndlessRanking()
        {endlessRunId=System.Guid.NewGuid().ToString("N");pendingRank=null;rankSubmitted=false;rankMessage="";rankTable=null;}
        private void PrepareEndlessRank()
        {
            pendingRank=new OfficeEndlessRecord{runId=endlessRunId,seconds=Mathf.Max(0,Mathf.FloorToInt(endlessTime)),kills=Mathf.Max(0,endlessKills),score=Mathf.Max(0,score),utcTicks=System.DateTime.UtcNow.Ticks};
            rankSubmitted=false;rankMessage="";rankTable=OfficeEndlessLeaderboard.Load();
            if(nameEditor==null)nameEditor=gameObject.AddComponent<OfficeNameEditor>();
            nameEditor.Open(OfficeEndlessLeaderboard.CleanName(PlayerPrefs.GetString(OfficeEndlessLeaderboard.NameKey,"")));
        }
        private bool SubmitEndlessRank()
        {
            if(!endlessMode||!ended||pendingRank==null||rankSubmitted||nameEditor==null||!nameEditor.Active)return false;
            if(nameEditor.Composition.Length>0){rankMessage="한글 조합을 완료한 뒤 등록해 주세요.";return false;}
            string name=OfficeEndlessLeaderboard.CleanName(nameEditor.Text);
            if(name.Length==0){rankMessage="이름을 입력해 주세요.";return false;}
            pendingRank.playerName=name;
            try
            {
                var table=OfficeEndlessLeaderboard.Load();int place=table.Add(pendingRank);table.Save();
                PlayerPrefs.SetString(OfficeEndlessLeaderboard.NameKey,name);PlayerPrefs.Save();
                rankTable=table;rankSubmitted=true;nameEditor.Close();
                rankMessage=place>0?place+"위 등록 완료!":"이번 기록은 Top 10에 들지 못했습니다.";return true;
            }
            catch(System.Exception error){rankMessage="기록 저장 실패 · 다시 시도해 주세요.";Debug.LogWarning("Endless ranking save: "+error.Message);return false;}
        }
        private void EnsureRankingStyles()
        {
            if(rankText!=null)return;
            rankText=new GUIStyle(menuSmall){fontSize=17,richText=false,wordWrap=false,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(0,0,0,0)};
            rankName=new GUIStyle(rankText){fontSize=14};
            rankInput=new GUIStyle(GUI.skin.textField){font=menuFont,fontSize=22,richText=false,alignment=TextAnchor.MiddleLeft};
        }
        private void DrawRankingRows(float top)
        {
            EnsureRankingStyles();if(rankTable==null)rankTable=OfficeEndlessLeaderboard.Load();
            string[] headers={"순위","이름","생존 시간","처치","게임 점수","종합점수"};float[] x={65,150,520,680,810,1010};float[] widths={65,350,145,110,185,200};
            for(int i=0;i<headers.Length;i++)GUI.Label(new Rect(x[i],top,widths[i],26),headers[i],rankText);
            for(int row=0;row<10;row++)
            {
                float y=top+32+row*29;var entry=row<rankTable.records.Count?rankTable.records[row]:null;
                bool own=entry!=null&&pendingRank!=null&&entry.runId==pendingRank.runId;
                Box(new Rect(50,y,1180,27),own?new Color(.04f,.25f,.26f):row%2==0?new Color(.06f,.10f,.15f):Navy);
                GUI.Label(new Rect(x[0],y,widths[0],27),(row+1).ToString(),rankText);
                if(entry==null){GUI.Label(new Rect(x[1],y,widths[1],27),"—",rankName);continue;}
                GUI.Label(new Rect(x[1],y,widths[1],27),entry.playerName,rankName);
                GUI.Label(new Rect(x[2],y,widths[2],27),SurvivalClock(entry.seconds),rankText);
                GUI.Label(new Rect(x[3],y,widths[3],27),entry.kills.ToString("N0"),rankText);
                GUI.Label(new Rect(x[4],y,widths[4],27),entry.score.ToString("N0"),rankText);
                GUI.Label(new Rect(x[5],y,widths[5],27),entry.Total.ToString("N0"),rankText);
            }
        }
        private void DrawEndlessResult()
        {
            EnsureRankingStyles();Box(new Rect(20,20,1240,680),new Color(.015f,.025f,.045f,.98f));
            GUI.Label(new Rect(50,32,1180,45),"생존 종료 · 무한 파괴 Top 10",menuHeading);
            if(pendingRank==null)return;
            GUI.Label(new Rect(50,82,1180,30),"이번 기록: "+SurvivalClock(pendingRank.seconds)+" · "+pendingRank.kills+"킬 · 게임 점수 "+pendingRank.score.ToString("N0")+" · 종합점수 "+pendingRank.Total.ToString("N0"),rankText);
            if(!rankSubmitted)
            {
                nameEditor.DrawField(new Rect(50,127,355,42),rankInput);
                if(MenuButton(new Rect(425,127,180,42),"기록 등록",true))SubmitEndlessRank();
            }
            else GUI.Label(new Rect(50,127,550,42),pendingRank.playerName,rankInput);
            GUI.Label(new Rect(630,127,580,42),rankMessage,rankText);
            DrawRankingRows(190);
            GUI.Label(new Rect(50,550,1180,26),"종합점수 = 게임 점수 + 생존 1초당 10점 + 처치 1명당 100점 · 이 PC의 기록",rankText);
            GUI.Label(new Rect(50,580,1180,26),rankSubmitted?"같은 이름의 다른 플레이 기록도 등록할 수 있습니다.":"이름은 최대 24자입니다. 등록 없이 나가면 이번 Top 10 기록은 남지 않습니다.",rankText);
            if(MenuButton(new Rect(50,625,300,48),rankSubmitted?"다시 도전":"등록 없이 재도전",true))BeginEndless();
            if(MenuButton(new Rect(380,625,300,48),rankSubmitted?"첫 메뉴로":"등록 없이 메뉴로"))ReturnToMainMenu();
        }
        private void OpenEndlessRanks(){rankTable=OfficeEndlessLeaderboard.Load();menuPage=MenuPage.EndlessRanks;}
        private void DrawEndlessRanks()
        {
            EnsureRankingStyles();GUI.Label(new Rect(80,198,1120,40),"무한 파괴 · 로컬 Top 10",menuHeading);DrawRankingRows(248);
            GUI.Label(new Rect(80,578,1120,26),"종합점수 = 게임 점수 + 생존 초 × 10 + 처치 × 100 · 한 판씩 기록 · 이 PC에 저장",rankText);
            if(MenuButton(new Rect(80,616,300,48),"뒤로"))menuPage=MenuPage.Endless;
        }
    }
}
