using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform ufoRoot;
        private float ufoLeft,ufoAge;
        private bool ufoCaptured;
        private GameObject ufoBeam;
        private Mesh ufoBeamMesh;
        private Material ufoBeamMaterial;
        private sealed class UFOCaptive
        {
            public OfficeDefender unit;
            public CharacterController body;
            public bool aiEnabled,bodyEnabled;
            public Vector3 from,scale;
            public Quaternion rotation;
            public float age;
        }
        private readonly List<UFOCaptive> ufoCaptives=new List<UFOCaptive>();
        private void StartUFO()
        {
            if(!endlessMode||!RunActive||CombatLocked||ufoLeft>0)return;
            ResetUFO();ufoLeft=12;ufoAge=0;ufoCaptured=false;
            ufoRoot=new GameObject("Giant UFO - 56 metre mothership").transform;ufoRoot.SetParent(world,false);
            ufoRoot.position=new Vector3(0,64,32);
            Shape(ufoRoot,"Mothership hull",Vector3.zero,new Vector3(56,4,56),new Color(.24f,.3f,.38f),false,PrimitiveType.Sphere);
            Shape(ufoRoot,"Upper dome",new Vector3(0,2.2f,0),new Vector3(24,7,24),new Color(.08f,.28f,.35f),false,PrimitiveType.Sphere);
            UFOGlow(ufoRoot,"Abduction aperture",new Vector3(0,-2,0),new Vector3(12,.4f,12),Color.green,PrimitiveType.Cylinder);
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI*2/24;
                UFOGlow(ufoRoot,"Hull navigation light",new Vector3(Mathf.Cos(a)*25,-.8f,Mathf.Sin(a)*25),new Vector3(1.6f,.5f,1.6f),i%2==0?Color.green:Cyan,PrimitiveType.Sphere);
            }
            var rim=new Vector3[65];for(int i=0;i<rim.Length;i++){float a=i*Mathf.PI*2/64;rim[i]=new Vector3(Mathf.Cos(a)*27,-.7f,Mathf.Sin(a)*27);}
            EnergyLine(ufoRoot,"Mothership rim",rim,Cyan,.22f,true);
            BuildUFOBeam();
            audioBus.Play("thunder",.65f,.55f);shake=1;
        }
        private void UFOGlow(Transform root,string name,Vector3 at,Vector3 size,Color color,PrimitiveType type)
        {Shape(root,name,at,size,color,false,type).GetComponent<Renderer>().sharedMaterial=EndlessGlow(color);}
        private void BuildUFORemote(Transform root)
        {
            Shape(root,"UFO controller",new Vector3(0,.13f,.5f),new Vector3(.6f,.16f,.8f),Navy,false);
            UFOGlow(root,"Miniature saucer",new Vector3(0,.38f,.5f),new Vector3(.7f,.12f,.7f),Cyan,PrimitiveType.Sphere);
            UFOGlow(root,"Saucer dome",new Vector3(0,.46f,.5f),new Vector3(.3f,.18f,.3f),Color.green,PrimitiveType.Sphere);
        }
        private void BuildUFOBeam()
        {
            // One continuous translucent volume, shared by every captive.
            const int segments=64;
            var vertices=new Vector3[segments*2];var triangles=new int[segments*6];
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                vertices[i*2]=radial*6+Vector3.down*2.3f;
                vertices[i*2+1]=radial*46+Vector3.down*25.9f;
                int next=((i+1)%segments)*2,k=i*6;
                triangles[k]=i*2;triangles[k+1]=next;triangles[k+2]=i*2+1;
                triangles[k+3]=i*2+1;triangles[k+4]=next;triangles[k+5]=next+1;
            }
            ufoBeamMesh=new Mesh{name="UFO single tractor volume"};
            ufoBeamMesh.vertices=vertices;ufoBeamMesh.triangles=triangles;ufoBeamMesh.RecalculateBounds();
            ufoBeamMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ufoBeamMaterial.SetColor("_BaseColor",new Color(.12f,1,.45f,.18f));
            ufoBeamMaterial.SetFloat("_Surface",1);ufoBeamMaterial.SetFloat("_SrcBlend",5);
            ufoBeamMaterial.SetFloat("_DstBlend",1);ufoBeamMaterial.SetFloat("_ZWrite",0);
            ufoBeamMaterial.SetFloat("_Cull",0);ufoBeamMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            ufoBeamMaterial.renderQueue=3000;
            ufoBeam=new GameObject("Single wide tractor beam");ufoBeam.transform.SetParent(ufoRoot,false);
            ufoBeam.AddComponent<MeshFilter>().sharedMesh=ufoBeamMesh;
            var renderer=ufoBeam.AddComponent<MeshRenderer>();renderer.sharedMaterial=ufoBeamMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            ufoBeam.SetActive(false);
        }
        private void CaptureUFOEnemies()
        {
            foreach(var chosen in defenders)
            {
                if(chosen==null||!chosen.Alive||chosen.boss||chosen.dog)continue;
                var captive=new UFOCaptive{unit=chosen,body=chosen.GetComponent<CharacterController>(),aiEnabled=chosen.enabled,from=chosen.transform.position,scale=chosen.transform.localScale,rotation=chosen.transform.rotation};
                if(captive.body!=null){captive.bodyEnabled=captive.body.enabled;captive.body.enabled=false;}
                chosen.enabled=false;if(chosen.warning!=null)chosen.warning.gameObject.SetActive(false);
                ufoCaptives.Add(captive);
            }
            audioBus.Play("orbitalLaser",.65f,.65f);shake=1;
        }
        private void TickUFO(float dt)
        {
            if(ufoLeft<=0||!endlessMode||!RunActive||CombatLocked||paused||dt<=0)return;
            ufoAge+=dt;ufoLeft=Mathf.Max(0,12-ufoAge);
            float height=ufoAge<2?Mathf.Lerp(64,26,Mathf.SmoothStep(0,1,ufoAge/2)):ufoAge>10?Mathf.Lerp(26,85,Mathf.SmoothStep(0,1,(ufoAge-10)/2)):26;
            ufoRoot.position=new Vector3(0,height,32);ufoRoot.Rotate(0,dt*8,0);
            if(ufoAge>=2&&!ufoCaptured)
            {
                ufoCaptured=true;CaptureUFOEnemies();
            }
            ufoBeam.SetActive(ufoAge>=2&&ufoAge<7);
            if(ufoBeam.activeSelf)
                ufoBeamMaterial.SetColor("_BaseColor",new Color(.12f,1,.45f,.18f*Mathf.Clamp01((7-ufoAge)/.7f)));
            for(int i=ufoCaptives.Count-1;i>=0;i--)
            {
                var c=ufoCaptives[i];
                if(c.unit==null||!c.unit.Alive){ufoCaptives.RemoveAt(i);continue;}
                c.age+=dt;float t=Mathf.Clamp01(c.age/4f);Vector3 mouth=ufoRoot.position-Vector3.up*2.3f;
                c.unit.transform.position=Vector3.Lerp(c.from,mouth,t*t)+new Vector3(Mathf.Sin(c.age*8),0,Mathf.Cos(c.age*8))*(Mathf.Sin(t*Mathf.PI)*1.2f);
                c.unit.transform.rotation=c.rotation*Quaternion.Euler(t*70,c.age*230,0);
                c.unit.transform.localScale=c.scale*Mathf.Lerp(1,.15f,t*t);
                if(t>=1){SkillDamage(()=>c.unit.Hit(1000000));RestoreUFOCaptive(c);ufoCaptives.RemoveAt(i);}
            }
            if(ufoLeft<=0){ResetUFO();ResumeAfterSupportUltimate();}
        }
        private void RestoreUFOCaptive(UFOCaptive c)
        {
            if(c.unit==null||!c.unit.Alive)return;
            c.unit.transform.position=c.from;c.unit.transform.rotation=c.rotation;c.unit.transform.localScale=c.scale;
            if(c.body!=null)c.body.enabled=c.bodyEnabled;c.unit.enabled=c.aiEnabled;
        }
        private void ResetUFO()
        {
            foreach(var c in ufoCaptives)RestoreUFOCaptive(c);ufoCaptives.Clear();
            if(ufoRoot!=null){ufoRoot.gameObject.SetActive(false);Destroy(ufoRoot.gameObject);}
            if(ufoBeamMesh!=null)Destroy(ufoBeamMesh);if(ufoBeamMaterial!=null)Destroy(ufoBeamMaterial);
            ufoRoot=null;ufoBeam=null;ufoBeamMesh=null;ufoBeamMaterial=null;ufoLeft=ufoAge=0;ufoCaptured=false;
        }
        private void UpdateUFOCamera()
        {
            // Wide establishing view keeps both the building-sized ship and ascending people visible.
            view.fieldOfView=72;view.transform.position=new Vector3(0,35,-40);
            view.transform.LookAt(new Vector3(0,16,32));
        }
    }
}
