using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform airSupportRoot;
        private readonly List<Transform> supportDrones=new List<Transform>(),supportJets=new List<Transform>();
        private sealed class SupportBomb{public Transform root;public Vector3 from,to;public float age;public bool ultimate;}
        private readonly List<SupportBomb> supportBombs=new List<SupportBomb>();
        private float droneLeft,droneClock,droneAge,jetLeft,jetClock,jetAge;
        private bool EndlessUltimateActive=>jetLeft>0||blackHoleLeft>0||ufoLeft>0||earthquakeLeft>0;
        private void EnsureAirSupportRoot(){if(airSupportRoot==null){airSupportRoot=new GameObject("Endless air support").transform;airSupportRoot.SetParent(world,false);}}
        private void ResetAirSupport()
        {
            ResetUFO();ResetEarthquake();
            if(airSupportRoot!=null){airSupportRoot.gameObject.SetActive(false);Destroy(airSupportRoot.gameObject);}
            airSupportRoot=null;supportDrones.Clear();supportJets.Clear();supportBombs.Clear();droneLeft=droneClock=droneAge=jetLeft=jetClock=jetAge=0;ResetBlackHole();
        }
        private void SummonSupportDrones()
        {
            EnsureAirSupportRoot();foreach(var drone in supportDrones)if(drone!=null)Destroy(drone.gameObject);supportDrones.Clear();
            droneLeft=10;droneClock=.3f;droneAge=0;
            for(int i=0;i<3;i++){var root=new GameObject("Bomber drone").transform;root.SetParent(airSupportRoot,false);BuildSupportDrone(root);root.position=player.position+new Vector3((i-1)*4,7,3);supportDrones.Add(root);}
            audioBus.Play("thunder",.35f,1.5f);
        }
        private void StartJetStrike()
        {
            EnsureAirSupportRoot();jetLeft=5;jetAge=0;jetClock=.25f;
            for(int i=0;i<3;i++){var root=new GameObject("Airstrike jet").transform;root.SetParent(airSupportRoot,false);BuildSupportJet(root);root.position=new Vector3((i-1)*17,12,-28);supportJets.Add(root);}
            audioBus.Play("thunder",.7f,.65f);
        }
        private void DropSupportBomb(Vector3 from,Vector3 to,bool ultimate)
        {
            if(supportBombs.Count>=24)return;
            var root=Shape(airSupportRoot,ultimate?"Jet bomb":"Drone grenade",from,new Vector3(.35f,.65f,.35f),ultimate?Orange:new Color(.24f,.3f,.12f),false,PrimitiveType.Capsule).transform;
            Shape(root,"Bomb warning tip",new Vector3(0,-.6f,0),Vector3.one*.35f,Color.red,false,PrimitiveType.Sphere);
            supportBombs.Add(new SupportBomb{root=root,from=from,to=to,ultimate=ultimate});
        }
        private void TickAirSupport(float dt)
        {
            if(droneLeft>0)
            {
                droneLeft=Mathf.Max(0,droneLeft-dt);droneAge+=dt;droneClock-=dt;
                for(int i=0;i<supportDrones.Count;i++)
                {
                    var root=supportDrones[i];float a=droneAge*.7f+i*Mathf.PI*2/3;Vector3 at=player.position+avatar.forward*7+new Vector3(Mathf.Cos(a)*6,4.8f+i*.5f,Mathf.Sin(a)*6);
                    at.x=Mathf.Clamp(at.x,-28,28);at.z=Mathf.Clamp(at.z,4,60);root.position=at;root.rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,Mathf.Sin(a)*8);
                    foreach(Transform child in root)if(child.name=="Drone rotor")child.Rotate(0,dt*1700,0);
                }
                if(droneLeft>0&&droneClock<=0)
                {
                    droneClock=1.25f;var candidates=defenders.FindAll(e=>e!=null&&e.Alive);
                    foreach(var drone in supportDrones){Vector3 at=candidates.Count>0?candidates[Random.Range(0,candidates.Count)].transform.position:player.position+new Vector3(Random.Range(-14,14),0,Random.Range(-14,14));at+=new Vector3(Random.Range(-3,3),0,Random.Range(-3,3));at.x=Mathf.Clamp(at.x,-29,29);at.z=Mathf.Clamp(at.z,3,61);at.y=.12f;DropSupportBomb(drone.position,at,false);}
                }
                if(droneLeft<=0){foreach(var root in supportDrones)if(root!=null)Destroy(root.gameObject);supportDrones.Clear();}
            }
            if(jetLeft>0)
            {
                jetLeft=Mathf.Max(0,jetLeft-dt);jetAge+=dt;jetClock-=dt;
                foreach(var jet in supportJets)jet.position+=Vector3.forward*26*dt;
                if(jetLeft>0&&jetClock<=0){jetClock=.3f;foreach(var jet in supportJets)if(jet.position.z>1&&jet.position.z<63)DropSupportBomb(jet.position,new Vector3(jet.position.x,.12f,jet.position.z+8),true);}
                if(jetLeft<=0)
                {
                    foreach(var jet in supportJets)if(jet!=null)Destroy(jet.gameObject);supportJets.Clear();
                    WipeEndlessEnemiesAndProps();ChangeEndlessEnvironment();ResumeAfterSupportUltimate();
                }
            }
            for(int i=supportBombs.Count-1;i>=0;i--)
            {
                var bomb=supportBombs[i];if(bomb.root==null){supportBombs.RemoveAt(i);continue;}
                bomb.age+=dt;float t=Mathf.Clamp01(bomb.age/1.1f);bomb.root.position=Vector3.Lerp(bomb.from,bomb.to,t*t);bomb.root.Rotate(0,dt*180,dt*90);
                if(t<1)continue;
                CreateBlast(bomb.to,false);SkillDamage(()=>EndlessArea(bomb.to,bomb.ultimate?12:6,bomb.ultimate?100000:100,false));audioBus.Play("explosion",bomb.ultimate?.3f:.18f);
                Destroy(bomb.root.gameObject);supportBombs.RemoveAt(i);
            }
            TickBlackHole(dt);TickUFO(dt);TickEarthquake(dt);
        }
        private void WipeEndlessEnemiesAndProps()
        {
            SkillDamage(()=>{foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive)enemy.Hit(1000000);foreach(var prop in endlessRoot.GetComponentsInChildren<OfficeProp>())if(prop!=null&&prop.health>0)prop.Hit(1000000,Vector3.up);});
        }
        private void ResumeAfterSupportUltimate(){endlessSpawnClock=3;endlessSupplyClock=1;endlessSupportClock=3;endlessPropClock=3;}
        private void BuildSupportDrone(Transform root)
        {
            Shape(root,"Drone body",Vector3.zero,new Vector3(1.3f,.45f,1),Navy,false);Shape(root,"Drone lens",new Vector3(0,-.2f,.5f),Vector3.one*.35f,Cyan,false,PrimitiveType.Sphere);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){Shape(root,"Drone arm",new Vector3(x*.55f,0,z*.55f),new Vector3(1.6f,.12f,.12f),Color.gray,false).transform.localRotation=Quaternion.Euler(0,-x*z*45,0);var rotor=new GameObject("Drone rotor").transform;rotor.SetParent(root,false);rotor.localPosition=new Vector3(x*1.1f,.2f,z*1.1f);Shape(rotor,"Rotor blade",Vector3.zero,new Vector3(1.5f,.06f,.15f),Cyan,false);Shape(rotor,"Rotor blade",Vector3.zero,new Vector3(.15f,.06f,1.5f),Cyan,false);}
        }
        private void BuildSupportJet(Transform root)
        {
            Shape(root,"Jet fuselage",Vector3.zero,new Vector3(1.3f,7,1),new Color(.28f,.34f,.4f),false,PrimitiveType.Capsule).transform.localRotation=Quaternion.Euler(90,0,0);
            Shape(root,"Jet wings",new Vector3(0,0,-.3f),new Vector3(8,.2f,2.4f),new Color(.2f,.25f,.31f),false);
            Shape(root,"Jet cockpit",new Vector3(0,.6f,1.4f),new Vector3(.9f,.55f,1.8f),Cyan,false,PrimitiveType.Sphere);
            Shape(root,"Jet tail",new Vector3(0,.8f,-2.6f),new Vector3(.18f,1.8f,1.4f),Orange,false);
            var flame=Shape(root,"Jet exhaust",new Vector3(0,0,-4),new Vector3(.6f,.6f,2),Orange,false,PrimitiveType.Sphere);flame.GetComponent<Renderer>().sharedMaterial=EndlessGlow(Orange);
        }
        private void BuildAirSupportRemote()
        {
            Shape(weapon,"Support controller",new Vector3(0,.1f,.4f),new Vector3(.6f,.2f,.7f),Navy,false);
            Shape(weapon,"Support display",new Vector3(0,.22f,.4f),new Vector3(.4f,.025f,.4f),endlessWeapon==17?Cyan:endlessWeapon==18?Orange:Color.magenta,false);
            if(endlessWeapon==19)Shape(weapon,"Gravity core",new Vector3(0,.45f,.4f),Vector3.one*.35f,Color.black,false,PrimitiveType.Sphere);
            else for(int side=-1;side<=1;side+=2)Shape(weapon,"Controller antenna",new Vector3(side*.2f,.3f,.7f),new Vector3(.04f,.04f,.5f),Color.gray,false);
        }
    }
}
