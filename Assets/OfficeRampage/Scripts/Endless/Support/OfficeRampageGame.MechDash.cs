using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private float mechDashLeft,mechDashTrailClock;
        private Vector3 mechDashDirection;
        private readonly HashSet<OfficeDefender> mechDashEnemies=new HashSet<OfficeDefender>();
        private readonly HashSet<OfficeProp> mechDashProps=new HashSet<OfficeProp>();
        private void ResetMechDash()
        {mechDashLeft=mechDashTrailClock=0;mechDashEnemies.Clear();mechDashProps.Clear();}
        private void StartMechDash()
        {
            if(!EndlessMechActive||!RunActive||CombatLocked||paused||endlessMechDashCooldown>0)return;
            ResetMechDash();endlessMechDashCooldown=3;mechDashLeft=.35f;
            mechDashDirection=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            audioBus.Play("swing",.8f,.6f);shake=.9f;
        }
        private void TickMechDash(float dt)
        {
            if(mechDashLeft<=0||dt<=0)return;
            float remaining=Mathf.Min(dt,mechDashLeft);mechDashLeft=Mathf.Max(0,mechDashLeft-dt);
            // Short movement steps keep impact detection reliable during low frame rates.
            while(remaining>0)
            {
                float step=Mathf.Min(remaining,1f/90);remaining-=step;
                foreach(var hit in Physics.OverlapSphere(player.position+Vector3.up*1.4f+mechDashDirection,2.1f))
                {
                    if(IsPlayerCollider(hit))continue;
                    var enemy=hit.GetComponentInParent<OfficeDefender>();var prop=hit.GetComponentInParent<OfficeProp>();
                    if(enemy!=null&&enemy.Alive&&mechDashEnemies.Add(enemy))SkillDamage(()=>{enemy.Hit(120);enemy.Stun(1.5f);});
                    if(prop!=null&&mechDashProps.Add(prop))SkillDamage(()=>prop.Hit(120,mechDashDirection));
                }
                var flags=controller.Move(mechDashDirection*(30*step));
                if((flags&CollisionFlags.Sides)!=0){mechDashLeft=0;break;}
            }
            mechDashTrailClock-=dt;
            if(mechDashTrailClock<=0)
            {
                mechDashTrailClock=.04f;Vector3 side=Vector3.Cross(Vector3.up,mechDashDirection);
                for(int i=-1;i<=1;i+=2)
                {
                    Vector3 point=player.position+Vector3.up*3+side*i*1.9f;
                    EnergyStroke("Mech dash booster",new[]{point,point-mechDashDirection*4},i<0?Color.green:Color.red,.4f,.22f);
                }
            }
        }
    }
}
