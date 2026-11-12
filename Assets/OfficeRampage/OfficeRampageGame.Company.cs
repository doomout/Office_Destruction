using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private OfficeCompanySettings companySettings, companyDraft, runCompany;
        private readonly int[] spawnedEnemies = new int[6];
        private Vector2 companyScroll;
        private int selectedCompanyRank;
        private string companyError="";
        private GUIStyle companyInput,companyTab;
        private void OpenCompanySettings()
        {
            companyDraft=companySettings.Copy(); companyError=""; companyScroll=Vector2.zero;
            selectedCompanyRank=0; menuPage=MenuPage.Units;
        }
        private bool CommitCompanySettings()
        {
            for(int rank=0;rank<OfficeCompanySettings.Ranks.Length;rank++)
                for(int i=0;i<companyDraft.ranks[rank].people.Length;i++)
                    if(string.IsNullOrWhiteSpace(companyDraft.ranks[rank].people[i].name))
                    {
                        selectedCompanyRank=rank; companyScroll=Vector2.zero;
                        companyError=OfficeCompanySettings.Ranks[rank]+" "+(i+1)+"번의 이름을 입력하세요."; return false;
                    }
            companySettings=companyDraft.Copy(); companySettings.Save(); companyError=""; return true;
        }
        private void DrawCompanySettings()
        {
            if(companyDraft==null) OpenCompanySettings();
            if(companyInput==null)
            {
                companyInput=new GUIStyle(GUI.skin.textField) { font=menuFont,fontSize=21,alignment=TextAnchor.MiddleLeft,richText=false };
                companyTab=new GUIStyle(menuButton) { fontSize=18 };
            }
            GUI.Label(new Rect(80,202,1120,40),"유닛 설정 · 이름과 성별만 변경",menuHeading);
            GUI.Label(new Rect(80,248,1120,30),"총 43명 · 직급별 인원 고정 · 왼쪽 직급 선택 → 각 인물 편집 → 저장 후 새 게임에 적용",menuSmall);
            for(int rank=0;rank<OfficeCompanySettings.Ranks.Length;rank++)
            {
                Color old=GUI.backgroundColor; GUI.backgroundColor=rank==selectedCompanyRank?Cyan:new Color(.38f,.48f,.58f);
                if(GUI.Button(new Rect(80,286+rank*28,230,26),OfficeCompanySettings.Ranks[rank]+" · "+OfficeCompanySettings.Counts[rank]+"명",companyTab))
                { selectedCompanyRank=rank; companyScroll=Vector2.zero; GUI.FocusControl(null); }
                GUI.backgroundColor=old;
            }
            var people=companyDraft.ranks[selectedCompanyRank].people;
            GUI.Label(new Rect(340,286,800,30),OfficeCompanySettings.Ranks[selectedCompanyRank]+" "+people.Length+"명 · 이름 최대 24자",menuText);
            companyScroll=GUI.BeginScrollView(new Rect(330,328,870,250),companyScroll,new Rect(0,0,840,Mathf.Max(250,people.Length*62)));
            for(int i=0;i<people.Length;i++)
            {
                float y=i*62; var person=people[i];
                Box(new Rect(0,y,840,57),i%2==0?new Color(.05f,.09f,.13f):Navy);
                GUI.Label(new Rect(10,y+12,100,32),(i+1)+"번",menuText);
                person.name=GUI.TextField(new Rect(100,y+8,490,42),person.name,24,companyInput);
                if(MenuButton(new Rect(620,y+8,90,42),"남",!person.female)) person.female=false;
                if(MenuButton(new Rect(720,y+8,90,42),"여",person.female)) person.female=true;
            }
            GUI.EndScrollView();
            if(companyError!="") GUI.Label(new Rect(330,580,870,30),companyError,menuSmall);
            if(MenuButton(new Rect(650,615,230,46),"취소")) { companyDraft=null; menuPage=MenuPage.Settings; }
            if(MenuButton(new Rect(900,615,300,46),"저장",true) && CommitCompanySettings()) menuPage=MenuPage.Settings;
        }
        private int EnemyTotal(int room) { return spawnedEnemies[room]; }
        private void ApplyCompanyIdentity(OfficeDefender unit,int rank,int index)
        {
            unit.companyRank=rank; unit.female=runCompany.ranks[rank].people[index].female;
            unit.displayName=runCompany.UnitName(rank,index);
            unit.label.richText=false;
            if(unit.female)
            {
                Shape(unit.visual,"Long hair",new Vector3(0,1.63f,-.22f),new Vector3(.49f,.64f,.15f),Navy,false);
                Shape(unit.visual,"Hair side left",new Vector3(-.24f,1.59f,0),new Vector3(.08f,.5f,.35f),Navy,false);
                Shape(unit.visual,"Hair side right",new Vector3(.24f,1.59f,0),new Vector3(.08f,.5f,.35f),Navy,false);
            }
            if(rank==OfficeCompanySettings.RepresentativeRank && !unit.boss) Shape(unit.visual,"Baton",new Vector3(.48f,.95f,.43f),new Vector3(.11f,.11f,.9f),new Color(.32f,.16f,.07f),false);
        }
        private void CheckFinalRoster()
        {
            if(current==null || current.number!=5 || bossFight || ended || LivingDefenders(5)>0) return;
            bossFight=true;
            if(bossUnit==null)
            {
                ClearDecoy(); endingLine="오늘은 대표가 없군요. 이제 퇴근합시다!"; End(true); return;
            }
            if(bossChair!=null) bossChair.SetActive(false);
            bossUnit.Wake(); audioBus.ChangeRoom(6);
            Notify("경비견을 모두 처치했습니다! 대표전 시작!",4);
        }
    }
}
