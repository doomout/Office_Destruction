using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform doubleDamageAura,doubleDamagePulse;
        private readonly Transform[] doubleDamageMotes=new Transform[6];
        private float doubleDamageVisualClock,doubleDamagePickupFlash;
        private static readonly Color DoubleDamagePurple=new Color(.85f,.18f,1);
        private void ClearDoubleDamageVisual()
        {
            if(doubleDamageAura!=null){doubleDamageAura.gameObject.SetActive(false);Destroy(doubleDamageAura.gameObject);}
            doubleDamageAura=doubleDamagePulse=null;doubleDamagePickupFlash=doubleDamageVisualClock=0;
            for(int i=0;i<doubleDamageMotes.Length;i++)doubleDamageMotes[i]=null;
        }
        private void ShowDoubleDamagePickup()
        {
            doubleDamagePickupFlash=1.2f;TickDoubleDamageVisual(0);
            audioBus.Play("thunder",.3f,1.6f);
        }
        private void TickDoubleDamageVisual(float dt)
        {
            if(!endlessMode||endlessDoubleDamageLeft<=0||ended){ClearDoubleDamageVisual();return;}
            if(doubleDamageAura==null)
            {
                doubleDamageAura=new GameObject("Player double damage aura").transform;doubleDamageAura.SetParent(player,false);
                Vector3[] circle=new Vector3[49];for(int i=0;i<circle.Length;i++){float a=i*Mathf.PI/24;circle[i]=new Vector3(Mathf.Cos(a),.08f,Mathf.Sin(a));}
                EnergyLine(doubleDamageAura,"Double damage ground ring",circle,DoubleDamagePurple,.07f,true);
                var pulse=new GameObject("Double damage pickup pulse").transform;pulse.SetParent(doubleDamageAura,false);doubleDamagePulse=pulse;
                EnergyLine(pulse,"Expanding pickup ring",circle,Color.white,.045f,true);
                for(int i=0;i<doubleDamageMotes.Length;i++)
                {
                    var mote=Shape(doubleDamageAura,"Double damage orbit spark",Vector3.zero,Vector3.one*.14f,DoubleDamagePurple,false);
                    mote.GetComponent<Renderer>().sharedMaterial=EndlessGlow(i%2==0?DoubleDamagePurple:Color.white);doubleDamageMotes[i]=mote.transform;
                }
            }
            doubleDamageVisualClock+=dt;doubleDamagePickupFlash=Mathf.Max(0,doubleDamagePickupFlash-dt);
            float size=EndlessMechActive?3:1;doubleDamageAura.localScale=Vector3.one*size;
            doubleDamagePulse.gameObject.SetActive(doubleDamagePickupFlash>0);
            doubleDamagePulse.localScale=Vector3.one*(1+(1-doubleDamagePickupFlash/1.2f)*1.7f);
            for(int i=0;i<doubleDamageMotes.Length;i++)
            {
                float a=doubleDamageVisualClock*2.6f+i*Mathf.PI/3;
                doubleDamageMotes[i].localPosition=new Vector3(Mathf.Cos(a)*.78f,.6f+i*.22f+Mathf.Sin(a*2)*.12f,Mathf.Sin(a)*.78f);
                doubleDamageMotes[i].localRotation=Quaternion.Euler(a*60,45,a*30);
            }
        }
        private void DrawDoubleDamageBadge()
        {
            if(endlessDoubleDamageLeft<=0||ended||combatLocked)return;
            EnsureCompactStyles();float pulse=doubleDamagePickupFlash>0?.5f+.5f*Mathf.Sin(doubleDamageVisualClock*12):1;
            Color accent=endlessDoubleDamageLeft<=3?Color.Lerp(Orange,DoubleDamagePurple,.5f+.5f*Mathf.Sin(doubleDamageVisualClock*8)):Color.Lerp(DoubleDamagePurple,Color.white,pulse*.35f);
            Box(new Rect(1048,512,136,65),accent);Box(new Rect(1051,515,130,59),new Color(.07f,.02f,.12f,.96f));
            var big=new GUIStyle(hudNumber){fontSize=32,fontStyle=FontStyle.Bold};big.normal.textColor=accent;
            GUI.Label(new Rect(1055,515,68,42),"×2",big);
            GUI.Label(new Rect(1120,523,58,30),endlessDoubleDamageLeft.ToString("0.0"),hudNumber);
            Box(new Rect(1056,565,120*Mathf.Clamp01(endlessDoubleDamageLeft/20),5),accent);
        }
    }
}
