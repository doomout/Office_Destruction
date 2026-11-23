using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private IEnumerator EndlessOrbitalStrike()
        {
            combatLocked=true;finaleCamera=true;ResetElements();whirlwindLeft=whirlwindReady=0;ClearDecoy();
            float oldFar=view.farClipPlane;bool fog=RenderSettings.fog;Color sky=view.backgroundColor;Transform space=null;
            Vector3 impact=new Vector3(0,0,32);
            try
            {
                view.farClipPlane=2500;view.fieldOfView=58;orbitalZoomStage=1;
                yield return CameraFlight(impact+new Vector3(-35,55,-65),impact,1.2f);yield return SceneWait(.35f);
                orbitalZoomStage=2;yield return CameraFlight(impact+new Vector3(-100,180,-230),impact,1.3f);yield return SceneWait(.35f);
                RenderSettings.fog=false;view.backgroundColor=new Color(.002f,.004f,.015f);
                endlessRoot.gameObject.SetActive(false);
                Transform[] satellites;space=BuildOrbitScene(impact,out satellites);orbitalZoomStage=3;
                yield return CameraFlight(impact+new Vector3(-520,380,-600),impact+Vector3.down*80,1.6f);yield return SceneWait(.8f);
                foreach(var satellite in satellites)
                {
                    Vector3 from=satellite.position+satellite.forward*14;Vector3 delta=impact-from;
                    var beam=Shape(space,"Satellite laser",(from+impact)/2,new Vector3(3,3,delta.magnitude),new Color(1,.07f,.13f),false);beam.transform.rotation=Quaternion.LookRotation(delta);
                    var core=Shape(space,"Laser white core",(from+impact)/2,new Vector3(1,1,delta.magnitude+.1f),Color.white,false);core.transform.rotation=beam.transform.rotation;
                }
                Shape(space,"Orbital impact glow",impact,Vector3.one*22,Color.white,false,PrimitiveType.Sphere);audioBus.Play("orbitalLaser",.8f);
                yield return SceneWait(2.3f);yield return SplitEarth(space);
                space.gameObject.SetActive(false);Destroy(space.gameObject);space=null;
                ClearEndlessBattle();endlessRoot.gameObject.SetActive(true);
                RenderSettings.fog=fog;view.backgroundColor=sky;view.farClipPlane=oldFar;finaleCamera=false;UpdateCamera();ChangeEndlessEnvironment();
                Notify("전장 정리 완료 · 3초 후 다시 시작",3);
                yield return SceneWait(3);
                RestoreEndlessBattle();Notify("적과 장비가 다시 등장했습니다!",3);
            }
            finally
            {
                if(space!=null)Destroy(space.gameObject);
                if(endlessRoot!=null)endlessRoot.gameObject.SetActive(true);
                if(view!=null){view.farClipPlane=oldFar;view.backgroundColor=sky;}
                RenderSettings.fog=fog;finaleCamera=false;combatLocked=false;orbitalZoomStage=0;ApplyEndlessEnvironmentLighting();
            }
        }
        private void ClearEndlessBattle()
        {
            int eliminated=0;
            foreach(var enemy in defenders)if(enemy!=null){if(enemy.Alive)eliminated++;enemy.gameObject.SetActive(false);Destroy(enemy.gameObject);}
            endlessKills+=eliminated;score+=eliminated*200;defenders.Clear();endlessTarget=null;
            foreach(var prop in endlessRoot.GetComponentsInChildren<OfficeProp>(true)){score+=100;prop.gameObject.SetActive(false);Destroy(prop.gameObject);}
            foreach(var supply in endlessPickups)if(supply!=null){supply.gameObject.SetActive(false);Destroy(supply.gameObject);}
            endlessPickups.Clear();nearbySupply=null;
            for(int i=0;i<endlessProps.Count;i++)endlessProps[i]=null;
            for(int i=world.childCount-1;i>=0;i--)
            {
                var child=world.GetChild(i);
                if(child==player||child==view.transform||child==endlessRoot||child.GetComponent<Light>()!=null)continue;
                child.gameObject.SetActive(false);Destroy(child.gameObject);
            }
            ResetAirSupport();ResetEndlessDemon();endlessCars.Clear();endlessDemon=null;endlessDemonLeft=endlessTrafficLeft=0;combatEffects.Clear();debris.Clear();
            current.total=current.destroyed=0;
        }
        private void RestoreEndlessBattle()
        {
            for(int i=0;i<endlessPropPositions.Count;i++)
            {
                if(Vector3.Distance(player.position,endlessPropPositions[i])<3)continue;
                SpawnProp(endlessRoot,current,endlessPropPositions[i],i%6);
                var spawned=endlessRoot.GetChild(endlessRoot.childCount-1);endlessProps[i]=spawned.GetComponent<OfficeProp>();ApplyKoreanWorldFont(spawned);
            }
            SeedEndlessEquipment();for(int i=0;i<24;i++)SpawnEndlessEnemy();
            endlessSpawnClock=1;endlessSupplyClock=EndlessWeaponInterval;endlessSupportClock=8;endlessPropClock=10;
        }
    }
}
