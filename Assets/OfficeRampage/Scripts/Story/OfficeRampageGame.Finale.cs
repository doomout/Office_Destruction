using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool orbitalRequested,orbitalReady,orbitalUsed,finaleCamera;
        private Coroutine orbitalDropRoutine;
        private float orbitalDropElapsed;
        private Transform orbitalDropVisual;
        private Texture2D redButtonTexture;
        private int orbitalZoomStage;
        public int OrbitalZoomStage { get { return orbitalZoomStage; } }
        public bool OrbitalRequested { get { return orbitalRequested; } }
        private void ResetFinale()
        {
            orbitalRequested=orbitalReady=orbitalUsed=finaleCamera=false;orbitalZoomStage=0;robotDeliveryStage=0;bossEjected=false;
            orbitalDropRoutine=null;orbitalDropVisual=null;orbitalDropElapsed=0;
            RenderSettings.fog=true;
        }
        private Texture2D RedButtonIcon()
        {
            if(redButtonTexture!=null) return redButtonTexture;
            redButtonTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));
                Color color=d>30?Color.clear:d>25?new Color(.22f,.25f,.28f):new Color(.7f+.3f*y/64f,.025f,.04f);
                if(d<22 && y>42) color=new Color(1,.3f,.3f);
                redButtonTexture.SetPixel(x,y,color);
            }
            redButtonTexture.Apply();return redButtonTexture;
        }
        private void BuildRedButton(Transform root)
        {
            Shape(root,"Button case",new Vector3(0,0,.35f),new Vector3(.55f,.15f,.55f),Navy,false);
            Shape(root,"Red button",new Vector3(0,.14f,.35f),new Vector3(.4f,.12f,.4f),Color.red,false,PrimitiveType.Cylinder);
        }
        private Transform RobotPart(Transform parent,string name,Vector3 at,Vector3 size,Color color)
        { return Shape(parent,name,at,size,color,false).transform; }
        private IEnumerator DeployBossRobot()
        {
            finaleCamera=true;
            var robot=new GameObject("CEO giant robot").transform;robot.SetParent(bossUnit.transform,false);
            var red=new Color(.5f,.035f,.045f);var steel=new Color(.15f,.21f,.27f);
            RobotPart(robot,"Lower torso",new Vector3(0,3.05f,0),new Vector3(2.8f,.7f,1.45f),red);
            RobotPart(robot,"Upper torso",new Vector3(0,4.25f,0),new Vector3(2.8f,1.6f,1.45f),red);
            RobotPart(robot,"Waist seam",new Vector3(0,3.44f,0),new Vector3(2.82f,.06f,1.47f),steel);
            RobotPart(robot,"Reactor",new Vector3(0,4.1f,.78f),new Vector3(.7f,.7f,.12f),Cyan);
            RobotPart(robot,"Head",new Vector3(0,5.85f,0),new Vector3(1.3f,1.2f,1.15f),steel);
            RobotPart(robot,"Visor",new Vector3(0,5.9f,.59f),new Vector3(1.05f,.25f,.1f),Color.red);
            for(int side=-1;side<=1;side+=2)
            {
                var leg=new GameObject(side<0?"Robot left leg":"Robot right leg").transform;leg.SetParent(robot,false);leg.localPosition=new Vector3(side*.85f,2.5f,0);
                RobotPart(leg,"Piston leg",new Vector3(0,-1,0),new Vector3(1.1f,2.2f,1.1f),steel);
                RobotPart(leg,"Foot",new Vector3(0,-2.1f,.3f),new Vector3(1.3f,.65f,1.8f),red);
                var arm=new GameObject(side<0?"Robot left arm":"Robot right arm").transform;arm.SetParent(robot,false);arm.localPosition=new Vector3(side*1.9f,4.6f,0);
                RobotPart(arm,"Shoulder",Vector3.zero,new Vector3(1.25f,1.3f,1.5f),red);
                RobotPart(arm,"Forearm",new Vector3(0,-1.1f,.15f),new Vector3(.95f,1.6f,1.1f),steel);
                RobotPart(arm,"Fist cannon",new Vector3(0,-1.8f,.6f),new Vector3(1.1f,.9f,1.4f),red);
            }
            yield return DeliverRobotFromOrbit(robot);
            robot.localPosition=Vector3.up*35;
            Vector3 target=bossUnit.transform.position+Vector3.up*3;
            view.transform.position=target+new Vector3(-12,8,-16);view.transform.LookAt(target);view.fieldOfView=58;
            float t=0;
            while(t<2.5f)
            {
                if(RunActive) { t+=StoryCinematicDelta;robot.localPosition=Vector3.up*Mathf.Lerp(35,0,Mathf.SmoothStep(0,1,t/2.5f)); }
                yield return null;
            }
            robot.localPosition=Vector3.zero;audioBus.Play("explosion",.65f);shake=3;
            var pilot=bossUnit.visual;pilot.name="CEO pilot";pilot.SetParent(robot,false);
            Vector3 from=new Vector3(0,0,2),seat=new Vector3(0,3.2f,.85f);pilot.localPosition=from;
            t=0;while(t<1.5f)
            { if(RunActive) { t+=StoryCinematicDelta;pilot.localPosition=Vector3.Lerp(from,seat,Mathf.SmoothStep(0,1,t/1.5f));pilot.localScale=Vector3.one*Mathf.Lerp(1,.7f,t/1.5f); } yield return null; }
            RobotPart(robot,"Cockpit frame",new Vector3(0,3.1f,1.05f),new Vector3(1.5f,.25f,.35f),steel);
            bossUnit.visual=robot;robot.gameObject.AddComponent<OfficeBossRobot>().Initialize(this,bossUnit);
            ResetAmmo();Notify("탄약 보급 완료 · 권총 200 / 샷건 100 / 로켓 20",4);
            finaleCamera=false;
        }
        public void RequestOrbitalWeapon()
        {
            if(orbitalRequested || bossUnit==null || !bossUnit.PhaseTwo || ended) return;
            storySkipRequested=false;
            StoryLine(executiveAccessGranted?"결재 권한 인증 · 대표의 위성 장비를 사용할 수 있다.":"위성 장비 비상 권한 인증 · 마지막 기회다.",7);
            orbitalRequested=true;orbitalDropElapsed=0;combatLocked=true;ClearBossCombatForCinematic();
            ResetElements();whirlwindLeft=whirlwindReady=0;ClearDecoy();
            orbitalDropRoutine=StartCoroutine(DropOrbitalButton());
        }
        private void TickOrbitalReward(float dt)
        {
            if(bossUnit==null || !bossUnit.PhaseTwo || !bossUnit.Alive || orbitalUsed || orbitalReady)return;
            if(!orbitalRequested && !combatLocked && bossUnit.Health<=bossUnit.MaxHealth*.2f+.01f)RequestOrbitalWeapon();
            if(!orbitalRequested)return;
            orbitalDropElapsed+=dt;
            // Reward availability must not depend forever on a cosmetic coroutine completing.
            if(orbitalDropElapsed>=4)
            {
                if(orbitalDropRoutine!=null)StopCoroutine(orbitalDropRoutine);
                CompleteOrbitalReward();
            }
        }
        private void CompleteOrbitalReward()
        {
            if(orbitalReady || orbitalUsed)return;
            if(orbitalDropVisual!=null)Destroy(orbitalDropVisual.gameObject);
            orbitalDropVisual=null;orbitalDropRoutine=null;orbitalReady=true;SetWeapon(7);combatLocked=false;cooldown=.4f;
            Notify("??? 획득 · 8번 빨간 버튼 · 새로 왼쪽 클릭하면 발동",6);
        }
        private IEnumerator DropOrbitalButton()
        {
            var drop=new GameObject("Mystery weapon sky drop").transform;drop.SetParent(world,false);BuildRedButton(drop);drop.localScale=Vector3.one*2;
            orbitalDropVisual=drop;
            Vector3 landing=player.position+avatar.forward*1.7f;landing.y=.3f;landing.x=Mathf.Clamp(landing.x,-current.width/2+1,current.width/2-1);landing.z=Mathf.Clamp(landing.z,current.start+1,current.end-1);
            float t=0;
            while(t<.8f)
            { if(RunActive) { t+=StoryCinematicDelta;drop.position=landing+Vector3.up*Mathf.Lerp(35,0,t/.8f); } yield return null; }
            for(int i=0;i<3;i++) Trail(landing+Vector3.up*30+Random.insideUnitSphere,landing,Cyan);
            audioBus.Play("thunder",.75f);Impact(landing,Cyan,Vector3.up,18);
            yield return SceneWait(.7f);
            CompleteOrbitalReward();
        }
        private void ActivateOrbital()
        {
            if(!RunActive || combatLocked || !orbitalReady || orbitalUsed || bossUnit==null || !bossUnit.Alive) return;
            orbitalUsed=true;orbitalReady=false;combatLocked=true;ClearBossCombatForCinematic();ClearDecoy();ResetElements();whirlwindLeft=0;
            StartCoroutine(OrbitalStrike());
        }
        private IEnumerator CameraFlight(Vector3 destination,Vector3 target,float duration)
        {
            Vector3 from=view.transform.position;Quaternion rotation=view.transform.rotation;Quaternion to=Quaternion.LookRotation(target-destination);
            float t=0;while(t<duration)
            {
                if(RunActive) { t+=StoryCinematicDelta;float k=Mathf.SmoothStep(0,1,t/duration);view.transform.position=Vector3.Lerp(from,destination,k);view.transform.rotation=Quaternion.Slerp(rotation,to,k); }
                yield return null;
            }
        }
        private Transform BuildOrbitScene(Vector3 impact,out Transform[] satellites)
        {
            var space=new GameObject("Orbital cinematic").transform;space.SetParent(world,false);
            Vector3 earth=impact+Vector3.down*181;
            var globe=Shape(space,"Earth",earth,Vector3.one*360,Color.white,false,PrimitiveType.Sphere);
            globe.AddComponent<OfficeEarthVisual>().Initialize();
            satellites=new Transform[5];
            for(int i=0;i<5;i++)
            {
                float a=(i*72+18)*Mathf.Deg2Rad;var sat=new GameObject("Laser satellite "+(i+1)).transform;sat.SetParent(space,false);
                sat.position=impact+new Vector3(Mathf.Cos(a)*245,100+30*(i%2),Mathf.Sin(a)*245);sat.LookAt(impact);satellites[i]=sat;
                Shape(sat,"Satellite core",Vector3.zero,new Vector3(16,12,15),new Color(.8f,.66f,.2f),false);
                for(int side=-1;side<=1;side+=2)
                {
                    Shape(sat,"Solar panel",new Vector3(side*27,0,0),new Vector3(36,2,20),new Color(.06f,.24f,.5f),false);
                    for(int row=-1;row<=1;row++) Shape(sat,"Solar panel stripe",new Vector3(side*27,1.1f,row*6),new Vector3(35,.3f,.35f),Cyan,false);
                }
                Shape(sat,"Laser emitter",new Vector3(0,0,10),new Vector3(7,7,10),Color.red,false);
            }
            return space;
        }
        private IEnumerator OrbitalStrike()
        {
            storySkipRequested=false;
            StoryLine("승인권자 변경 완료. 이제 당신이 결재할 차례다.",6);
            finaleCamera=true;orbitalZoomStage=1;float oldFar=view.farClipPlane;bool fog=RenderSettings.fog;Color sky=view.backgroundColor;
            view.farClipPlane=2500;view.fieldOfView=58;
            Vector3 impact=bossUnit.transform.position;impact.y=0;
            yield return CameraFlight(impact+new Vector3(-35,55,-65),impact,1.2f);yield return SceneWait(.35f);
            orbitalZoomStage=2;
            yield return CameraFlight(impact+new Vector3(-100,180,-230),impact,1.3f);yield return SceneWait(.35f);
            RenderSettings.fog=false;view.backgroundColor=new Color(.002f,.004f,.015f);
            var arena=world.Find("Desert arena");if(arena!=null) arena.gameObject.SetActive(false);
            Transform[] satellites;var space=BuildOrbitScene(impact,out satellites);
            orbitalZoomStage=3;
            yield return CameraFlight(impact+new Vector3(-520,380,-600),impact+Vector3.down*80,1.6f);yield return SceneWait(.8f);
            var lasers=new GameObject("Five converging orbital lasers").transform;lasers.SetParent(space,false);
            foreach(var satellite in satellites)
            {
                Vector3 from=satellite.position+satellite.forward*14;Vector3 delta=impact-from;
                var beam=Shape(lasers,"Satellite laser",(from+impact)/2,new Vector3(3,3,delta.magnitude),new Color(1,.07f,.13f),false);
                beam.transform.rotation=Quaternion.LookRotation(delta);
                var core=Shape(lasers,"Laser white core",(from+impact)/2,new Vector3(1,1,delta.magnitude+.1f),Color.white,false);core.transform.rotation=beam.transform.rotation;
            }
            Shape(space,"Orbital impact glow",impact,Vector3.one*22,Color.white,false,PrimitiveType.Sphere);audioBus.Play("orbitalLaser",.8f);
            yield return SceneWait(2.3f);
            yield return SplitEarth(space);
            if(arena!=null) arena.gameObject.SetActive(true);space.gameObject.SetActive(false);Destroy(space.gameObject);
            RenderSettings.fog=fog;view.backgroundColor=sky;
            var floor=arena.Find("Sand floor");if(floor!=null) floor.gameObject.SetActive(false);
            var crater=new GameObject("Orbital impact crater");crater.transform.SetParent(arena,false);
            crater.AddComponent<OfficeCraterFloor>().Build(current.width+50,current.start,current.end,impact,12,Mat(new Color(.48f,.32f,.18f)));
            bossUnit.Teleport(new Vector3(impact.x,-11.9f,impact.z));
            controller.enabled=false;player.position=new Vector3(Mathf.Clamp(impact.x+(impact.x>0?-15:15),-current.width/2+2,current.width/2-2),.1f,Mathf.Clamp(impact.z,current.start+2,current.end-2));controller.enabled=true;vertical=0;
            audioBus.Play("explosion",1);
            yield return ShatterRobotAndEject(bossUnit);
            view.farClipPlane=oldFar;finaleCamera=false;combatLocked=false;
            bossUnit.Hit(bossUnit.MaxHealth*10);
        }
    }
}
