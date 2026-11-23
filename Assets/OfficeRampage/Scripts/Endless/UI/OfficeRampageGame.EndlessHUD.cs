using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private static string SurvivalClock(float seconds){int t=Mathf.FloorToInt(seconds);return (t/60).ToString("00")+":"+(t%60).ToString("00");}
        private void DrawEndlessHUD()
        {
            if(ended){DrawEndlessResult();return;}
            DrawEndlessTopStrip();
            if(endlessTarget!=null&&endlessTarget.Alive)HudTile(1194,30,HudSymbol.Skull,Mathf.CeilToInt(endlessTarget.Health).ToString(),endlessTarget.Health/endlessTarget.MaxHealth,Orange);
            Box(new Rect(22,570,305,127),Navy);GUI.Label(new Rect(38,577,280,26),"체력 "+Mathf.CeilToInt(hp)+" / 100",labelStyle);
            Box(new Rect(38,610,270,9),new Color(.2f,.25f,.3f));Box(new Rect(38,610,270*hp/100,9),Color.green);
            GUI.Label(new Rect(38,632,280,26),"갑옷 "+Mathf.CeilToInt(endlessArmor)+" / 100",labelStyle);
            Box(new Rect(38,665,270,9),new Color(.2f,.25f,.3f));Box(new Rect(38,665,270*endlessArmor/100,9),Cyan);
            Box(new Rect(1050,590,208,107),Navy);
            if(!endlessSaber&&endlessWeapon==11)HudIcon(new Rect(1008,600,32,32),HudSymbol.UFO,Color.green);
            var weaponLabel=new GUIStyle(labelStyle){fontSize=16,wordWrap=false,padding=new RectOffset(0,0,0,0),alignment=TextAnchor.MiddleLeft};
            var ammoLabel=new GUIStyle(weaponLabel){fontSize=18};
            GUI.Label(new Rect(1060,596,188,26),endlessSaber?"광선검":EndlessWeapons[endlessWeapon],weaponLabel);
            string amount=EndlessMechActive?endlessMechLeft.ToString("0.0")+"초 탑승":endlessSaber?"무제한":endlessWeapon>=7?endlessSpecialAmmo.ToString():endlessWeapon==4?(3-decoysUsed[1]).ToString():ammunition[weaponIndex]<0?"무제한":ammunition[weaponIndex].ToString();
            GUI.Label(new Rect(1060,630,188,30),amount,ammoLabel);
            GUI.Label(new Rect(1060,668,188,22),EndlessMechActive?"L / R":"E",smallStyle);
            GUI.Label(new Rect(623,338,35,35),"+",labelStyle);DrawWeaponSkills();DrawSprintHUD();
            if(weaponIndex==4)DrawDecoyHUD();
            DrawCompactPickup();
            DrawCompactNotice();DrawDoubleDamageBadge();
            if(paused||ended)
            {
                Box(new Rect(280,200,720,330),new Color(.015f,.025f,.045f,.97f));
                GUI.Label(new Rect(320,225,650,50),ended?"오늘의 스트레스 해소 완료":"일시정지",menuHeading);
                GUI.Label(new Rect(320,290,650,65),"생존 "+SurvivalClock(endlessTime)+"  ·  처치 "+endlessKills+"  ·  점수 "+score.ToString("N0"),menuText);
                if(MenuButton(new Rect(320,385,300,58),ended?"다시 도전":"계속하기",true))
                {if(ended)BeginEndless();else{paused=false;Time.timeScale=1;LockCursor(true);audioBus.SetPaused(false);}}
                if(MenuButton(new Rect(650,385,300,58),"첫 메뉴로")){SaveEndlessRecord();ReturnToMainMenu();}
            }
        }
        private GUIStyle endlessTopStyle;
        private void DrawEndlessTopStrip()
        {
            const float width=1150;
            if(endlessTopStyle==null)endlessTopStyle=new GUIStyle(smallStyle)
            {wordWrap=false,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(0,0,0,0),richText=false};
            string text="무한 파괴  ·  생존 "+SurvivalClock(endlessTime)
                +"  |  처치 "+endlessKills.ToString("N0")+"  |  점수 "+score.ToString("N0")
                +"  |  콤보 "+combo.ToString("N0")+"  |  적 "+defenders.Count+"/"+EndlessEnemyCap
                +"  |  최고 "+SurvivalClock(PlayerPrefs.GetFloat("OfficeRampage.Endless.BestTime",0));
            var content=new GUIContent(text);endlessTopStyle.fontSize=18;
            while(endlessTopStyle.fontSize>10&&endlessTopStyle.CalcSize(content).x>width-24)endlessTopStyle.fontSize--;
            Box(new Rect(22,12,width,34),new Color(.015f,.025f,.045f,.78f));
            GUI.Label(new Rect(34,12,width-24,34),content,endlessTopStyle);
        }
        private void DrawEndlessInfo()
        {
            GUI.Label(new Rect(80,215,1100,55),"무한 파괴 · 생존 도전",menuHeading);
            GUI.Label(new Rect(80,290,1100,240),"끝없이 몰려오는 직장 스트레스를 날려 버리세요.\nWASD 이동 · 마우스 조준 · 왼쪽 클릭 공격 · Shift 달리기\n바닥 장비 근처에서 E: 즉시 장착 / 갑옷·체력 보급\n숫자키로 무기 교체 불가 · 탄약 소진 시 기본 광선검으로 전환됩니다.\n가구는 다시 등장합니다. 전자기기 폭발은 적에게 피해를 줍니다.\n갑옷은 적 공격 피해의 70%를 내구도만큼 흡수합니다.",menuText);
            if(MenuButton(new Rect(80,590,360,65),"생존 시작",true))BeginEndless();
            if(MenuButton(new Rect(470,590,350,65),"Top 10 랭킹"))OpenEndlessRanks();
            if(MenuButton(new Rect(850,590,300,65),"뒤로"))menuPage=MenuPage.Main;
        }
    }
}
