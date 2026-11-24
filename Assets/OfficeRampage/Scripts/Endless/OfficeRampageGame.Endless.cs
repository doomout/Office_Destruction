using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool endlessMode,endlessEquipAllowed,endlessAttackDelegate;
        public bool IsEndless=>endlessMode;
        private float endlessTime,endlessSpawnClock,endlessSupplyClock,endlessPropClock,endlessArmor;
        private int endlessKills,endlessSpawned,endlessWeapon,endlessSpecialAmmo;
        private const int EndlessEnemyCap=80,EndlessPickupCap=18;
        private OfficeNavigation endlessNavigation;
        private Transform endlessRoot;
        private readonly List<OfficeEndlessPickup> endlessPickups=new List<OfficeEndlessPickup>();
        private readonly List<Vector3> endlessPropPositions=new List<Vector3>();
        private readonly List<OfficeProp> endlessProps=new List<OfficeProp>();
        private OfficeEndlessPickup nearbySupply;
        private OfficeDefender endlessTarget;
        private static readonly string[] EndlessWeapons={"야구빠따","망치","권총","기관총","미인계 더미","화염 샷건","로켓런처","BFG 18","번개 스테이플러","레이저 프린터","용암 커피포트","UFO 소환기","퇴근 종","위성 호출기","지옥문","교통사고","헐크버스터","드론 조종기","전투기 폭격 지원","블랙홀 생성기","지진 유발기"};
        private static readonly int[] EndlessBases={0,1,2,3,4,5,6,7,2,3,5,6,1,7,7,7,7,7,7,7,7};
        private void BeginEndless(){endlessMode=true;ResetRun();}
        private void ResetEndlessState()
        {
            ResetEndlessEnvironment();ResetEndlessRanking();ResetEndlessExtras();endlessTime=endlessKills=endlessSpawned=endlessWeapon=0;endlessArmor=0;endlessSpawnClock=5;endlessSupplyClock=EndlessWeaponInterval;endlessPropClock=10;
            endlessSpecialAmmo=0;endlessEquipAllowed=true;endlessAttackDelegate=false;endlessPickups.Clear();endlessProps.Clear();endlessPropPositions.Clear();nearbySupply=null;endlessTarget=null;
        }
        private Room BuildEndlessArena()
        {
            activeFloor=null;var room=new Room{number=1,start=0,end=64,width=64,layoutName="무한 파괴 광장"};rooms.Add(room);
            endlessRoot=new GameObject("Endless destruction arena").transform;endlessRoot.SetParent(world,false);
            Shape(endlessRoot,"Arena floor",new Vector3(0,-.25f,32),new Vector3(64,.5f,64),new Color(.12f,.19f,.23f));
            for(int side=-1;side<=1;side+=2)
            {
                Shape(endlessRoot,"Arena boundary",new Vector3(side*32,2,32),new Vector3(.5f,4,64),Navy);
                Shape(endlessRoot,"Arena boundary",new Vector3(0,2,side<0?0:64),new Vector3(64,4,.5f),Navy);
                for(int i=0;i<6;i++)Shape(endlessRoot,"City silhouette",new Vector3(side*42,5+i%3*3,i*12),new Vector3(6,15+i%3*6,7),Navy,false);
            }
            for(int i=0;i<8;i++)Shape(endlessRoot,"Floor route light",new Vector3(0,.01f,4+i*8),new Vector3(.12f,.015f,3),Cyan,false);
            var layout=new OfficeLayout{width=64,length=64};
            foreach(float z in new[]{10f,18,26,38,46,54})foreach(float x in new[]{-24f,-16,-8,8,16,24})
            {
                int kind=endlessPropPositions.Count%6;var at=new Vector3(x,0,z);endlessPropPositions.Add(at);
                SpawnProp(endlessRoot,room,at,kind);endlessProps.Add(endlessRoot.GetComponentsInChildren<OfficeProp>()[endlessPropPositions.Count-1]);layout.furniture.Add(new OfficeLayout.Point(x,z,kind));
            }
            endlessNavigation=new OfficeNavigation(layout);
            controller.enabled=false;player.position=new Vector3(0,.08f,32);controller.enabled=true;pitch=30;
            SeedEndlessEquipment();
            FloorSign(endlessRoot,"무한 파괴 · 오래 버티고 마음껏 부수세요",new Vector3(0,4,63),.24f);
            ApplyKoreanWorldFont(endlessRoot);endlessEquipAllowed=false;return room;
        }
        private void TickEndless(float dt)
        {
            if(!endlessMode || !RunActive || CombatLocked)return;
            TickEndlessExtras(dt);endlessTime+=dt;TickEndlessEnvironment(dt);defenders.RemoveAll(u=>u==null||!u.Alive);endlessPickups.RemoveAll(p=>p==null);
            endlessTarget=null;float targetDistance=16;
            foreach(var unit in defenders){Vector3 delta=unit.transform.position-player.position;delta.y=0;if(delta.magnitude<targetDistance&&Vector3.Dot(Quaternion.Euler(0,yaw,0)*Vector3.forward,delta.normalized)>.85f){endlessTarget=unit;targetDistance=delta.magnitude;}}
            if(EndlessUltimateActive){endlessSpawnClock=Mathf.Max(endlessSpawnClock,dt+1);endlessSupplyClock=Mathf.Max(endlessSupplyClock,dt+1);endlessSupportClock=Mathf.Max(endlessSupportClock,dt+1);endlessPropClock=Mathf.Max(endlessPropClock,dt+1);}
            endlessSpawnClock-=dt;
            if(endlessSpawnClock<=0){for(int i=0;i<2&&defenders.Count<EndlessEnemyCap;i++)SpawnEndlessEnemy();endlessSpawnClock=Mathf.Max(.4f,1.5f-endlessTime/240);}
            TickEndlessSupply(dt);
            endlessPropClock-=dt;
            if(endlessPropClock<=0)
            {
                for(int i=0;i<endlessProps.Count;i++)if(endlessProps[i]==null && Vector3.Distance(player.position,endlessPropPositions[i])>6 && !Physics.CheckSphere(endlessPropPositions[i]+Vector3.up,.9f))
                {SpawnProp(endlessRoot,current,endlessPropPositions[i],i%6);var spawned=endlessRoot.GetChild(endlessRoot.childCount-1);endlessProps[i]=spawned.GetComponent<OfficeProp>();ApplyKoreanWorldFont(spawned);}
                endlessPropClock=10;
            }
            nearbySupply=null;float nearest=2.5f;
            foreach(var supply in endlessPickups)if(supply!=null&&(!EndlessMechActive||supply.kind!=0)){float d=Vector3.Distance(player.position,supply.transform.position);if(d<nearest){nearest=d;nearbySupply=supply;}}
            if(Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame&&nearbySupply!=null)CollectEndless(nearbySupply);
        }
        private Vector3 SafeEndlessDrop()
        {
            for(int i=0;i<24;i++){Vector3 p=new Vector3(Random.Range(-28,28),.08f,Random.Range(4,60));if(!Physics.CheckSphere(p+Vector3.up,.8f))return p;}
            return new Vector3(0,.08f,32);
        }
        private void SpawnEndlessEnemy()
        {
            Vector3 at=Vector3.zero;
            for(int i=0;i<24;i++){float a=Random.value*Mathf.PI*2;at=player.position+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*Random.Range(16,24);at.x=Mathf.Clamp(at.x,-29,29);at.z=Mathf.Clamp(at.z,3,61);at.y=.08f;if(Vector3.Distance(at,player.position)>12&&!Physics.CheckSphere(at+Vector3.up,.7f))break;}
            if(Vector3.Distance(at,player.position)<12||Physics.CheckSphere(at+Vector3.up,.7f))return;
            int rank=Mathf.Min(9,(int)(endlessTime/40));bool dog=endlessSpawned%9==8;
            var unit=CreateDefender(endlessRoot,current,endlessNavigation,at,dog?"야근 경비견":runCompany.UnitName(rank,endlessSpawned%OfficeCompanySettings.Counts[rank]),Mathf.Min(180,30+(int)(endlessTime/40)*8),dog,false);
            if(!dog)ApplyCompanyIdentity(unit,rank,endlessSpawned%OfficeCompanySettings.Counts[rank]);
            unit.label.gameObject.SetActive(false);
            endlessSpawned++;
        }
        private void EndlessDefeated(OfficeDefender unit)
        {
            if(unit.warning!=null)Destroy(unit.warning.gameObject);
            endlessKills++;score+=200;
            if(endlessKills%4==0)SpawnEndlessPickup(endlessKills%12==0?1:2,0,unit.transform.position);
        }
        private float AbsorbEndlessDamage(float damage)
        {if(EndlessMechActive)damage*=.5f;float absorbed=Mathf.Min(endlessArmor,damage*.7f);endlessArmor-=absorbed;return damage-absorbed;}
        private void SaveEndlessRecord()
        {
            if(endlessTime>PlayerPrefs.GetFloat("OfficeRampage.Endless.BestTime",0))PlayerPrefs.SetFloat("OfficeRampage.Endless.BestTime",endlessTime);
            if(score>PlayerPrefs.GetInt("OfficeRampage.Endless.BestScore",0))PlayerPrefs.SetInt("OfficeRampage.Endless.BestScore",score);
            PlayerPrefs.Save();
        }
    }
}
