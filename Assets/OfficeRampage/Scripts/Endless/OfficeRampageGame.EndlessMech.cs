using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform endlessMechRoot,endlessMechSword,endlessMechLeftLeg,endlessMechRightLeg;
        private float endlessMechLeft,endlessMechShotClock,endlessMechSwing,endlessMechWalk;
        private Vector3 endlessMechPrevious,mechOldCenter;
        private float mechOldHeight,mechOldRadius;
        private bool mechShapeSaved;
        private bool EndlessMechActive=>endlessMode&&endlessMechRoot!=null&&endlessMechLeft>0;
        private void ActivateEndlessMech()
        {
            if(!endlessMode||!RunActive||CombatLocked||EndlessMechActive)return;
            ResetElements();whirlwindLeft=whirlwindReady=0;ClearDecoy();endlessSaber=false;
            SkillDamage(()=>EndlessArea(player.position,3,60,true));
            mechOldHeight=controller.height;mechOldRadius=controller.radius;mechOldCenter=controller.center;mechShapeSaved=true;
            controller.enabled=false;
            player.position=new Vector3(Mathf.Clamp(player.position.x,-30,30),player.position.y,Mathf.Clamp(player.position.z,2,62));
            controller.height=6.6f;controller.radius=1.05f;controller.center=Vector3.up*3.3f;controller.enabled=true;
            endlessMechRoot=new GameObject("Player hulkbuster").transform;endlessMechRoot.SetParent(player,false);BuildEndlessMechFrame();
            avatar.gameObject.SetActive(false);endlessMechLeft=20;endlessMechShotClock=0;endlessMechSwing=0;endlessMechWalk=0;endlessMechPrevious=player.position;
            pitch=25;cooldown=.3f;audioBus.Play("explosion",.5f);shake=1.2f;
            ChangeEndlessEnvironment();
            Notify("헐크버스터 탑승 · 20초 · 왼쪽 거대 검 / 오른쪽 광선",4);
        }
        private void ResetEndlessMech()
        {
            if(endlessMechRoot!=null){endlessMechRoot.gameObject.SetActive(false);Destroy(endlessMechRoot.gameObject);}
            if(mechShapeSaved&&controller!=null)
            {
                bool enabled=controller.enabled;controller.enabled=false;controller.radius=mechOldRadius;controller.height=mechOldHeight;controller.center=mechOldCenter;controller.enabled=enabled;
            }
            if(avatar!=null)avatar.gameObject.SetActive(true);
            mechShapeSaved=false;endlessMechRoot=endlessMechSword=endlessMechLeftLeg=endlessMechRightLeg=null;
            endlessMechLeft=endlessMechShotClock=endlessMechSwing=endlessMechWalk=0;
        }
        private void TickEndlessMech(float dt)
        {
            if(!EndlessMechActive||!RunActive||CombatLocked)return;
            endlessMechLeft=Mathf.Max(0,endlessMechLeft-dt);
            if(endlessMechLeft<=0)
            {ResetEndlessMech();EquipEndlessSaber();pitch=30;cooldown=.25f;Notify("탑승 종료 · 기본 광선검으로 복귀",3);return;}
            float distance=Vector3.Distance(player.position,endlessMechPrevious);endlessMechPrevious=player.position;endlessMechWalk+=distance*1.7f;
            float angle=distance>.001f?Mathf.Sin(endlessMechWalk)*24:0;
            endlessMechLeftLeg.localRotation=Quaternion.Euler(angle,0,0);endlessMechRightLeg.localRotation=Quaternion.Euler(-angle,0,0);
            endlessMechRoot.rotation=Quaternion.Euler(0,yaw,0);endlessMechSwing=Mathf.Max(0,endlessMechSwing-dt*3);
            endlessMechSword.localRotation=Quaternion.Euler(12,endlessMechSwing>0?Mathf.Lerp(75,-65,endlessMechSwing):0,0);
            endlessMechShotClock-=dt;
            if(Mouse.current!=null&&Mouse.current.rightButton.isPressed&&Cursor.lockState==CursorLockMode.Locked)FireEndlessMechLaser();
        }
        private void AttackEndlessMech()
        {
            if(!EndlessMechActive||!RunActive||CombatLocked||cooldown>0)return;
            cooldown=.75f;endlessMechSwing=1;shake=.7f;audioBus.Play("swing",.6f,.65f);
            Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            SkillDamage(()=>SweepWeapon(player.position+Vector3.up*1.35f,forward,7,140,120,false));
            for(int i=-3;i<=3;i++)Fx("Mech sword arc",player.position+Vector3.up*1.5f+(Quaternion.Euler(0,i*18,0)*forward)*5,Vector3.one*.3f,Cyan,.2f,Vector3.zero);
        }
        private void FireEndlessMechLaser()
        {
            if(!EndlessMechActive||!RunActive||CombatLocked||endlessMechShotClock>0)return;
            endlessMechShotClock=.7f;Vector3 origin=player.position+Vector3.up*3.4f;
            Vector3 point=view.transform.position+view.transform.forward*50;
            if(AimHit(view.transform.position,view.transform.forward,50,out var aim))point=aim.point;
            Vector3 direction=(point-origin).normalized;Vector3 end=origin+direction*50;float distance=50;Collider target=null;
            foreach(var hit in Physics.SphereCastAll(origin,.45f,direction,50))if(!IsPlayerCollider(hit.collider)&&hit.distance<distance){distance=hit.distance;end=hit.point;target=hit.collider;}
            if(target!=null)
            {
                var enemy=target.GetComponentInParent<OfficeDefender>();var prop=target.GetComponentInParent<OfficeProp>();
                SkillDamage(()=>{if(enemy!=null)enemy.Hit(90);if(prop!=null)prop.Hit(90,direction);});
            }
            var beam=Fx("Player mech laser",(origin+end)/2,new Vector3(.22f,.22f,Vector3.Distance(origin,end)),Cyan,.18f,Vector3.zero);
            beam.transform.rotation=Quaternion.LookRotation(direction);audioBus.Play("orbitalLaser",.35f,1.5f);shake=.35f;
        }
    }
}
