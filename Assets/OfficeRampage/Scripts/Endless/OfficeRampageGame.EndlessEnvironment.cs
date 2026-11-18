using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        // 0: original plaza, 1: desert, 2: tropical island. Nature themes alternate thereafter.
        private int endlessEnvironment;
        private float endlessEnvironmentNext=300,endlessWaveClock;
        private Transform endlessScenery;
        private readonly List<Transform> endlessShoreWaves=new List<Transform>();
        private void ResetEndlessEnvironment()
        {
            if(endlessScenery!=null){endlessScenery.gameObject.SetActive(false);Destroy(endlessScenery.gameObject);}
            endlessScenery=null;endlessShoreWaves.Clear();endlessEnvironment=0;endlessEnvironmentNext=300;endlessWaveClock=0;
        }
        private void TickEndlessEnvironment(float dt)
        {
            if(endlessTime>=endlessEnvironmentNext)
            {
                // Fixed survival-time milestones: weapon uses do not postpone the next five-minute change.
                endlessEnvironmentNext=(Mathf.Floor(endlessTime/300)+1)*300;
                ChangeEndlessEnvironment();
            }
            endlessWaveClock+=dt;
            for(int i=0;i<endlessShoreWaves.Count;i++)
            {
                var wave=endlessShoreWaves[i];if(wave==null)continue;
                Vector3 p=wave.localPosition;p.y=-.13f+Mathf.Sin(endlessWaveClock*1.1f+i*.7f)*.06f;wave.localPosition=p;
            }
        }
        private void ChangeEndlessEnvironment()
        {
            if(!endlessMode||endlessRoot==null)return;
            endlessEnvironment=endlessEnvironment==1?2:1;
            if(endlessScenery!=null){endlessScenery.gameObject.SetActive(false);Destroy(endlessScenery.gameObject);}
            endlessShoreWaves.Clear();endlessWaveClock=0;
            endlessScenery=new GameObject(endlessEnvironment==1?"Endless desert scenery":"Endless island scenery").transform;
            endlessScenery.SetParent(endlessRoot,false);
            // Keep original collision walls/floor and combat objects. Only their scenery renderers change.
            foreach(Transform child in endlessRoot)
            {
                if(child.name=="City silhouette"||child.name=="Arena boundary"||child.name=="Floor route light")
                {var renderer=child.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=false;}
            }
            var floor=endlessRoot.Find("Arena floor");
            if(floor!=null)floor.GetComponent<Renderer>().sharedMaterial=Mat(endlessEnvironment==1?DesertSand:IslandSand);
            if(endlessEnvironment==1)BuildEndlessDesert();else BuildEndlessIsland();
            foreach(var pickup in endlessPickups)if(pickup!=null)StyleEndlessEnvironmentPickup(pickup.transform);
            ApplyEndlessEnvironmentLighting();
        }
        private static readonly Color DesertSand=new Color(.65f,.48f,.29f),IslandSand=new Color(.66f,.63f,.49f);
        private void StyleEndlessEnvironmentPickup(Transform pickup)
        {
            if(endlessEnvironment==0)return;
            var label=pickup.GetComponentInChildren<TextMesh>();if(label!=null)label.color=new Color(.07f,.12f,.16f);
        }
        private GameObject Scenery(string name,Vector3 at,Vector3 size,Color color,PrimitiveType type=PrimitiveType.Cube)
        {return Shape(endlessScenery,name,at,size,color,false,type);}
        private void ApplyEndlessEnvironmentLighting()
        {
            if(!endlessMode||view==null||endlessEnvironment==0)return;
            bool desertTheme=endlessEnvironment==1;
            view.backgroundColor=desertTheme?new Color(.83f,.66f,.43f):new Color(.34f,.74f,.91f);
            view.farClipPlane=240;RenderSettings.fog=true;RenderSettings.fogColor=view.backgroundColor;
            RenderSettings.fogStartDistance=75;RenderSettings.fogEndDistance=195;
            RenderSettings.ambientLight=desertTheme?new Color(.5f,.48f,.43f):new Color(.45f,.52f,.55f);
            var light=world.Find("Office daylight");if(light!=null){var sun=light.GetComponent<Light>();sun.color=desertTheme?new Color(1,.95f,.86f):new Color(1,.98f,.94f);sun.intensity=1.1f;}
        }
        private void BoundaryStones(Color color)
        {
            // Visible low stones mark the same 64m arena; no new collider is introduced.
            for(int i=0;i<16;i++)for(int side=-1;side<=1;side+=2)
            {
                Scenery("Arena edge stone",new Vector3(side*31.6f,.17f,2+i*4),new Vector3(.65f,.4f,1.1f),color,PrimitiveType.Sphere);
                Scenery("Arena edge stone",new Vector3(-30+i*4,.17f,side<0?.4f:63.6f),new Vector3(1.1f,.4f,.65f),color,PrimitiveType.Sphere);
            }
        }
        private void BuildEndlessDesert()
        {
            Scenery("Desert horizon",new Vector3(0,-.65f,32),new Vector3(360,1,360),DesertSand);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {
                Scenery("Sand dune",new Vector3(side*(48+i%2*13),1.2f,-20+i*26),new Vector3(25,7+i%3*3,32),i%2==0?new Color(.86f,.61f,.32f):DesertSand,PrimitiveType.Sphere);
                var rock=Scenery("Sandstone mesa",new Vector3(side*(77+i%2*12),5+i%3*4,-30+i*31),new Vector3(14,12+i%3*7,19),new Color(.57f,.3f,.17f));rock.transform.localRotation=Quaternion.Euler(0,i*17,0);
            }
            for(int i=0;i<5;i++)
            {
                Scenery("Northern dune",new Vector3(-60+i*30,1,83),new Vector3(37,9+i%2*7,27),new Color(.86f,.61f,.32f),PrimitiveType.Sphere);
                Scenery("Southern dune",new Vector3(-60+i*30,1,-22),new Vector3(37,9+i%2*7,27),DesertSand,PrimitiveType.Sphere);
            }
            for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)BuildCactus(new Vector3(side*(36+i%2*3),0,9+i*16),1+i%2*.35f);
            BoundaryStones(new Color(.53f,.32f,.18f));
            for(int i=0;i<8;i++)Scenery("Sand ripple",new Vector3(-25+i*7,.015f,7+i%3*22),new Vector3(4,.02f,.18f),new Color(.71f,.46f,.22f));
        }
        private void BuildCactus(Vector3 at,float scale)
        {
            Color green=new Color(.22f,.37f,.15f);
            Scenery("Cactus trunk",at+Vector3.up*2*scale,new Vector3(.7f,4,.7f)*scale,green,PrimitiveType.Capsule);
            for(int side=-1;side<=1;side+=2)
            {
                Scenery("Cactus branch",at+new Vector3(side*.6f,1.9f+side*.3f,0)*scale,new Vector3(.45f,1.3f,.45f)*scale,green,PrimitiveType.Capsule).transform.localRotation=Quaternion.Euler(0,0,90);
                Scenery("Cactus arm",at+new Vector3(side*1.1f,2.4f+side*.3f,0)*scale,new Vector3(.45f,1.4f,.45f)*scale,green,PrimitiveType.Capsule);
            }
        }
        private void BuildEndlessIsland()
        {
            Scenery("Tropical ocean",new Vector3(0,-1.1f,32),new Vector3(380,.5f,380),new Color(.02f,.36f,.62f));
            Scenery("Shallow lagoon",new Vector3(0,-.63f,32),new Vector3(132,.3f,132),new Color(.05f,.69f,.72f),PrimitiveType.Cylinder);
            Scenery("Island beach",new Vector3(0,-.15f,32),new Vector3(101,.1f,101),IslandSand,PrimitiveType.Cylinder);
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI/8;Vector3 center=new Vector3(Mathf.Cos(a)*52,-.13f,32+Mathf.Sin(a)*52);
                var wave=Scenery("Shore foam",center,new Vector3(7,.035f,.55f),new Color(.74f,.96f,.91f));wave.transform.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);endlessShoreWaves.Add(wave.transform);
            }
            for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++)BuildPalm(new Vector3(side*(36+i%2*2),0,10+i*22),1+i%2*.2f);
            BuildPalm(new Vector3(-16,0,69),1.2f);BuildPalm(new Vector3(17,0,-7),1);
            for(int i=0;i<5;i++)
            {
                Scenery("Distant green island",new Vector3(-90+i*44,.4f,135),new Vector3(27,9+i%2*8,23),new Color(.16f,.4f,.24f),PrimitiveType.Sphere);
                Scenery("Tropical cloud",new Vector3(-105+i*50,30+i%2*10,105),new Vector3(24,5,9),new Color(.91f,.96f,.97f),PrimitiveType.Sphere);
            }
            BoundaryStones(new Color(.58f,.57f,.47f));
        }
        private void BuildPalm(Vector3 at,float scale)
        {
            Scenery("Palm trunk",at+Vector3.up*3*scale,new Vector3(.65f,6,.65f)*scale,new Color(.44f,.28f,.12f),PrimitiveType.Cylinder).transform.localRotation=Quaternion.Euler(0,0,8);
            Vector3 crown=at+new Vector3(-.42f,6,0)*scale;
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7;Vector3 direction=new Vector3(Mathf.Cos(a),-.2f,Mathf.Sin(a));
                var leaf=Scenery("Palm frond",crown+direction*1.8f*scale,new Vector3(.85f,.16f,4.6f)*scale,i%2==0?new Color(.13f,.47f,.2f):new Color(.24f,.6f,.23f),PrimitiveType.Sphere);
                leaf.transform.rotation=Quaternion.LookRotation(direction);
            }
            Scenery("Coconut",crown-Vector3.up*.25f,new Vector3(.6f,.6f,.6f)*scale,new Color(.3f,.18f,.08f),PrimitiveType.Sphere);
        }
    }
}
