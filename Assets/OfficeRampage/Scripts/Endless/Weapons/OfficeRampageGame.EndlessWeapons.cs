using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void AttackEndless()
        {
            if(!RunActive||CombatLocked||cooldown>0||whirlwindLeft>0||thunderLeft>0)return;
            if(EndlessMechActive){AttackEndlessMech();return;}
            EnsureEndlessSaber();if(endlessSaber){AttackEndlessSaber();return;}
            if(endlessWeapon<7)
            {endlessAttackDelegate=true;try{AttackExpanded();}finally{endlessAttackDelegate=false;}return;}
            if(EndlessUltimateActive&&(endlessWeapon==7||endlessWeapon==11||endlessWeapon==13||endlessWeapon==16||endlessWeapon>=18)){cooldown=.3f;return;}
            if(endlessSpecialAmmo<=0){Notify("탄약 소진 · 바닥의 새 장비를 주우세요",1);cooldown=.4f;return;}
            endlessSpecialAmmo--;swing=1;cooldown=endlessWeapon==7||endlessWeapon==13?2:endlessWeapon==12?1.1f:endlessWeapon==10?.28f:.45f;
            var origin=player.position+Vector3.up*1.2f;Vector3 forward=avatar.forward;
            if(endlessWeapon==11){StartUFO();return;}
            if(endlessWeapon==17){SummonSupportDrones();return;}
            if(endlessWeapon==18){StartJetStrike();return;}
            if(endlessWeapon==19){StartBlackHole();return;}
            if(endlessWeapon==EarthquakeWeaponId){StartEarthquake();return;}
            if(endlessWeapon==MeteorWeaponId){StartMeteor();return;}
            if(endlessWeapon==16){ActivateEndlessMech();return;}
            if(endlessWeapon==13){StartCoroutine(EndlessOrbitalStrike());return;}
            if(endlessWeapon==14){SummonEndlessDemon();return;}
            if(endlessWeapon==15){endlessTrafficLeft=10;endlessCarClock=0;audioBus.Play("thunder",.4f);return;}
            if(endlessWeapon==7)
            {
                var center=player.position+forward*10;center.x=Mathf.Clamp(center.x,-28,28);center.z=Mathf.Clamp(center.z,4,60);
                CreateBlast(center,true);audioBus.Play("explosion",.85f);
                SkillDamage(()=>EndlessArea(center,24,500,false));ChangeEndlessEnvironment();shake=4;return;
            }
            if(endlessWeapon==8)
            {
                endlessGunKick=.32f;origin=EndlessGunMuzzle();GlowFx("Stapler muzzle flash",origin,Vector3.one*.4f,Cyan,.25f,Vector3.zero);
                var candidates=new List<OfficeDefender>();foreach(var enemy in defenders)if(enemy!=null&&enemy.Alive&&Vector3.Distance(origin,enemy.AimPoint)<18&&Vector3.Dot(forward,(enemy.AimPoint-origin).normalized)>.25f&&ClearDefenseSight(origin,enemy.AimPoint,player,enemy.transform))candidates.Add(enemy);
                candidates.Sort((a,b)=>Vector3.SqrMagnitude(a.AimPoint-origin).CompareTo(Vector3.SqrMagnitude(b.AimPoint-origin)));Vector3 previous=origin;
                for(int i=0;i<Mathf.Min(5,candidates.Count);i++){var enemy=candidates[i];ElectricDischarge(previous,enemy.AimPoint);previous=enemy.AimPoint;enemy.Hit(45);if(enemy.Alive)enemy.Stun(.7f);}
                if(candidates.Count==0)
                {
                    Vector3 end=origin+forward*18;foreach(var hit in Physics.RaycastAll(origin,forward,18))if(!IsPlayerCollider(hit.collider)&&Vector3.Distance(origin,hit.point)<Vector3.Distance(origin,end))end=hit.point;
                    ElectricDischarge(origin,end);
                }
                audioBus.Play("thunder",.35f);return;
            }
            if(endlessWeapon==9)
            {
                endlessGunKick=.32f;origin=EndlessGunMuzzle();
                Vector3 direction=EndlessAimDirection(origin,45);
                var hits=Physics.RaycastAll(origin,direction,45);System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));var seen=new HashSet<OfficeDefender>();var props=new HashSet<OfficeProp>();Vector3 end=origin+direction*45;
                foreach(var hit in hits){if(IsPlayerCollider(hit.collider))continue;var enemy=hit.collider.GetComponentInParent<OfficeDefender>();var prop=hit.collider.GetComponentInParent<OfficeProp>();if(enemy!=null){if(seen.Add(enemy))enemy.Hit(60);}else if(prop!=null){if(props.Add(prop))prop.Hit(80,direction);}else{end=hit.point;break;}}
                PrinterBeam(origin,end);audioBus.Play("orbitalLaser",.2f,1.8f);return;
            }
            if(endlessWeapon==12)
            {SkillDamage(()=>EndlessArea(player.position,11,75,true));for(int i=0;i<20;i++){float angle=i*Mathf.PI/10;Fx("Bell wave",origin,Vector3.one*.2f,Color.yellow,.5f,new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*22);}audioBus.Play("thunder",.45f);return;}
            float range=9;
            foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive&&Vector3.Distance(origin,enemy.AimPoint)<range&&Vector3.Dot(forward,(enemy.AimPoint-origin).normalized)>.55f&&ClearDefenseSight(origin,enemy.AimPoint,player,enemy.transform))
            {
                enemy.Hit(22);
                if(enemy.Alive)Ignite(enemy.gameObject);
            }
            var touched=new HashSet<OfficeProp>();foreach(var col in Physics.OverlapSphere(origin,range)){var prop=col.GetComponentInParent<OfficeProp>();if(prop!=null&&prop.health>0&&touched.Add(prop)&&Vector3.Dot(forward,(prop.transform.position-origin).normalized)>.55f){Ignite(prop.gameObject);}}
            for(int i=-3;i<=3;i++){var ray=Quaternion.Euler(0,i*10,0)*forward;Fx("Special stream",origin,Vector3.one*.2f,Orange,.35f,ray*(range/.35f));}
            audioBus.Play("swing",.3f);
        }
        private void EndlessArea(Vector3 center,float radius,float damage,bool stun)
        {
            foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive&&Vector3.Distance(center,enemy.transform.position)<=radius){enemy.Hit(damage);if(stun&&enemy.Alive)enemy.Stun(2);}
            var seen=new HashSet<OfficeProp>();foreach(var col in Physics.OverlapSphere(center,radius)){var prop=col.GetComponentInParent<OfficeProp>();if(prop!=null&&seen.Add(prop))prop.Hit(damage,Vector3.up);}
        }
        private void EndlessPropDestroyed(int kind,Vector3 position)
        {
            current.destroyed++;score+=100;
            if(skillDamageDepth==0){combo=comboLeft>0?combo+1:1;comboLeft=4;ChargeWhirlwind();ChargeElements();score+=Mathf.Min(combo,20)*15;}
            if(kind==OfficeRules.WaterCooler)hp=Mathf.Min(100,hp+5);
            if(OfficeRules.IsElectronic(kind))
            {
                for(int i=0;i<6;i++)Fx("Office blast spark",position,Vector3.one*.35f,Orange,.5f,Random.onUnitSphere*4,2);
                audioBus.Play("explosion",.35f);
                foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive&&Vector3.Distance(enemy.transform.position,position)<4)enemy.Hit(40);
            }
        }
    }
}
