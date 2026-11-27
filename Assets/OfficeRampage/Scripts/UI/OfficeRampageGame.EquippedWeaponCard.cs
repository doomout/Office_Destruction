using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private GUIStyle equippedCount,equippedKey;
        private void DrawEquippedWeaponCard(int id,bool saber,string amount,string key,bool empty)
        {
            if(equippedCount==null)
            {
                equippedCount=new GUIStyle(labelStyle){fontSize=28,alignment=TextAnchor.MiddleRight,wordWrap=false,padding=new RectOffset(0,0,0,0)};
                equippedKey=new GUIStyle(smallStyle){fontSize=13,alignment=TextAnchor.MiddleRight,wordWrap=false,padding=new RectOffset(0,0,0,0)};
            }
            Box(new Rect(1050,590,208,107),Navy);
            Box(new Rect(1050,590,208,3),empty?Orange:Cyan);
            DrawEquippedWeaponIcon(new Rect(1062,606,88,72),id,saber,empty?Color.gray:Color.white);
            GUI.Label(new Rect(1156,610,90,40),amount,equippedCount);
            GUI.Label(new Rect(1156,665,90,22),key,equippedKey);
        }
        private void DrawEquippedWeaponIcon(Rect area,int id,bool saber,Color color)
        {
            Vector2 P(float x,float y)=>new Vector2(area.x+area.width*x,area.y+area.height*y);
            void L(float x,float y,float a,float b,float thick=3)=>HudLine(P(x,y),P(a,b),color,thick);
            void R(float x,float y,float w,float h){L(x,y,x+w,y);L(x+w,y,x+w,y+h);L(x+w,y+h,x,y+h);L(x,y+h,x,y);}
            if(saber){L(.18f,.85f,.38f,.65f,7);L(.28f,.58f,.45f,.75f);HudLine(P(.4f,.63f),P(.87f,.12f),Cyan,9);HudLine(P(.4f,.63f),P(.87f,.12f),Color.white,3);return;}
            if(id<8){DrawWeaponIcon(area,id,color);return;}
            switch(id)
            {
                case 8:R(.12f,.7f,.76f,.13f);L(.12f,.7f,.2f,.2f);L(.2f,.2f,.85f,.45f,6);HudIcon(new Rect(area.x+area.width*.48f,area.y,area.width*.35f,area.height*.5f),HudSymbol.Thunder,Color.yellow);break;
                case 9:R(.1f,.32f,.8f,.45f);R(.25f,.05f,.5f,.28f);R(.25f,.58f,.5f,.35f);L(.35f,.7f,.65f,.7f);L(.35f,.8f,.65f,.8f);break;
                case 10:R(.28f,.28f,.5f,.6f);R(.78f,.38f,.17f,.3f);L(.28f,.38f,.04f,.25f);L(.04f,.25f,.28f,.65f);L(.4f,.18f,.35f,.05f);L(.6f,.18f,.55f,.05f);break;
                case 11:HudIcon(area,HudSymbol.UFO,color);break;
                case 12:HudIcon(area,HudSymbol.Clock,color);break;
                case 13:GUI.DrawTexture(area,RedButtonIcon(),ScaleMode.ScaleToFit,true);break;
                case 14:HudIcon(area,HudSymbol.Demon,color);break;
                case 15:HudIcon(area,HudSymbol.Car,color);break;
                case 16:HudIcon(area,HudSymbol.Robot,color);break;
                case 17:HudIcon(area,HudSymbol.Drone,color);break;
                case 18:HudIcon(area,HudSymbol.Jet,color);break;
                case 19:HudIcon(area,HudSymbol.BlackHole,color);break;
                case 20:HudIcon(area,HudSymbol.Volcano,color);break;
                case 21:HudIcon(area,HudSymbol.Meteor,color);break;
                default:HudIcon(area,HudSymbol.Weapon,color);break;
            }
        }
    }
}
