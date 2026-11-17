using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool endlessSaber;
        private float endlessDoubleDamageLeft,endlessDemonLeft,endlessTrafficLeft,endlessCarClock,endlessDemonAttack;
        private Transform endlessDemon;
        public float PlayerDamage(float amount)=>amount*(endlessMode&&endlessDoubleDamageLeft>0?2:1);
        private void RegisterDefeatCombo(OfficeDefender unit)
        {
            if(unit.dog||unit.boss||skillDamageDepth>0)return;
            combo=comboLeft>0?combo+1:1;comboLeft=4;ChargeWhirlwind();ChargeElements();
        }
        private void ResetEndlessExtras()
        {
            ResetEndlessMech();endlessSaber=endlessMode;endlessDoubleDamageLeft=endlessDemonLeft=endlessTrafficLeft=endlessCarClock=endlessDemonAttack=0;
            endlessDemon=null;endlessCars.Clear();endlessOrbitalStage=0;
        }
        private void SeedEndlessEquipment()
        {
            for(int i=0;i<6;i++)SpawnEndlessPickup(0,new[]{2,1,3,5,6,4}[i],new Vector3((i%3-1)*4,.08f,37+(i/3)*5));
            SpawnEndlessPickup(0,14,new Vector3(-6,.08f,33));SpawnEndlessPickup(0,15,new Vector3(6,.08f,33));
            SpawnEndlessPickup(0,16,new Vector3(0,.08f,25));
            SpawnEndlessPickup(1,0,new Vector3(-4,.08f,29));SpawnEndlessPickup(2,0,new Vector3(4,.08f,29));
        }
        private void TickEndlessExtras(float dt)
        {
            endlessDoubleDamageLeft=Mathf.Max(0,endlessDoubleDamageLeft-dt);
            TickEndlessMech(dt);EnsureEndlessSaber();TickEndlessDemon(dt);TickEndlessTraffic(dt);
        }
        private void EnsureEndlessSaber()
        {
            if(!endlessMode||endlessSaber||EndlessMechActive||CombatLocked||whirlwindLeft>0||thunderLeft>0)return;
            bool empty=endlessWeapon>=7?endlessSpecialAmmo<=0:endlessWeapon==4?decoysUsed[1]>=3:ammunition[weaponIndex]==0;
            if(!empty)return;
            EquipEndlessSaber();
            Notify("탄약 소진 · 기본 광선검으로 전환 · 3콤보 후 F 휠윈드",3);
        }
        private void EquipEndlessSaber()
        {
            endlessSaber=true;ResetElements();whirlwindReady=whirlwindLeft=0;
            for(int i=weapon.childCount-1;i>=0;i--){weapon.GetChild(i).gameObject.SetActive(false);Destroy(weapon.GetChild(i).gameObject);}
            weaponIndex=0;weapon.localScale=Vector3.one;
            BuildEndlessSaber();
        }
        private void AttackEndlessSaber()
        {
            endlessAttackDelegate=true;try{AttackExpanded();}finally{endlessAttackDelegate=false;}
        }
        private void BuildEndlessSummonWeapon()
        {
            if(endlessWeapon==16){BuildEndlessMechRemote();return;}
            if(endlessWeapon==14)
            {
                Shape(weapon,"Infernal book",new Vector3(0,.1f,.4f),new Vector3(.6f,.15f,.7f),new Color(.3f,.01f,.02f),false);
                Shape(weapon,"Portal rune",new Vector3(0,.19f,.4f),new Vector3(.3f,.02f,.35f),Orange,false);
            }
            else
            {
                Shape(weapon,"Traffic remote",new Vector3(0,.1f,.35f),new Vector3(.25f,.15f,.5f),Navy,false);
                for(int i=0;i<3;i++)Shape(weapon,"Traffic signal",new Vector3(0,.2f,.2f+i*.15f),Vector3.one*.1f,i==0?Color.red:i==1?Color.yellow:Color.green,false,PrimitiveType.Sphere);
            }
        }
        private void SummonEndlessDemon()
        {
            if(endlessDemon!=null)Destroy(endlessDemon.gameObject);
            endlessDemonLeft=10;endlessDemonAttack=0;
            Vector3 at=player.position+avatar.forward*2;
            for(int i=0;i<24;i++){float a=i*Mathf.PI/12;Fx("Hell portal",at+new Vector3(Mathf.Cos(a)*1.4f,.1f,Mathf.Sin(a)*1.4f),Vector3.one*.3f,Orange,1.8f,Vector3.up*.4f);}
            endlessDemon=new GameObject("Friendly demon").transform;endlessDemon.SetParent(world,false);endlessDemon.position=at;
            var red=new Color(.65f,.03f,.04f);
            Shape(endlessDemon,"Demon body",Vector3.up*1.1f,new Vector3(.9f,1.3f,.6f),red,false,PrimitiveType.Capsule);
            Shape(endlessDemon,"Demon head",Vector3.up*2,new Vector3(.7f,.65f,.6f),red,false,PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(endlessDemon,"Horn",new Vector3(side*.3f,2.5f,0),new Vector3(.15f,.65f,.15f),Color.yellow,false).transform.localRotation=Quaternion.Euler(0,0,-side*24);
                Shape(endlessDemon,"Demon wing",new Vector3(side*.8f,1.5f,-.25f),new Vector3(.8f,.9f,.1f),red*.7f,false).transform.localRotation=Quaternion.Euler(0,side*25,side*25);
                Shape(endlessDemon,"Demon eye",new Vector3(side*.15f,2.06f,.3f),new Vector3(.13f,.1f,.05f),Color.yellow,false);
            }
            audioBus.Play("thunder",.6f);
        }
        private void TickEndlessDemon(float dt)
        {
            if(endlessDemon==null)return;
            endlessDemonLeft=Mathf.Max(0,endlessDemonLeft-dt);
            if(endlessDemonLeft<=0){Destroy(endlessDemon.gameObject);endlessDemon=null;return;}
            OfficeDefender target=null;float distance=24;
            foreach(var enemy in defenders)if(enemy!=null&&enemy.Alive){float d=Vector3.Distance(enemy.transform.position,endlessDemon.position);if(d<distance){target=enemy;distance=d;}}
            Vector3 destination=target!=null?target.transform.position:player.position-avatar.right*2;
            Vector3 delta=destination-endlessDemon.position;delta.y=0;
            if(delta.magnitude>1.5f)endlessDemon.position=Vector3.MoveTowards(endlessDemon.position,destination,dt*9);
            if(delta.sqrMagnitude>.01f)endlessDemon.rotation=Quaternion.LookRotation(delta);
            endlessDemonAttack-=dt;
            if(target!=null&&Vector3.Distance(target.transform.position,endlessDemon.position)<3&&endlessDemonAttack<=0)
            {
                endlessDemonAttack=.5f;SkillDamage(()=>target.Hit(45));
                Trail(endlessDemon.position+Vector3.up*1.7f,target.AimPoint,Orange);audioBus.Play("swing",.2f);
            }
        }
    }
}
