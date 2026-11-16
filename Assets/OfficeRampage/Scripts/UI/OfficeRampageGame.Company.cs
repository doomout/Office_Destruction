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
            bool oldEnabled=GUI.enabled; GUI.enabled=oldEnabled && (nameEditor==null || !nameEditor.Active);
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
                if(GUI.Button(new Rect(100,y+8,490,42),person.name+"  ✎",companyInput)) OpenNameEditor(selectedCompanyRank,i);
                if(MenuButton(new Rect(620,y+8,90,42),"남",!person.female)) person.female=false;
                if(MenuButton(new Rect(720,y+8,90,42),"여",person.female)) person.female=true;
            }
            GUI.EndScrollView();
            if(companyError!="") GUI.Label(new Rect(330,580,870,30),companyError,menuSmall);
            if(MenuButton(new Rect(650,615,230,46),"취소")) { companyDraft=null; menuPage=MenuPage.Settings; }
            if(MenuButton(new Rect(900,615,300,46),"저장",true) && CommitCompanySettings()) menuPage=MenuPage.Settings;
            GUI.enabled=oldEnabled; DrawNameEditor();
        }
        private int EnemyTotal(int room) { return spawnedEnemies[room]; }
        private void ApplyCompanyIdentity(OfficeDefender unit,int rank,int index)
        {
            unit.visual.GetComponent<OfficeCharacterVisual>()?.SetIdentity(runCompany.ranks[rank].people[index].female,index,rank);
            unit.companyRank=rank; unit.female=runCompany.ranks[rank].people[index].female;
            unit.displayName=runCompany.UnitName(rank,index);
            unit.label.richText=false;
            unit.label.color=unit.female?new Color(1,.6f,.85f):new Color(.5f,.85f,1);
            foreach(Transform part in unit.visual)
            {
                if(part.name=="Jacket") part.localScale=unit.female?new Vector3(.56f,.72f,.36f):new Vector3(.76f,.72f,.4f);
                if(part.name=="Tie" && unit.female) part.gameObject.SetActive(false);
            }
            if(unit.female)
            {
                var jacket=unit.visual.Find("Jacket").GetComponent<Renderer>().sharedMaterial.color;
                Shape(unit.visual,"Office skirt",new Vector3(0,.57f,0),new Vector3(.79f,.48f,.49f),jacket,false);
                Shape(unit.visual,"Scarf left",new Vector3(-.1f,1.34f,.22f),new Vector3(.16f,.09f,.08f),new Color(1,.65f,.7f),false).transform.localRotation=Quaternion.Euler(0,0,-25);
                Shape(unit.visual,"Scarf right",new Vector3(.1f,1.34f,.22f),new Vector3(.16f,.09f,.08f),new Color(1,.65f,.7f),false).transform.localRotation=Quaternion.Euler(0,0,25);
                Shape(unit.visual,"Ponytail",new Vector3(0,1.55f,-.4f),new Vector3(.3f,.75f,.3f),Navy,false,PrimitiveType.Capsule);
            }
            else
            {
                Shape(unit.visual,"Short hair crown",new Vector3(0,1.96f,-.02f),new Vector3(.47f,.14f,.43f),Navy,false);
                Shape(unit.visual,"Suit lapel left",new Vector3(-.17f,1.26f,.22f),new Vector3(.13f,.3f,.05f),new Color(.75f,.8f,.85f),false).transform.localRotation=Quaternion.Euler(0,0,-24);
                Shape(unit.visual,"Suit lapel right",new Vector3(.17f,1.26f,.22f),new Vector3(.13f,.3f,.05f),new Color(.75f,.8f,.85f),false).transform.localRotation=Quaternion.Euler(0,0,24);
            }
            if(unit.female)
            {
                Shape(unit.visual,"Long hair",new Vector3(0,1.63f,-.22f),new Vector3(.49f,.64f,.15f),Navy,false);
                Shape(unit.visual,"Hair side left",new Vector3(-.24f,1.59f,0),new Vector3(.08f,.5f,.35f),Navy,false);
                Shape(unit.visual,"Hair side right",new Vector3(.24f,1.59f,0),new Vector3(.08f,.5f,.35f),Navy,false);
            }
            if(rank==OfficeCompanySettings.RepresentativeRank && !unit.boss) Shape(unit.visual,"Baton",new Vector3(.48f,.95f,.43f),new Vector3(.11f,.11f,.9f),new Color(.32f,.16f,.07f),false);
            unit.visual.GetComponent<OfficeCharacterVisual>()?.RefreshLegacy();
        }
        private void CheckFinalRoster()
        {
            if(current==null || current.number!=4 || bossFight || ended || LivingDefenders(4)>0) return;
            if(securityWavePending) return;
            if(!securityWaveStarted) { securityWavePending=true; RemoveStageBarrier(dogRoomDoor); StartCoroutine(RevealSecurity()); return; }
            bossFight=true; RemoveStageBarrier(guardRoomDoor);
            if(bossUnit==null)
            {
                ClearDecoy(); endingLine="오늘은 대표가 없군요. 이제 퇴근합시다!"; End(true); return;
            }
            if(bossChair!=null) bossChair.SetActive(false);
            bossUnit.patrolStart=-1;
            if(activeFloor!=null)bossUnit.navigation=new OfficeNavigation(activeFloor.ReadLayout().CreateLayout());
            bossUnit.Wake(); audioBus.ChangeRoom(6);
            Notify("경비까지 모두 처치했습니다! 대표전 시작!",4);
        }
    }
}
