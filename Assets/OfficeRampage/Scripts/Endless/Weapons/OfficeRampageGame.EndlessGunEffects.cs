using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private readonly Dictionary<Color,Material> endlessGlowMaterials=new Dictionary<Color,Material>();
        private float endlessGunKick;
        private Material EndlessGlow(Color color)
        {
            if(!endlessGlowMaterials.TryGetValue(color,out var mat))
            {mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));mat.color=color;endlessGlowMaterials[color]=mat;}
            return mat;
        }
        private GameObject GlowFx(string name,Vector3 at,Vector3 size,Color color,float life,Vector3 velocity,float growth=0,PrimitiveType type=PrimitiveType.Sphere)
        {var obj=Fx(name,at,size,color,life,velocity,growth,type);obj.GetComponent<Renderer>().sharedMaterial=EndlessGlow(color);return obj;}
        private LineRenderer EnergyLine(Transform root,string name,Vector3[] points,Color color,float width,bool local=false)
        {
            var line=new GameObject(name).AddComponent<LineRenderer>();line.transform.SetParent(root,false);line.useWorldSpace=!local;
            line.sharedMaterial=EndlessGlow(color);line.positionCount=points.Length;line.SetPositions(points);line.widthMultiplier=width;line.numCapVertices=3;line.numCornerVertices=3;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
        }
        private void EnergyStroke(string name,Vector3[] points,Color color,float width,float life)
        {
            while(combatEffects.Count>=160){var old=combatEffects.Dequeue();if(old!=null)Destroy(old);}
            var root=new GameObject(name);root.transform.SetParent(world,false);
            EnergyLine(root.transform,"Energy glow",points,color,width);EnergyLine(root.transform,"White core",points,Color.white,width*.28f);
            var fx=root.AddComponent<OfficeVisualEffect>();fx.game=this;fx.life=life;combatEffects.Enqueue(root);
        }
        private Vector3 EndlessGunMuzzle()=>weapon.TransformPoint(endlessWeapon==8?new Vector3(0,.15f,1.02f):new Vector3(0,.2f,.94f));
        private Vector3 EndlessAimDirection(Vector3 origin,float range)
        {
            Vector3 target=view.transform.position+view.transform.forward*range;float nearest=range;
            foreach(var hit in Physics.RaycastAll(view.transform.position,view.transform.forward,range,~0,QueryTriggerInteraction.Ignore))
            {
                if(IsPlayerCollider(hit.collider)||hit.distance>=nearest)continue;
                nearest=hit.distance;target=hit.point;
            }
            return (target-origin).normalized;
        }
        private void ElectricDischarge(Vector3 start,Vector3 end)
        {
            var points=new Vector3[13];Vector3 direction=(end-start).normalized;Vector3 side=Vector3.Cross(direction,Vector3.up).normalized;
            for(int i=0;i<points.Length;i++)points[i]=Vector3.Lerp(start,end,i/12f)+(i==0||i==12?Vector3.zero:side*Random.Range(-.38f,.38f)+Vector3.up*Random.Range(-.25f,.25f));
            EnergyStroke("Stapler lightning",points,Cyan,.22f,.32f);
            GlowFx("Stapler impact",end,Vector3.one*.55f,Cyan,.25f,Vector3.zero,.8f);
        }
        private void PrinterBeam(Vector3 start,Vector3 end)
        {
            EnergyStroke("Printer laser beam",new[]{start,end},Color.magenta,.38f,.34f);
            GlowFx("Printer muzzle flash",start,Vector3.one*.5f,Color.magenta,.23f,Vector3.zero,1);
            GlowFx("Printer laser impact",end,Vector3.one*.7f,Color.magenta,.32f,Vector3.zero,2);
            Vector3 direction=(end-start).normalized;
            for(int i=0;i<3;i++){var paper=GlowFx("Printer ejected sheet",start+Vector3.up*.1f,new Vector3(.22f,.02f,.3f),Color.white,.65f,-direction*1.2f+avatar.right*(1.5f+i*.4f)+Vector3.up*(.4f+i*.15f),0,PrimitiveType.Cube);paper.GetComponent<OfficeVisualEffect>().spin=new Vector3(100,160,80);}
        }
        private void TickEndlessGunPose(float dt)
        {
            if(!endlessMode)return;endlessGunKick=Mathf.Max(0,endlessGunKick-dt);
            if(endlessSaber||EndlessMechActive||(endlessWeapon!=8&&endlessWeapon!=9))return;
            float pulse=Mathf.Clamp01(endlessGunKick/.32f);
            weapon.localPosition=new Vector3(.48f,.95f,.25f-pulse*.2f);weapon.localRotation=Quaternion.Euler(-pulse*12,0,0);
            var jaw=weapon.Find("Stapler jaws");if(jaw!=null)jaw.localRotation=Quaternion.Euler(-Mathf.Sin(pulse*Mathf.PI)*28,0,0);
            var roller=weapon.Find("Printer feed roller");if(roller!=null)roller.localRotation=Quaternion.Euler(pulse*360,0,90);
        }
    }
}
