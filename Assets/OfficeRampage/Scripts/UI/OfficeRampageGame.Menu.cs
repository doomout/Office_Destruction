using UnityEngine;

namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private enum MenuPage { Main, Settings, Units, Story, Endless, EndlessRanks, Quit, Intro, Game }
        private MenuPage menuPage = MenuPage.Main;
        private int difficulty = 1;
        private Font menuFont;
        private GUIStyle menuTitle, menuHeading, menuText, menuSmall, menuButton;
        private GUIStyle volumeTrack, volumeThumb;
        private const string SettingsPrefix = "OfficeRampage.Menu.";

        private void InitializeMenu()
        {
            companySettings = OfficeCompanySettings.Load(); companySettings.Save();
            difficulty = OfficeMenuRules.Normalize(PlayerPrefs.GetInt(SettingsPrefix + "Difficulty", 1));
            audioBus = GetComponent<OfficeAudio>();
            if (audioBus == null) audioBus = gameObject.AddComponent<OfficeAudio>();
            audioBus.MusicVolume = Mathf.Clamp(PlayerPrefs.GetFloat(SettingsPrefix + "Music", .38f), 0, .6f);
            audioBus.EffectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SettingsPrefix + "Effects", .6f));
            audioBus.Muted = PlayerPrefs.GetInt(SettingsPrefix + "Muted", 0) == 1;
            ResetRun();
            ReturnToMainMenu();
        }

        private void SaveMenuSettings()
        {
            if (audioBus == null) return;
            PlayerPrefs.SetInt(SettingsPrefix + "Difficulty", difficulty);
            PlayerPrefs.SetFloat(SettingsPrefix + "Music", audioBus.MusicVolume);
            PlayerPrefs.SetFloat(SettingsPrefix + "Effects", audioBus.EffectsVolume);
            PlayerPrefs.SetInt(SettingsPrefix + "Muted", audioBus.Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ReturnToMainMenu()
        {
            CancelElevatorReward();
            if(meteorActive)ResetMeteor();
            if (nameEditor != null) nameEditor.Close();
            SaveMenuSettings();
            menuPage = MenuPage.Main;
            paused = true;
            Time.timeScale = 0;
            LockCursor(false);
            if (audioBus != null) audioBus.SetPaused(true);
        }

        private void EnsureMenuStyles()
        {
            if (menuTitle != null) return;
            menuFont = OfficeGameFont.Shared;
            menuText = new GUIStyle(GUI.skin.label) { font = menuFont, fontSize = 21, wordWrap = true };
            menuText.normal.textColor = new Color(.84f, .89f, .93f);
            menuTitle = new GUIStyle(menuText) { fontSize = 52, fontStyle = FontStyle.Bold };
            menuTitle.normal.textColor = Color.white;
            menuHeading = new GUIStyle(menuTitle) { fontSize = 30 };
            menuSmall = new GUIStyle(menuText) { fontSize = 17 };
            menuButton = new GUIStyle(GUI.skin.button) { font = menuFont, fontSize = 24, fontStyle = FontStyle.Bold };
        }

        private bool MenuButton(Rect rect, string text, bool primary = false)
        {
            Color before = GUI.backgroundColor;
            GUI.backgroundColor = primary ? Cyan : new Color(.38f, .48f, .58f);
            bool pressed = GUI.Button(rect, text, menuButton);
            GUI.backgroundColor = before;
            return pressed;
        }

        private void DrawReturnToMenu()
        {
            EnsureMenuStyles();
            if (MenuButton(new Rect(900, 610, 300, 48), "시작 메뉴로")) ReturnToMainMenu();
        }

        private void DrawFrontEnd()
        {
            EnsureMenuStyles();
            Matrix4x4 previous = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.matrix = Matrix4x4.identity;
            Box(new Rect(0, 0, Screen.width, Screen.height), new Color(.015f, .025f, .045f, .96f));
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            try
            {
                Box(new Rect(52, 56, 7, 90), Orange);
                GUI.Label(new Rect(80, 55, 650, 70), "사무실 파괴", menuTitle);
                GUI.Label(new Rect(83, 125, 800, 32), "오늘의 스트레스, 여기서 부수고 퇴근하세요.", menuText);
                Box(new Rect(80, 183, 1120, 2), new Color(.22f, .32f, .4f));
                if (menuPage == MenuPage.Main) DrawMainPage();
                else if (menuPage == MenuPage.Settings) DrawSettingsPage();
                else if (menuPage == MenuPage.Units) DrawCompanySettings();
                else if (menuPage == MenuPage.Intro) DrawIntro();
                else if(menuPage==MenuPage.Endless)DrawEndlessInfo();
                else if(menuPage==MenuPage.EndlessRanks)DrawEndlessRanks();
                else DrawInformationPage();
                GUI.Label(new Rect(80, 673, 1100, 30), (menuPage==MenuPage.Endless||menuPage==MenuPage.EndlessRanks)?"끝없는 적 · 바닥 장비 줍기 · 생존 기록 도전":"스토리: 로비 → 사무실 → 임원층 → 대표층  /  시간 제한 없음", menuSmall);
            }
            finally { GUI.matrix = previous; GUI.color = previousColor; }
        }

        private void DrawMainPage()
        {
            var credits=new GUIStyle(menuSmall){fontSize=13};
            GUI.Label(new Rect(80,615,1120,22),"지구 지도: Solar System Scope / solarsystemscope.com (CC BY 4.0)",credits);
            GUI.Label(new Rect(80,638,1120,26),"아이콘: Delapouite · Lorc · Skoll · DarkZaitzev / game-icons.net (CC BY 3.0) · 효과음: kurt · rubberduck · StarNinjas (CC0)",credits);
            if (MenuButton(new Rect(80, 215, 400, 65), "스토리 모드", true)) { SaveMenuSettings(); BeginIntro(); return; }
            if (MenuButton(new Rect(80, 295, 400, 65), "무한 파괴")) menuPage = MenuPage.Endless;
            if (MenuButton(new Rect(80, 375, 400, 65), "설정")) menuPage = MenuPage.Settings;
            if (MenuButton(new Rect(80, 455, 400, 65), "스토리")) menuPage = MenuPage.Story;
            if (MenuButton(new Rect(80, 535, 400, 65), "끝내기")) menuPage = MenuPage.Quit;
            Box(new Rect(535, 215, 665, 410), new Color(.055f, .1f, .15f));
            Box(new Rect(535, 215, 665, 5), Cyan);
            GUI.Label(new Rect(567, 240, 600, 45), "처음 오셨나요?", menuHeading);
            GUI.Label(new Rect(567, 302, 590, 105), "스토리: 퇴근을 막은 원인을 추적하고 대표에게 도전!\n무한 파괴: 장비를 주우며 끝없이 버티세요.", menuText);
            GUI.Label(new Rect(567, 422, 590, 150), "WASD 이동 · 마우스 조준 · 왼쪽 클릭 공격\n스토리: 숫자키 무기 선택 / 무한 파괴: E로 줍기\nEsc 일시정지 · M 음소거", menuText);
            GUI.Label(new Rect(567, 572, 590, 36), "음량과 직원 이름·성별은 ‘설정’에서 바꿀 수 있어요.", menuSmall);
        }

        // Keep Unity's slider interaction, but draw high-contrast graphics independent of its dark skin.
        private float DrawVolumeSlider(Rect rect, float value, float maximum)
        {
            if (volumeTrack == null)
            {
                volumeTrack = new GUIStyle(GUIStyle.none) { fixedHeight = 40, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) };
                volumeThumb = new GUIStyle(GUIStyle.none) { fixedWidth = 20, fixedHeight = 40, margin = new RectOffset(0, 0, 0, 0) };
            }
            float updated = GUI.HorizontalSlider(rect, value, 0, maximum, volumeTrack, volumeThumb);
            float left = rect.x + 10;
            float width = rect.width - 20;
            float centerY = rect.center.y;
            float filled = width * Mathf.Clamp01(updated / maximum);
            Box(new Rect(left, centerY - 5, width, 10), new Color(.48f, .58f, .68f));
            Box(new Rect(left, centerY - 5, filled, 10), Cyan);
            Box(new Rect(left + filled - 10, centerY - 15, 20, 30), Color.white);
            Box(new Rect(left + filled - 5, centerY - 10, 10, 20), Cyan);
            return updated;
        }

        private void DrawSettingsPage()
        {
            GUI.Label(new Rect(80, 210, 600, 45), "설정", menuHeading);
            GUI.Label(new Rect(80, 270, 520, 60), "사운드", menuHeading);
            GUI.changed = false;
            GUI.Label(new Rect(80, 340, 230, 35), "음악  " + Mathf.RoundToInt(audioBus.MusicVolume / .6f * 100) + "%", menuText);
            audioBus.MusicVolume = DrawVolumeSlider(new Rect(300, 337, 265, 40), audioBus.MusicVolume, .6f);
            GUI.Label(new Rect(80, 400, 230, 35), "효과음  " + Mathf.RoundToInt(audioBus.EffectsVolume * 100) + "%", menuText);
            audioBus.EffectsVolume = DrawVolumeSlider(new Rect(300, 397, 265, 40), audioBus.EffectsVolume, 1f);
            bool changed = GUI.changed;
            if (MenuButton(new Rect(80, 470, 485, 55), audioBus.Muted ? "전체 소리: 꺼짐 (눌러서 켜기)" : "전체 소리: 켜짐 (눌러서 끄기)"))
            { audioBus.Muted = !audioBus.Muted; changed = true; }
            GUI.Label(new Rect(650,270,550,60),"진행 방식",menuHeading);
            GUI.Label(new Rect(650,340,550,170),"시간 제한 없음\n1층: 단말기·방어 / 2층: 서버·방어 / 3층: 임원\n가구 파괴는 진행 조건에서 제외\n대표층: 경비견 → 보디가드 → 대표",menuText);
            if(changed) SaveMenuSettings();
            if (MenuButton(new Rect(80, 535, 485, 45), "유닛 설정")) OpenCompanySettings();
            if (MenuButton(new Rect(80, 590, 300, 55), "뒤로")) { SaveMenuSettings(); menuPage = MenuPage.Main; }
        }

        private void DrawInformationPage()
        {
            string heading, body;
            if (menuPage == MenuPage.Story)
            {
                heading = "스토리";
                body = "금요일 밤의 야근이 끝나자 토요일 출근 지시가 왔다.\n주말 근무 신청은 하지 말고, 보고서는 처음부터 다시 쓰란다.\n\n출근은 확인됐는데 퇴근 권한은 없다고?\n퇴근을 막은 시스템과 그 뒤의 승인권자를 찾아 올라가자.\n\n2층의 서버 3개를 파괴하고 지정 방어 인원을 처치하세요.\n갇힌 동료를 구할지는 당신의 선택입니다.";
            }
            else
            {
                heading = "오늘은 여기까지?";
                body = "게임을 종료하시겠습니까?\n사운드와 유닛 설정은 저장됩니다.";
            }
            GUI.Label(new Rect(80, 215, 1100, 55), heading, menuHeading);
            GUI.Label(new Rect(80, 290, 1100, 290), body, menuText);
            if (menuPage == MenuPage.Quit && MenuButton(new Rect(410, 590, 300, 55), "끝내기", true))
            {
                SaveMenuSettings();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            if (MenuButton(new Rect(80, 590, 300, 55), "시작 메뉴로")) menuPage = MenuPage.Main;
        }
    }
}
