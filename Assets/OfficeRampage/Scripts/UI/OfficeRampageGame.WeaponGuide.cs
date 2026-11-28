using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool guideEndless;
        private int guidePage,guideSelection;
        private GUIStyle guideLabel,guideBody;
        private static readonly int[] StoryGuideIds={0,1,2,3,4,5,6,7,13};
        private static readonly int[] EndlessGuideIds={22,0,1,2,3,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21};
        private static readonly string[] GuideAttacks={
            "가까운 적과 가구를 가로로 휘두르며 공격합니다. 탄약 제한이 없습니다.",
            "느리지만 강력한 근접 타격입니다. 탄약 제한이 없습니다.",
            "조준한 방향으로 단발 사격합니다. 기본 탄약은 200발입니다.",
            "빠르게 연속 사격합니다. 별도의 콤보 스킬이나 탄약 제한은 없습니다.",
            "공격 대신 미끼를 던집니다. 남성 적이 10초 동안 더미 주변에 모이며 대표는 면역입니다.",
            "권총처럼 단일 표적을 쏘고 불을 붙입니다. 화상은 초당 1 피해를 줍니다. 기본 탄약은 100발입니다.",
            "불꽃을 남기며 직진하는 로켓이 명중 지점에서 폭발합니다. 넓은 범위를 공격하며 기본 탄약은 20발입니다.",
            "한 번 쓰는 대형 폭발 무기입니다.",
            "전방의 가까운 적부터 최대 5명에게 번개가 이어집니다. 짧은 기절 효과가 있습니다.",
            "조준한 직선으로 레이저를 발사합니다. 적과 가구를 관통하며 벽에는 막힙니다.",
            "전방에 뜨거운 커피를 뿜어 적과 가구에 불을 붙입니다.",
            "거대한 UFO가 두꺼운 광선으로 사람들을 한꺼번에 납치합니다. 가구는 남습니다.",
            "주변에 원형 충격파를 퍼뜨려 범위 피해를 주고 적을 기절시킵니다.",
            "빨간 버튼으로 위성 5기의 집중 공격을 요청합니다. 카메라가 우주까지 이동하는 궁극기입니다.",
            "붉은 지옥문에서 거대한 악마를 소환합니다. 악마가 10초 동안 주변 적과 싸워 줍니다.",
            "10초 동안 여러 자동차가 달려와 적을 들이받습니다.",
            "20초 동안 거대 로봇에 탑승합니다. 빨강·초록 광선검 두 자루를 동시에 휘두릅니다.",
            "드론 3대가 10초 동안 돌아다니며 수류탄을 떨어뜨립니다.",
            "전투기 3대가 지나가며 폭격합니다. 전장을 정리하고 배경을 바꿉니다.",
            "블랙홀로 적과 가구를 끌어들여 전장을 정리하고 배경을 바꿉니다.",
            "강한 지진과 지면 균열, 화산 폭발로 전장을 정리하고 배경을 바꿉니다.",
            "우주에서 지구를 보여 준 뒤 여러 운석이 떨어집니다. 전장을 정리하고 배경을 바꿉니다.",
            "무한 모드의 기본 근접 무기입니다. 야구빠따와 같은 피해를 주며 탄약이 필요 없습니다."
        };
        private string GuideName(int id)=>id==22?"광선검":id==13&&!guideEndless?"??? · 위성 버튼":EndlessWeapons[id];
        private string GuideSkill(int id)
        {
            const string ready="일반 공격으로 3콤보 단위 달성 → 10초 안에 F\n";
            if(id==0||id==22)return ready+"휠윈드: 10초 동안 회전하며 주변을 공격합니다.";
            if(id==1)return ready+"토르의 망치: 3초 동안 낙뢰를 소환합니다. 맞은 사람은 3초 동안 기절합니다.";
            if(id==2)return ready+"얼음 총알: 10초 동안 명중한 적을 3초간 얼립니다.";
            if(id==4)return "오른쪽 버튼으로 투척 위치 확인 → 왼쪽 클릭으로 투척\n사무실당 3개. 무한 모드에는 등장하지 않습니다.";
            if(id==16)return "왼쪽 클릭: 쌍검 공격\n오른쪽 클릭: 대시 / 재사용 3초\n탑승이 끝나면 기본 광선검으로 돌아옵니다.";
            if(id==7||id==11||id>=13&&id<=21)return "왼쪽 클릭으로 사용 · 1회용\n별도의 F 콤보 스킬은 없습니다.";
            return "왼쪽 클릭으로 공격\n별도의 F 콤보 스킬은 없습니다.";
        }
        private string GuideAcquire(int id)
        {
            if(guideEndless)
            {
                if(id==22)return "시작 시 기본 장착. 탄약을 소진하면 기본 광선검으로 돌아갑니다.";
                return "바닥에 나온 장비 가까이에서 E로 줍습니다. 무기 교체는 주운 장비로만 가능합니다."+(id>=8&&id<=12&&id!=11?"\n획득 시 40회 사용 가능합니다.":"");
            }
            if(id==7)return "인간 형태 대표의 체력이 20% 이하가 되면 비서가 지급합니다. 대표실을 폭파하고 로봇 페이즈로 넘어갑니다.";
            if(id==13)return "로봇 대표의 체력이 20% 이하가 되면 하늘에서 내려옵니다. 획득 후 8번 무기로 선택해 발사합니다.";
            return "숫자 "+(id+1)+"번 키로 선택합니다.";
        }
        private void DrawGuideIcon(Rect area,int id)
        {
            // BFG's catalogue icon must not depend on a previous run's orbital unlock flag.
            if(id==7){if(weaponIcons[7]==null)weaponIcons[7]=Resources.Load<Texture2D>("OfficeIcons/bfg");if(weaponIcons[7]!=null)GUI.DrawTexture(area,weaponIcons[7],ScaleMode.ScaleToFit,true);return;}
            DrawEquippedWeaponIcon(area,id,id==22,Color.white);
        }
        private void DrawWeaponGuide()
        {
            if(guideLabel==null){guideLabel=new GUIStyle(menuText){fontSize=18,alignment=TextAnchor.MiddleLeft,wordWrap=false};guideBody=new GUIStyle(menuText){fontSize=20};}
            if(MenuButton(new Rect(80,212,240,48),"스토리 무기",!guideEndless)){guideEndless=false;guidePage=guideSelection=0;}
            if(MenuButton(new Rect(332,212,240,48),"무한 모드 무기",guideEndless)){guideEndless=true;guidePage=guideSelection=0;}
            GUI.Label(new Rect(610,222,590,30),"무기 도감 · 아이콘을 골라 특징과 스킬 확인",menuSmall);
            int[] ids=guideEndless?EndlessGuideIds:StoryGuideIds;
            int pages=(ids.Length+5)/6;guidePage=Mathf.Clamp(guidePage,0,pages-1);guideSelection=Mathf.Clamp(guideSelection,0,ids.Length-1);
            for(int row=0;row<6;row++)
            {
                int index=guidePage*6+row;if(index>=ids.Length)break;
                var rect=new Rect(80,278+row*50,350,44);
                Box(rect,index==guideSelection?new Color(.05f,.30f,.36f):new Color(.06f,.11f,.16f));
                if(OfficeUIInput.Button(rect,"",GUIStyle.none))guideSelection=index;
                DrawGuideIcon(new Rect(90,rect.y+5,34,34),ids[index]);
                GUI.Label(new Rect(138,rect.y,280,44),GuideName(ids[index]),guideLabel);
            }
            int id=ids[guideSelection];Box(new Rect(458,278,742,330),Navy);
            DrawGuideIcon(new Rect(480,294,64,64),id);
            GUI.Label(new Rect(562,300,610,50),GuideName(id),menuHeading);
            GUI.Label(new Rect(480,365,695,83),GuideAttacks[id],guideBody);
            GUI.Label(new Rect(480,455,695,93),GuideSkill(id),guideBody);
            GUI.Label(new Rect(480,557,695,48),guideEndless?"무한 모드 전용 규칙 적용":"스토리 모드 규칙 적용 · 후반 무기 포함",menuSmall);
            if(MenuButton(new Rect(80,590,100,42),"이전")){guidePage=(guidePage+pages-1)%pages;guideSelection=guidePage*6;}
            GUI.Label(new Rect(195,598,100,32),(guidePage+1)+" / "+pages,menuText);
            if(MenuButton(new Rect(310,590,120,42),"다음")){guidePage=(guidePage+1)%pages;guideSelection=guidePage*6;}
            GUI.Label(new Rect(480,616,720,48),GuideAcquire(id),menuSmall);
            if(MenuButton(new Rect(80,640,350,30),"시작 메뉴로"))menuPage=MenuPage.Main;
        }
    }
}
