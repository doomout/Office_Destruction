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
            ResetAirSupport();ClearDoubleDamageVisual();ResetEndlessDemon();ResetEndlessSupply();endlessGunKick=0;ResetEndlessMech();endlessSaber=endlessMode;endlessDoubleDamageLeft=endlessDemonLeft=endlessTrafficLeft=endlessCarClock=endlessDemonAttack=0;
            endlessDemon=null;endlessCars.Clear();endlessOrbitalStage=0;
        }
        private void SeedEndlessEquipment()
        {
            for(int i=0;i<8;i++)SpawnEndlessPickup(0,new[]{2,1,3,5,6,8,9,10}[i],new Vector3((i%4-1.5f)*4,.08f,37+(i/4)*5));
            SpawnEndlessPickup(0,14,new Vector3(-6,.08f,33));SpawnEndlessPickup(0,15,new Vector3(6,.08f,33));
            SpawnEndlessPickup(0,16,new Vector3(0,.08f,25));
            SpawnEndlessPickup(0,11,new Vector3(-8,.08f,25));
            SpawnEndlessPickup(0,17,new Vector3(-10,.08f,29));SpawnEndlessPickup(0,18,new Vector3(10,.08f,29));SpawnEndlessPickup(0,19,new Vector3(0,.08f,20));
            SpawnEndlessPickup(1,0,new Vector3(-4,.08f,29));SpawnEndlessPickup(2,0,new Vector3(4,.08f,29));
        }
        private void TickEndlessExtras(float dt)
        {
            endlessDoubleDamageLeft=Mathf.Max(0,endlessDoubleDamageLeft-dt);
            TickDoubleDamageVisual(dt);
            TickEndlessMech(dt);EnsureEndlessSaber();TickEndlessDemon(dt);TickEndlessTraffic(dt);TickAirSupport(dt);
        }
        private void EnsureEndlessSaber()
        {
            if(!endlessMode||endlessSaber||EndlessMechActive||CombatLocked||whirlwindLeft>0||thunderLeft>0)return;
            if(endlessWeapon==4){ClearDecoy();EquipEndlessSaber();return;}
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


    }
}
