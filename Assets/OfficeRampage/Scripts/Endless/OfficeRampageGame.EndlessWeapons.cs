using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void AttackEndless()
        {
            if(!RunActive||CombatLocked||cooldown>0||whirlwindLeft>0||thunderLeft>0)return;
            EnsureEndlessFists();if(endlessUnarmed){PunchEndless();return;}
            if(endlessWeapon<7)
            {endlessAttackDelegate=true;try{AttackExpanded();}finally{endlessAttackDelegate=false;}return;}
            if(endlessSpecialAmmo<=0){Notify("탄약 소진 · 바닥의 새 장비를 주우세요",1);cooldown=.4f;return;}
            endlessSpecialAmmo--;swing=1;cooldown=endlessWeapon==7||endlessWeapon==13?2:endlessWeapon==12?1.1f:endlessWeapon==10?.28f:.45f;
            var origin=player.position+Vector3.up*1.2f;Vector3 forward=avatar.forward;
            if(endlessWeapon==13){StartCoroutine(EndlessOrbitalStrike());return;}
            if(endlessWeapon==14){SummonEndlessDemon();return;}
            if(endlessWeapon==15){endlessTrafficLeft=10;endlessCarClock=0;audioBus.Play("thunder",.4f);return;}
            if(endlessWeapon==7)
            {
                var center=player.position+forward*10;center.x=Mathf.Clamp(center.x,-28,28);center.z=Mathf.Clamp(center.z,4,60);
                CreateBlast(center,true);audioBus.Play("explosion",.85f);
                SkillDamage(()=>EndlessArea(center,24,500,false));shake=4;return;
            }
            if(endlessWeapon==8)
            {
                var candidates=new List<OfficeDefender>();foreach(var enemy in defenders)if(enemy!=null&&enemy.Alive&&Vector3.Distance(origin,enemy.AimPoint)<18&&Vector3.Dot(forward,(enemy.AimPoint-origin).normalized)>.25f&&ClearDefenseSight(origin,enemy.AimPoint,player,enemy.transform))candidates.Add(enemy);
                candidates.Sort((a,b)=>Vector3.SqrMagnitude(a.AimPoint-origin).CompareTo(Vector3.SqrMagnitude(b.AimPoint-origin)));Vector3 previous=origin;
                for(int i=0;i<Mathf.Min(5,candidates.Count);i++){var enemy=candidates[i];Trail(previous,enemy.AimPoint,Cyan);previous=enemy.AimPoint;enemy.Hit(45);if(enemy.Alive)enemy.Stun(.7f);}
                audioBus.Play("thunder",.35f);return;
            }
            if(endlessWeapon==9)
            {
                Vector3 direction=view.transform.forward;Vector3 target=view.transform.position+direction*45;direction=(target-origin).normalized;
                var hits=Physics.RaycastAll(origin,direction,45);System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));var seen=new HashSet<OfficeDefender>();var props=new HashSet<OfficeProp>();Vector3 end=origin+direction*45;
                foreach(var hit in hits){if(IsPlayerCollider(hit.collider))continue;var enemy=hit.collider.GetComponentInParent<OfficeDefender>();var prop=hit.collider.GetComponentInParent<OfficeProp>();if(enemy!=null){if(seen.Add(enemy))enemy.Hit(60);}else if(prop!=null){if(props.Add(prop))prop.Hit(80,direction);}else{end=hit.point;break;}}
                Trail(origin,end,Color.magenta);audioBus.Play("orbitalLaser",.2f,1.8f);return;
            }
            if(endlessWeapon==12)
            {SkillDamage(()=>EndlessArea(player.position,11,75,true));for(int i=0;i<20;i++){float angle=i*Mathf.PI/10;Fx("Bell wave",origin,Vector3.one*.2f,Color.yellow,.5f,new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*22);}audioBus.Play("thunder",.45f);return;}
            float range=endlessWeapon==10?9:14;
            foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive&&Vector3.Distance(origin,enemy.AimPoint)<range&&Vector3.Dot(forward,(enemy.AimPoint-origin).normalized)>.55f&&ClearDefenseSight(origin,enemy.AimPoint,player,enemy.transform))
            {
                enemy.Hit(endlessWeapon==10?22:28);
                if(enemy.Alive){if(endlessWeapon==10)Ignite(enemy.gameObject);else{enemy.Stun(.4f);var pull=(player.position-enemy.transform.position).normalized*1.8f;enemy.GetComponent<CharacterController>().Move(pull);}}
            }
            var touched=new HashSet<OfficeProp>();foreach(var col in Physics.OverlapSphere(origin,range)){var prop=col.GetComponentInParent<OfficeProp>();if(prop!=null&&prop.health>0&&touched.Add(prop)&&Vector3.Dot(forward,(prop.transform.position-origin).normalized)>.55f){if(endlessWeapon==10)Ignite(prop.gameObject);else prop.Hit(28,forward);}}
            for(int i=-3;i<=3;i++){var ray=Quaternion.Euler(0,i*10,0)*forward;Fx("Special stream",endlessWeapon==10?origin:origin+ray*range,Vector3.one*.2f,endlessWeapon==10?Orange:Cyan,.35f,ray*(endlessWeapon==10?range/.35f:-range/.35f));}
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
