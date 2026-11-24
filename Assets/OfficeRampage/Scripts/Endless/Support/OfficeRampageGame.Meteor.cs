using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private const int MeteorWeaponId=21;
        private bool meteorActive,meteorOldFog;
        private float meteorAge,meteorOldFar,meteorOldFov;
        private Color meteorOldSky;
        private Vector3 meteorCameraStart;
        private Quaternion meteorCameraRotation;
        private Transform meteorSpace;
        private int meteorHits;
        private sealed class FallingMeteor
        {
            public Transform rock,burst;
            public LineRenderer trail;
            public Vector3 from,to;
            public float delay,size;
            public bool hit;
        }
        private readonly List<FallingMeteor> fallingMeteors=new List<FallingMeteor>();
        private void TickMeteorSupply(float dt)
        {
            if(!endlessMode||!RunActive||CombatLocked||paused||dt<=0)return;
            meteorSupplyClock=Mathf.Max(0,meteorSupplyClock-dt);
            if(meteorSupplyClock>0||EndlessUltimateActive)return;
            var previous=endlessPickups.Count>0?endlessPickups[endlessPickups.Count-1]:null;
            SpawnEndlessPickup(0,MeteorWeaponId,NearbyEndlessDrop());
            var latest=endlessPickups.Count>0?endlessPickups[endlessPickups.Count-1]:null;
            if(latest!=null&&latest!=previous&&latest.kind==0&&latest.weaponId==MeteorWeaponId)meteorSupplyClock=30;
        }
        private void BuildMeteorRemote()
        {
            Shape(weapon,"Meteor controller",new Vector3(0,.12f,.4f),new Vector3(.6f,.2f,.7f),Navy,false);
            var rock=Shape(weapon,"Meteor hologram",new Vector3(0,.4f,.5f),Vector3.one*.28f,Orange,false,PrimitiveType.Sphere);
            rock.GetComponent<Renderer>().sharedMaterial=EndlessGlow(Orange);
            EnergyLine(weapon,"Meteor trajectory",new[]{new Vector3(0,.4f,.5f),new Vector3(.2f,.6f,.7f),new Vector3(.3f,.75f,.9f)},Color.yellow,.04f,true);
        }
        private void StartMeteor()
        {
            if(!endlessMode||!RunActive||CombatLocked||EndlessUltimateActive)return;
            meteorOldFar=view.farClipPlane;meteorOldFov=view.fieldOfView;meteorOldSky=view.backgroundColor;meteorOldFog=RenderSettings.fog;
            meteorCameraStart=view.transform.position;meteorCameraRotation=view.transform.rotation;
            meteorAge=0;meteorHits=0;meteorActive=true;combatLocked=true;finaleCamera=true;
            ResetElements();whirlwindLeft=whirlwindReady=0;ClearDecoy();
            view.farClipPlane=2500;view.fieldOfView=58;
            audioBus.Play("rocket",.45f,.65f);
        }
        private void BuildMeteorSpace()
        {
            Vector3 impact=new Vector3(0,0,32),earth=impact+Vector3.down*181;
            Transform[] satellites;meteorSpace=BuildOrbitScene(impact,out satellites);
            meteorSpace.name="Meteor bombardment orbital scene";
            foreach(var satellite in satellites){satellite.gameObject.SetActive(false);Destroy(satellite.gameObject);}
            var normal=(new Vector3(-520,380,-600)+Vector3.up*181).normalized;
            var right=Vector3.Cross(Vector3.up,normal).normalized;var up=Vector3.Cross(normal,right).normalized;
            for(int i=0;i<24;i++)
            {
                float angle=i*2.39996f,radius=.85f*Mathf.Sqrt((i+.5f)/24);
                var outward=(normal+(right*Mathf.Cos(angle)+up*Mathf.Sin(angle))*radius).normalized;
                Vector3 to=earth+outward*181,from=to+normal*(300+i%4*35)+Vector3.up*180+right*(80*Mathf.Sin(angle));
                float size=8+i%4*2;
                var rock=Shape(meteorSpace,"Incoming meteor",from,Vector3.one*size,new Color(.2f,.085f,.035f),false,PrimitiveType.Sphere).transform;
                Shape(rock,"Molten meteor core",Vector3.zero,new Vector3(.72f,1.03f,.75f),Orange,false,PrimitiveType.Sphere).GetComponent<Renderer>().sharedMaterial=EndlessGlow(Orange);
                var trail=EnergyLine(meteorSpace,"Meteor fire trail",new[]{from,from},Orange,size*.65f);
                trail.startWidth=size*.7f;trail.endWidth=.2f;
                var burst=Shape(meteorSpace,"Planet impact fireball",to,Vector3.one,Orange,false,PrimitiveType.Sphere).transform;
                burst.GetComponent<Renderer>().sharedMaterial=EndlessGlow(new Color(1,.35f,.03f));
                rock.gameObject.SetActive(false);trail.gameObject.SetActive(false);burst.gameObject.SetActive(false);
                fallingMeteors.Add(new FallingMeteor{rock=rock,trail=trail,burst=burst,from=from,to=to,delay=i*.16f,size=size});
            }
            endlessRoot.gameObject.SetActive(false);RenderSettings.fog=false;view.backgroundColor=new Color(.002f,.004f,.015f);
        }
        private void TickMeteor(float dt)
        {
            if(!meteorActive||!endlessMode||!RunActive||paused||dt<=0)return;
            meteorAge+=dt;
            Vector3 impact=new Vector3(0,0,32),earth=impact+Vector3.down*181;
            Vector3 middle=impact+new Vector3(-35,55,-65),destination=impact+new Vector3(-520,380,-600);
            if(meteorAge<1.5f)
            {
                float t=Mathf.SmoothStep(0,1,meteorAge/1.5f);
                view.transform.position=Vector3.Lerp(meteorCameraStart,middle,t);
                view.transform.rotation=Quaternion.Slerp(meteorCameraRotation,Quaternion.LookRotation(impact-middle),t);
            }
            else
            {
                if(meteorSpace==null)BuildMeteorSpace();
                float t=Mathf.SmoothStep(0,1,(meteorAge-1.5f)/2);
                view.transform.position=Vector3.Lerp(middle,destination,t);
                view.transform.LookAt(Vector3.Lerp(impact,earth,t));
            }
            foreach(var meteor in fallingMeteors)
            {
                float age=meteorAge-3.5f-meteor.delay;if(age<0)continue;
                float t=Mathf.Clamp01(age/1.6f);
                if(t<1)
                {
                    meteor.rock.gameObject.SetActive(true);meteor.trail.gameObject.SetActive(true);
                    var at=Vector3.Lerp(meteor.from,meteor.to,t*t);var direction=(meteor.to-meteor.from).normalized;
                    meteor.rock.position=at;meteor.rock.Rotate(dt*100,dt*140,0);
                    meteor.trail.SetPosition(0,at);meteor.trail.SetPosition(1,at-direction*Mathf.Min(95,age*150));
                }
                else
                {
                    meteor.rock.gameObject.SetActive(false);meteor.trail.gameObject.SetActive(false);
                    if(!meteor.hit){meteor.hit=true;meteorHits++;if(meteorHits%4==0)audioBus.Play("explosion",.32f,.7f);}
                    float blast=age-1.6f;meteor.burst.gameObject.SetActive(blast<1);
                    meteor.burst.localScale=Vector3.one*(meteor.size*4*Mathf.Sin(Mathf.Clamp01(blast)*Mathf.PI));
                }
            }
            if(meteorAge>=10)
            {
                // Shared orbital cleanup credits every living enemy once without charging combos.
                // It also calls ResetAirSupport -> ResetMeteor to restore camera/arena state.
                ClearEndlessBattle();ChangeEndlessEnvironment();ResumeAfterSupportUltimate();
            }
        }
        private void ResetMeteor()
        {
            if(meteorSpace!=null){meteorSpace.gameObject.SetActive(false);Destroy(meteorSpace.gameObject);}
            meteorSpace=null;fallingMeteors.Clear();
            if(meteorActive)
            {
                if(endlessRoot!=null)endlessRoot.gameObject.SetActive(true);
                if(view!=null){view.farClipPlane=meteorOldFar;view.fieldOfView=meteorOldFov;view.backgroundColor=meteorOldSky;view.transform.position=meteorCameraStart;view.transform.rotation=meteorCameraRotation;}
                RenderSettings.fog=meteorOldFog;combatLocked=false;finaleCamera=false;
            }
            meteorActive=false;meteorAge=0;
        }
    }
}
