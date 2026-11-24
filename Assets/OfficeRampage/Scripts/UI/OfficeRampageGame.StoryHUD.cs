using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private GUIStyle storyTopStyle,storyWeaponStyle;
        private string StoryProgressText()
        {
            if(current.number==2)return "서버 "+StoryServerCount+"/3 · 방어 "+StoryGuardsLeft+" · 구출 "+(storyRescued?"1/1":"0/1")+(current.unlocked?" · 출구 열림":"");
            if(current.number==4)return bossFight?"대표전"+(bossUnit!=null&&bossUnit.PhaseTwo?" · 로봇":""):(securityWaveStarted?"보디가드 ":"경비견 ")+LivingDefenders(4);
            return "남은 직원 "+LivingDefenders(current.number)+(current.unlocked?" · 출구 열림":"");
        }
        private void DrawStoryTopStrip()
        {
            if(storyTopStyle==null)storyTopStyle=new GUIStyle(smallStyle){wordWrap=false,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(0,0,0,0),richText=false};
            string text="스토리  ·  "+current.number+"층 / 4층  |  "+StoryProgressText()+"  |  점수 "+score.ToString("N0")+"  |  콤보 "+combo.ToString("N0");
            var content=new GUIContent(text);storyTopStyle.fontSize=18;
            while(storyTopStyle.fontSize>10&&storyTopStyle.CalcSize(content).x>1126)storyTopStyle.fontSize--;
            Box(new Rect(22,12,1150,34),new Color(.015f,.025f,.045f,.78f));
            GUI.Label(new Rect(34,12,1126,34),content,storyTopStyle);
            if(comboLeft>0)Box(new Rect(22,44,1150*Mathf.Clamp01(comboLeft/4),2),Orange);
            if(bossFight&&bossUnit!=null&&!ended)
            {
                HudTile(1194,30,HudSymbol.Skull,Mathf.CeilToInt(bossUnit.Health).ToString(),bossUnit.Health/bossUnit.MaxHealth,Orange);
                Box(new Rect(440,54,400,8),Navy);
                Box(new Rect(440,54,400*Mathf.Clamp01(bossUnit.Health/bossUnit.MaxHealth),8),Orange);
            }
        }
        private void DrawStoryWeaponCard()
        {
            if(storyWeaponStyle==null)storyWeaponStyle=new GUIStyle(labelStyle){fontSize=16,wordWrap=false,padding=new RectOffset(0,0,0,0),alignment=TextAnchor.MiddleLeft};
            Box(new Rect(1050,590,208,107),Navy);
            DrawWeaponIcon(new Rect(1004,598,38,38),weaponIndex,ammunition[weaponIndex]==0?Color.gray:Color.white);
            string name=weaponIndex==7&&orbitalRequested?"???":weaponNames[weaponIndex];
            var content=new GUIContent(name);storyWeaponStyle.fontSize=16;
            while(storyWeaponStyle.fontSize>11&&storyWeaponStyle.CalcSize(content).x>188)storyWeaponStyle.fontSize--;
            GUI.Label(new Rect(1060,596,188,26),content,storyWeaponStyle);
            string amount=weaponIndex==4?Mathf.Max(0,DecoysPerOffice-decoysUsed[current.number]).ToString():weaponIndex==7?(orbitalReady||(bfgReady&&!bfgUsed)?"1":"0"):ammunition[weaponIndex]<0?"무제한":ammunition[weaponIndex].ToString();
            GUI.Label(new Rect(1060,630,188,30),amount,labelStyle);
            GUI.Label(new Rect(1060,668,188,22),(weaponIndex+1)+"  /  1~8 무기 전환",smallStyle);
        }
    }
}
