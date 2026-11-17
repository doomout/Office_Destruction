using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private static string SurvivalClock(float seconds){int t=Mathf.FloorToInt(seconds);return (t/60).ToString("00")+":"+(t%60).ToString("00");}
        private void DrawEndlessHUD()
        {
            Box(new Rect(22,20,400,160),Navy);
            GUI.Label(new Rect(38,28,360,40),"무한 파괴 · "+SurvivalClock(endlessTime),titleStyle);
            GUI.Label(new Rect(38,78,360,28),"처치 "+endlessKills+"  ·  점수 "+score.ToString("N0"),labelStyle);
            GUI.Label(new Rect(38,112,360,26),"콤보 "+combo+" · 최고 생존 "+SurvivalClock(PlayerPrefs.GetFloat("OfficeRampage.Endless.BestTime",0)),smallStyle);
            GUI.Label(new Rect(38,143,360,26),"활성 적 "+defenders.Count+" / "+EndlessEnemyCap+" · 계속 증원",smallStyle);
            if(endlessTarget!=null&&endlessTarget.Alive)GUI.Label(new Rect(460,35,780,35),"전방: "+endlessTarget.displayName+" · 체력 "+Mathf.CeilToInt(endlessTarget.Health),labelStyle);
            Box(new Rect(22,570,305,127),Navy);GUI.Label(new Rect(38,577,280,26),"체력 "+Mathf.CeilToInt(hp)+" / 100",labelStyle);
            Box(new Rect(38,610,270,9),new Color(.2f,.25f,.3f));Box(new Rect(38,610,270*hp/100,9),Color.green);
            GUI.Label(new Rect(38,632,280,26),"갑옷 "+Mathf.CeilToInt(endlessArmor)+" / 100",labelStyle);
            Box(new Rect(38,665,270,9),new Color(.2f,.25f,.3f));Box(new Rect(38,665,270*endlessArmor/100,9),Cyan);
            Box(new Rect(900,570,358,127),Navy);
            var weaponLabel=new GUIStyle(labelStyle){fontSize=20,wordWrap=false,padding=new RectOffset(0,0,0,0),alignment=TextAnchor.MiddleLeft};
            var ammoLabel=new GUIStyle(weaponLabel){fontSize=18};
            GUI.Label(new Rect(918,580,322,30),endlessUnarmed?"맨주먹":EndlessWeapons[endlessWeapon],weaponLabel);
            string amount=endlessUnarmed?"무제한":endlessWeapon>=7?endlessSpecialAmmo.ToString():endlessWeapon==4?(3-decoysUsed[1]).ToString():ammunition[weaponIndex]<0?"무제한":ammunition[weaponIndex].ToString();
            GUI.Label(new Rect(918,620,322,30),"남은 사용량: "+amount,ammoLabel);
            if(endlessDoubleDamageLeft>0)GUI.Label(new Rect(460,78,600,30),"피해 2배 · "+endlessDoubleDamageLeft.ToString("0.0")+"초",labelStyle);
            if(endlessDemonLeft>0||endlessTrafficLeft>0)GUI.Label(new Rect(460,112,700,30),"악마 지원 "+endlessDemonLeft.ToString("0.0")+"초 · 교통사고 "+endlessTrafficLeft.ToString("0.0")+"초",smallStyle);
            GUI.Label(new Rect(918,660,330,25),"E: 바닥 장비 줍기 · 숫자키 교체 불가",smallStyle);
            GUI.Label(new Rect(623,338,35,35),"+",labelStyle);DrawWeaponSkills();DrawSprintHUD();
            if(weaponIndex==4)DrawDecoyHUD();
            if(nearbySupply!=null&&!ended){Box(new Rect(340,460,600,56),Navy);GUI.Label(new Rect(355,470,570,40),"[E] "+nearbySupply.DisplayName,menuText);}
            if(noticeLeft>0&&!ended)GUI.Label(new Rect(300,205,900,40),notice,menuText);
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
        private void DrawEndlessInfo()
        {
            GUI.Label(new Rect(80,215,1100,55),"무한 파괴 · 생존 도전",menuHeading);
            GUI.Label(new Rect(80,290,1100,240),"끝없이 몰려오는 직장 스트레스를 날려 버리세요.\nWASD 이동 · 마우스 조준 · 왼쪽 클릭 공격 · Shift 달리기\n바닥 장비 근처에서 E: 즉시 장착 / 갑옷·체력 보급\n숫자키로 무기 교체 불가 · 탄약 소진 시 맨주먹으로 전환됩니다.\n가구는 다시 등장합니다. 전자기기 폭발은 적에게 피해를 줍니다.\n갑옷은 적 공격 피해의 70%를 내구도만큼 흡수합니다.",menuText);
            if(MenuButton(new Rect(80,590,400,65),"생존 시작",true))BeginEndless();
            if(MenuButton(new Rect(510,590,300,65),"뒤로"))menuPage=MenuPage.Main;
        }
    }
}
