using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private readonly int[] ammunition=new int[8];
        private void ResetAmmo()
        { for(int i=0;i<8;i++) ammunition[i]=-1; ammunition[2]=200; ammunition[5]=100; ammunition[6]=20; }
        private bool SpendAmmo(int index)
        {
            if(ammunition[index]<0) return true;
            if(ammunition[index]==0) { cooldown=.3f; Notify("탄약이 없습니다. 다른 무기를 선택하세요.",1); return false; }
            ammunition[index]--; return true;
        }
        private void IconBlock(Rect r,Color color) { Box(r,color); }
        private void IconStroke(Vector2 a,Vector2 b,float width,Color color)
        {
            Matrix4x4 before=GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
            IconBlock(new Rect(a.x,a.y-width/2,Vector2.Distance(a,b),width),color); GUI.matrix=before;
        }
        // Code-drawn silhouettes scale with the existing HUD; no font glyph dependency.
        private void DrawWeaponIcon(Rect area,int type,Color color)
        {
            Matrix4x4 before=GUI.matrix;
            GUI.matrix=before*Matrix4x4.TRS(new Vector3(area.x,area.y,0),Quaternion.identity,new Vector3(area.width/80,area.height/50,1));
            if(type==0) { IconStroke(new Vector2(17,42),new Vector2(58,8),10,color); IconStroke(new Vector2(10,47),new Vector2(23,36),5,Orange); }
            else if(type==1) { IconStroke(new Vector2(23,45),new Vector2(45,16),6,Orange); IconStroke(new Vector2(29,8),new Vector2(63,31),15,color); }
            else if(type==4)
            {
                IconBlock(new Rect(29,3,17,15),color);IconBlock(new Rect(23,20,30,14),color);
                IconStroke(new Vector2(28,33),new Vector2(23,48),7,color);IconStroke(new Vector2(47,33),new Vector2(52,48),7,color);
                IconStroke(new Vector2(22,23),new Vector2(12,33),5,color);IconStroke(new Vector2(54,23),new Vector2(65,12),5,new Color(1,.3f,.6f));
            }
            else if(type==6)
            {
                IconBlock(new Rect(12,18,48,15),color);IconStroke(new Vector2(60,18),new Vector2(70,25),10,Orange);
                IconStroke(new Vector2(60,33),new Vector2(70,25),10,Orange);IconBlock(new Rect(26,33,8,13),color);IconBlock(new Rect(6,14,6,24),color);
            }
            else if(type==7)
            {
                IconBlock(new Rect(8,14,53,23),color);IconBlock(new Rect(60,18,14,15),color);
                for(int i=0;i<3;i++) IconBlock(new Rect(16+i*15,18,9,14),Color.green);
                IconBlock(new Rect(19,37,10,11),color);
            }
            else
            {
                bool pistol=type==2;IconBlock(new Rect(pistol?22:9,13,pistol?43:58,12),color);
                IconStroke(new Vector2(31,24),new Vector2(23,44),11,color);
                if(!pistol) IconBlock(new Rect(3,19,12,14),color);
                if(type==3) { IconBlock(new Rect(43,25,9,19),Orange);IconBlock(new Rect(60,9,6,4),color); }
                if(type==5) { IconBlock(new Rect(35,27,32,5),Orange);IconBlock(new Rect(65,14,8,16),color); }
            }
            GUI.matrix=before;
        }
        private void DrawWeaponSlots()
        {
            EnsureMenuStyles();
            for(int i=0;i<8;i++)
            {
                float x=345+i*113; bool locked=i==7 && (!bfgReady || bfgUsed);
                Rect slot=new Rect(x,580,106,84); bool empty=ammunition[i]==0;
                Box(slot,i==weaponIndex?new Color(.06f,.37f,.4f):Navy);
                if(i==weaponIndex) Box(new Rect(x,580,106,3),Cyan);
                DrawWeaponIcon(new Rect(x+15,589,80,44),i,locked||empty?new Color(.35f,.42f,.48f):Color.white);
                GUI.Label(new Rect(x+5,585,20,25),(i+1).ToString(),menuSmall);
                string amount=i==4?(DecoysPerOffice-decoysUsed[current.number]).ToString():i==7?(bfgReady&&!bfgUsed?"1":"0"):ammunition[i]<0?"∞":ammunition[i].ToString();
                var countStyle=new GUIStyle(menuText){alignment=TextAnchor.MiddleRight,fontStyle=FontStyle.Bold};
                countStyle.normal.textColor=empty?Orange:Color.white;
                GUI.Label(new Rect(x+8,635,88,26),amount,countStyle);
                if(slot.Contains(Event.current.mousePosition))
                { Box(new Rect(x-20,545,160,31),Navy);GUI.Label(new Rect(x-12,548,150,26),weaponNames[i],menuSmall); }
            }
        }
    }
}
