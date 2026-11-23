using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform endlessMechRoot,endlessMechSword,endlessMechOtherSword,endlessMechLeftLeg,endlessMechRightLeg;
        private float endlessMechLeft,endlessMechDashCooldown,endlessMechSwing,endlessMechWalk;
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
            avatar.gameObject.SetActive(false);endlessMechLeft=20;endlessMechDashCooldown=0;endlessMechSwing=0;endlessMechWalk=0;endlessMechPrevious=player.position;
            pitch=25;cooldown=.3f;audioBus.Play("explosion",.5f);shake=1.2f;
            ChangeEndlessEnvironment();
            Notify("헐크버스터 탑승 · 20초 · 왼쪽 쌍검 / 오른쪽 대쉬",4);
        }
        private void ResetEndlessMech()
        {
            if(endlessMechRoot!=null){endlessMechRoot.gameObject.SetActive(false);Destroy(endlessMechRoot.gameObject);}
            if(mechShapeSaved&&controller!=null)
            {
                bool enabled=controller.enabled;controller.enabled=false;controller.radius=mechOldRadius;controller.height=mechOldHeight;controller.center=mechOldCenter;controller.enabled=enabled;
            }
            if(avatar!=null)avatar.gameObject.SetActive(true);
            ResetMechDash();mechShapeSaved=false;endlessMechRoot=endlessMechSword=endlessMechOtherSword=endlessMechLeftLeg=endlessMechRightLeg=null;
            endlessMechLeft=endlessMechDashCooldown=endlessMechSwing=endlessMechWalk=0;
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
            endlessMechSword.localRotation=HorizontalSwingRotation(endlessMechSwing);
            endlessMechOtherSword.localRotation=Quaternion.Inverse(endlessMechSword.localRotation);
            endlessMechDashCooldown=Mathf.Max(0,endlessMechDashCooldown-dt);
            TickMechDash(dt);
            if(Mouse.current!=null&&Mouse.current.rightButton.wasPressedThisFrame&&Cursor.lockState==CursorLockMode.Locked)StartMechDash();
        }
        private void AttackEndlessMech()
        {
            if(!EndlessMechActive||!RunActive||CombatLocked||cooldown>0)return;
            cooldown=.75f;endlessMechSwing=1;shake=.7f;audioBus.Play("swing",.6f,.65f);
            Vector3 forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            SkillDamage(()=>SweepWeapon(player.position+Vector3.up*1.35f,forward,7,140,120,false));
            var arc=new Vector3[17];for(int i=0;i<arc.Length;i++)arc[i]=player.position+Vector3.up*2.8f+(Quaternion.Euler(0,-70+i*140f/16,0)*forward)*5;
            EnergyStroke("Mech red sword arc",arc,Color.red,.16f,.22f);
            for(int i=0;i<arc.Length;i++)arc[i]+=Vector3.up*.3f;
            EnergyStroke("Mech green sword arc",arc,Color.green,.16f,.22f);
        }
    }
}
