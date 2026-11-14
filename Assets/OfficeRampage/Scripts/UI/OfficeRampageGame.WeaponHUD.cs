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
        private readonly Texture2D[] weaponIcons=new Texture2D[8];
        private static readonly string[] iconNames={"bat","hammer","pistol","machinegun","decoy","shotgun","rocket","bfg"};
        private void DrawWeaponIcon(Rect area,int type,Color color)
        {
            if(type==7 && orbitalRequested)
            {
                GUI.DrawTexture(area,RedButtonIcon(),ScaleMode.ScaleToFit,true);return;
            }
            if(weaponIcons[type]==null) weaponIcons[type]=Resources.Load<Texture2D>("OfficeIcons/"+iconNames[type]);
            Color old=GUI.color; GUI.color=color;
            if(weaponIcons[type]!=null) GUI.DrawTexture(area,weaponIcons[type],ScaleMode.ScaleToFit,true);
            else GUI.Label(area,weaponNames[type],menuSmall);
            GUI.color=old;
        }
        private void DrawWeaponSlots()
        {
            EnsureMenuStyles();
            for(int i=0;i<8;i++)
            {
                float x=345+i*113; bool locked=i==7 && !(orbitalReady || (bfgReady && !bfgUsed));
                Rect slot=new Rect(x,580,106,84); bool empty=ammunition[i]==0;
                Box(slot,i==weaponIndex?new Color(.06f,.37f,.4f):Navy);
                if(i==weaponIndex) Box(new Rect(x,580,106,3),Cyan);
                DrawWeaponIcon(new Rect(x+17,585,72,52),i,locked||empty?new Color(.35f,.42f,.48f):Color.white);
                GUI.Label(new Rect(x+5,585,20,25),(i+1).ToString(),menuSmall);
                string amount=i==4?(DecoysPerOffice-decoysUsed[current.number]).ToString():i==7?(orbitalReady || (bfgReady&&!bfgUsed)?"1":"0"):ammunition[i]<0?"∞":ammunition[i].ToString();
                var countStyle=new GUIStyle(menuText){alignment=TextAnchor.MiddleRight,fontStyle=FontStyle.Bold};
                countStyle.normal.textColor=empty?Orange:Color.white;
                GUI.Label(new Rect(x+8,635,88,26),amount,countStyle);
                if(i==7 && orbitalReady) GUI.Label(new Rect(x+24,610,60,25),"???",menuSmall);
                if(slot.Contains(Event.current.mousePosition))
                { Box(new Rect(x-20,545,160,31),Navy);GUI.Label(new Rect(x-12,548,150,26),i==7 && orbitalRequested?"???":weaponNames[i],menuSmall); }
            }
        }
    }
}
