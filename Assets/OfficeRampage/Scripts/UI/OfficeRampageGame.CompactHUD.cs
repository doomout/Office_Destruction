using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private enum HudSymbol { Whirlwind, Thunder, Ice, Heart, Shield, Run, Skull, Door, Ammo, Weapon, Double, Demon, Car, Robot, Oil, Alert, Clock, Drone, Jet, BlackHole }
        private GUIStyle hudNumber,hudKey;
        private HudSymbol hudNoticeSymbol;
        private string hudNoticeNumber="";
        private int hudNoticeWeapon;
        private void EnsureCompactStyles()
        {
            if(hudNumber!=null)return;EnsureMenuStyles();
            hudNumber=new GUIStyle(menuSmall){fontSize=17,alignment=TextAnchor.MiddleCenter,wordWrap=false,padding=new RectOffset(0,0,0,0)};
            hudKey=new GUIStyle(hudNumber){fontSize=12,fontStyle=FontStyle.Bold};
        }
        // Vector pictograms stay sharp at all HUD scales; no font-symbol fallback required.
        private void HudLine(Vector2 a,Vector2 b,Color color,float thickness=2)
        {
            var matrix=GUI.matrix;var old=GUI.color;GUI.color=color;
            GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);
            GUI.DrawTexture(new Rect(0,-thickness/2,Vector2.Distance(a,b),thickness),Texture2D.whiteTexture);
            GUI.matrix=matrix;GUI.color=old;
        }
        private void HudIcon(Rect area,HudSymbol icon,Color color)
        {
            Vector2 P(float x,float y)=>new Vector2(area.x+x*area.width,area.y+y*area.height);
            void L(float x,float y,float a,float b)=>HudLine(P(x,y),P(a,b),color,2);
            void Ring(float x,float y,float r,int segments=16){for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;L(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,x+Mathf.Cos(b)*r,y+Mathf.Sin(b)*r);}}
            switch(icon)
            {
                case HudSymbol.Drone:Ring(.2f,.2f,.15f);Ring(.8f,.2f,.15f);Ring(.2f,.8f,.15f);Ring(.8f,.8f,.15f);L(.2f,.2f,.8f,.8f);L(.2f,.8f,.8f,.2f);break;
                case HudSymbol.Jet:L(.5f,.05f,.6f,.45f);L(.6f,.45f,.95f,.7f);L(.95f,.7f,.6f,.65f);L(.6f,.65f,.65f,.95f);L(.65f,.95f,.5f,.85f);L(.5f,.85f,.35f,.95f);L(.35f,.95f,.4f,.65f);L(.4f,.65f,.05f,.7f);L(.05f,.7f,.4f,.45f);L(.4f,.45f,.5f,.05f);break;
                case HudSymbol.BlackHole:Ring(.5f,.5f,.2f);Ring(.5f,.5f,.4f);L(.05f,.7f,.95f,.3f);break;
                case HudSymbol.Whirlwind:
                    for(int i=1;i<=30;i++){float a=(i-1)*.36f,b=i*.36f,r=.05f+(i-1)*.012f,s=.05f+i*.012f;L(.5f+Mathf.Cos(a)*r,.5f+Mathf.Sin(a)*r,.5f+Mathf.Cos(b)*s,.5f+Mathf.Sin(b)*s);}break;
                case HudSymbol.Thunder:L(.6f,.05f,.22f,.55f);L(.22f,.55f,.55f,.48f);L(.55f,.48f,.38f,.95f);L(.38f,.95f,.8f,.38f);L(.8f,.38f,.5f,.43f);L(.5f,.43f,.6f,.05f);break;
                case HudSymbol.Ice:
                    for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),n=new Vector2(-d.y,d.x);HudLine(P(.5f,.5f),P(.5f+d.x*.43f,.5f+d.y*.43f),color);foreach(int s in new[]{-1,1}){Vector2 q=d*.21f+n*.13f*s;HudLine(P(.5f+d.x*.32f,.5f+d.y*.32f),P(.5f+q.x,.5f+q.y),color);}}break;
                case HudSymbol.Heart:L(.5f,.88f,.08f,.4f);L(.08f,.4f,.15f,.15f);L(.15f,.15f,.34f,.12f);L(.34f,.12f,.5f,.3f);L(.5f,.3f,.66f,.12f);L(.66f,.12f,.85f,.15f);L(.85f,.15f,.92f,.4f);L(.92f,.4f,.5f,.88f);break;
                case HudSymbol.Shield:L(.12f,.18f,.5f,.06f);L(.5f,.06f,.88f,.18f);L(.88f,.18f,.8f,.65f);L(.8f,.65f,.5f,.92f);L(.5f,.92f,.2f,.65f);L(.2f,.65f,.12f,.18f);break;
                case HudSymbol.Run:Ring(.62f,.12f,.08f);L(.55f,.3f,.4f,.58f);L(.4f,.58f,.7f,.68f);L(.7f,.68f,.84f,.9f);L(.4f,.58f,.25f,.83f);L(.25f,.83f,.05f,.83f);L(.52f,.35f,.72f,.46f);L(.72f,.46f,.92f,.35f);L(.52f,.35f,.3f,.3f);L(.3f,.3f,.14f,.5f);break;
                case HudSymbol.Door:L(.2f,.95f,.2f,.08f);L(.2f,.08f,.8f,.08f);L(.8f,.08f,.8f,.95f);L(.32f,.15f,.6f,.3f);L(.6f,.3f,.6f,.85f);L(.6f,.85f,.32f,.95f);L(.32f,.95f,.32f,.15f);break;
                case HudSymbol.Ammo:for(int i=0;i<3;i++){float x=.12f+i*.3f;L(x,.85f,x,.3f);L(x,.3f,x+.1f,.1f);L(x+.1f,.1f,x+.2f,.3f);L(x+.2f,.3f,x+.2f,.85f);L(x,.85f,x+.2f,.85f);}break;
                case HudSymbol.Double:L(.05f,.35f,.4f,.7f);L(.4f,.35f,.05f,.7f);L(.55f,.3f,.82f,.3f);L(.82f,.3f,.94f,.4f);L(.94f,.4f,.55f,.72f);L(.55f,.72f,.94f,.72f);break;
                case HudSymbol.Demon:L(.12f,.1f,.18f,.55f);L(.18f,.55f,.5f,.92f);L(.5f,.92f,.82f,.55f);L(.82f,.55f,.88f,.1f);L(.88f,.1f,.65f,.35f);L(.65f,.35f,.35f,.35f);L(.35f,.35f,.12f,.1f);L(.25f,.5f,.4f,.58f);L(.6f,.58f,.75f,.5f);break;
                case HudSymbol.Car:L(.08f,.7f,.08f,.45f);L(.08f,.45f,.25f,.38f);L(.25f,.38f,.35f,.18f);L(.35f,.18f,.7f,.18f);L(.7f,.18f,.85f,.45f);L(.85f,.45f,.95f,.5f);L(.95f,.5f,.95f,.7f);L(.08f,.7f,.95f,.7f);Ring(.25f,.75f,.12f);Ring(.77f,.75f,.12f);break;
                case HudSymbol.Robot:L(.2f,.08f,.8f,.08f);L(.8f,.08f,.8f,.7f);L(.8f,.7f,.65f,.9f);L(.65f,.9f,.35f,.9f);L(.35f,.9f,.2f,.7f);L(.2f,.7f,.2f,.08f);L(.3f,.4f,.43f,.4f);L(.57f,.4f,.7f,.4f);L(.35f,.68f,.65f,.68f);break;
                case HudSymbol.Oil:L(.5f,.05f,.13f,.6f);L(.13f,.6f,.22f,.85f);L(.22f,.85f,.5f,.95f);L(.5f,.95f,.78f,.85f);L(.78f,.85f,.87f,.6f);L(.87f,.6f,.5f,.05f);break;
                case HudSymbol.Clock:Ring(.5f,.5f,.42f);L(.5f,.5f,.5f,.2f);L(.5f,.5f,.72f,.6f);break;
                case HudSymbol.Skull:Ring(.5f,.4f,.32f);L(.28f,.65f,.28f,.88f);L(.28f,.88f,.72f,.88f);L(.72f,.88f,.72f,.65f);Ring(.37f,.4f,.06f);Ring(.63f,.4f,.06f);break;
                case HudSymbol.Weapon:DrawWeaponIcon(area,Mathf.Clamp(hudNoticeWeapon,0,7),color);break;
                default:L(.5f,.05f,.06f,.9f);L(.06f,.9f,.94f,.9f);L(.94f,.9f,.5f,.05f);L(.5f,.32f,.5f,.6f);L(.5f,.72f,.5f,.77f);break;
            }
        }
        private void HudTile(float x,float y,HudSymbol icon,string value,float fraction,Color color,string key="")
        {
            EnsureCompactStyles();Box(new Rect(x,y,58,60),new Color(.02f,.04f,.07f,.82f));
            HudIcon(new Rect(x+14,y+5,30,30),icon,color);
            GUI.Label(new Rect(x+2,y+35,54,21),value,hudNumber);
            Box(new Rect(x+3,y+57,52*Mathf.Clamp01(fraction),2),color);
            if(key.Length>0){Box(new Rect(x-3,y-5,22,19),color);var old=GUI.color;GUI.color=Color.black;GUI.Label(new Rect(x-3,y-5,22,19),key,hudKey);GUI.color=old;}
        }
        private void DrawCompactSkills()
        {
            if(ended||combatLocked)return;
            DrawFocusedStatus();
            float x=endlessMode?350:345,y=endlessMode?630:512;
            void Skill(HudSymbol icon,float ready,float active,float max,int weaponId)
            {
                if(ready<=0&&active<=0)return;
                bool running=active>0,selected=weaponIndex==weaponId;
                float time=running?active:ready;
                HudTile(x,y,icon,time.ToString("0.0"),time/(running?max:10),running?Orange:selected?Cyan:Color.gray,running?"":selected?"F":endlessMode?"":(weaponId+1).ToString());x+=64;
            }
            Skill(HudSymbol.Whirlwind,whirlwindReady,whirlwindLeft,10,0);
            Skill(HudSymbol.Thunder,thunderReady,thunderLeft,3,1);
            Skill(HudSymbol.Ice,iceReady,iceLeft,10,2);
            if(weaponIndex==4||lureLeft>0){HudTile(x,y,HudSymbol.Heart,lureLeft>0?lureLeft.ToString("0.0"):(DecoysPerOffice-decoysUsed[current.number]).ToString(),lureLeft>0?lureLeft/10:1,decoyAiming&&!decoyValid?Color.red:new Color(1,.4f,.65f),decoyAiming?"L":"");x+=64;}
            if(slowedByOil){HudTile(x,y,HudSymbol.Oil,"50%",1,Orange);x+=64;}
            if(endlessMode)
            {
                void Buff(HudSymbol icon,float left,float max,Color color){if(left<=0)return;HudTile(x,y,icon,left.ToString("0.0"),left/max,color);x+=64;}
                // Double damage has a dedicated, larger badge beside the weapon panel.
                Buff(HudSymbol.Demon,endlessDemonLeft,10,Orange);Buff(HudSymbol.Car,endlessTrafficLeft,10,Color.yellow);
                Buff(HudSymbol.Robot,endlessMechLeft,20,Cyan);
                if(EndlessMechActive){HudTile(x,y,HudSymbol.Run,endlessMechDashCooldown>0?endlessMechDashCooldown.ToString("0.0"):"0",1-endlessMechDashCooldown/3,Color.green,"우클릭");x+=64;}
                Buff(HudSymbol.Drone,droneLeft,10,Cyan);Buff(HudSymbol.Jet,jetLeft,5,Orange);Buff(HudSymbol.BlackHole,blackHoleLeft,5,Color.magenta);
            }
        }
        private void DrawFocusedStatus()
        {
            OfficeDefender target=null;float closest=16;
            Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            foreach(var unit in defenders)
            {
                if(unit==null||!unit.Alive||unit.room!=current.number)continue;
                Vector3 delta=unit.transform.position-player.position;delta.y=0;
                if(delta.magnitude<closest&&Vector3.Dot(forward,delta.normalized)>.85f){target=unit;closest=delta.magnitude;}
            }
            if(target==null)return;
            if(target.StunLeft>0)HudTile(1194,320,HudSymbol.Thunder,target.StunLeft.ToString("0.0"),target.StunLeft/3,Orange);
            if(target.FrozenLeft>0)HudTile(1194,386,HudSymbol.Ice,target.FrozenLeft.ToString("0.0"),target.FrozenLeft/3,Cyan);
        }
        private void SetCompactNotice(string message)
        {
            hudNoticeSymbol=HudSymbol.Alert;hudNoticeNumber="";hudNoticeWeapon=weaponIndex;
            // Keep legacy notification producers intact; only presentation is translated here.
            if(message.Contains("체력")){hudNoticeSymbol=HudSymbol.Heart;var value=System.Text.RegularExpressions.Regex.Match(message,@"[+-]\d+");hudNoticeNumber=value.Success?value.Value:Mathf.CeilToInt(hp).ToString();}
            else if(message.Contains("휠윈드")||message.Contains("토르")||message.Contains("얼음")||message.Contains("스킬 충전")||message.Contains("미인계 성공")){hudNoticeNumber="";return;}
            else if(message.Contains("분노")){hudNoticeSymbol=HudSymbol.Skull;hudNoticeNumber="+50%";}
            else if(message.Contains("???")||message.Contains("비에프지")){hudNoticeSymbol=HudSymbol.Weapon;hudNoticeWeapon=7;hudNoticeNumber="1";}
            else if(message.Contains("탄약")){hudNoticeSymbol=HudSymbol.Ammo;hudNoticeNumber=message.Contains("소진")||message.Contains("없습니다")?"0":"+";}
            else if(message.Contains("갑옷")){hudNoticeSymbol=HudSymbol.Shield;hudNoticeNumber="+60";}
            else if(message.Contains("2배")){hudNoticeSymbol=HudSymbol.Double;hudNoticeNumber="2";}
            else if(message.Contains("탑승")){hudNoticeSymbol=HudSymbol.Robot;hudNoticeNumber=EndlessMechActive?"20":"0";}
            else if(message.Contains("획득")){hudNoticeSymbol=HudSymbol.Weapon;hudNoticeNumber="+1";}
            else if(message.Contains("3초")){hudNoticeSymbol=HudSymbol.Clock;hudNoticeNumber="3";}
            else if(message.Contains("층")||message.Contains("열립")||message.Contains("개방")||message.Contains("처치했습니다")){hudNoticeSymbol=HudSymbol.Door;hudNoticeNumber=current!=null?current.number.ToString():"";}
            else if(message.Contains("등장")||message.Contains("시작")){hudNoticeSymbol=HudSymbol.Skull;hudNoticeNumber=endlessMode?"∞":"";}
        }
        private void DrawCompactNotice()
        {
            if(noticeLeft<=0||ended||hudNoticeNumber.Length==0)return;
            HudTile(1194,180,hudNoticeSymbol,hudNoticeNumber,1,Cyan);
        }
        private void DrawCompactSprint()
        {
            EnsureCompactStyles();float y=endlessMode?532:540;
            Box(new Rect(22,y,170,36),Navy);HudIcon(new Rect(30,y+5,25,25),HudSymbol.Run,sprintExhausted?Orange:Cyan);
            GUI.Label(new Rect(58,y+2,60,25),sprintEnergy.ToString("0.0"),hudNumber);GUI.Label(new Rect(117,y+2,65,25),"Shift",hudKey);
            Box(new Rect(30,y+31,154,3),Color.gray);Box(new Rect(30,y+31,154*sprintEnergy/10,3),sprinting?Orange:Cyan);
        }
        private void DrawCompactPickup()
        {
            if(nearbySupply==null||ended)return;
            var p=nearbySupply;HudSymbol icon=p.kind==1?HudSymbol.Shield:p.kind==2?HudSymbol.Heart:p.kind==3?HudSymbol.Double:HudSymbol.Weapon;
            int previous=hudNoticeWeapon;hudNoticeWeapon=p.kind==0?EndlessBases[p.weaponId]:0;
            HudTile(1194,490,icon,p.kind==1?"60":p.kind==2?"30":p.kind==3?"2":"1",1,Cyan,"E");hudNoticeWeapon=previous;
        }
    }
}
