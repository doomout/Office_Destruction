using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        private float robotClock=1.5f;
        private int robotState,robotAttacks;
        private bool robotHit;
        private Vector3 robotDirection;
        private void TickRobot(float dt,Vector3 toward)
        {
            var model=visual.GetComponent<OfficeBossRobot>();
            if(model==null) return;
            robotClock-=dt;
            if(robotState==0)
            {
                model.Swing(0);
                bool dash=robotAttacks%4==2 && toward.magnitude<=30;
                bool ranged=!dash && (robotAttacks%4==3 || toward.magnitude>10);
                float range=ranged?70:dash?30:9;
                if(robotClock<=0 && toward.magnitude<=range && game.ClearDefenseSight(AimPoint,game.PlayerPosition+Vector3.up,transform,null))
                {
                    robotDirection=toward.sqrMagnitude>.01f?toward.normalized:transform.forward;
                    transform.rotation=Quaternion.LookRotation(robotDirection);
                    robotState=ranged?6:dash?2:1;robotClock=ranged?.65f:dash?.55f:.4f;robotHit=false;
                    game.ShowDefenderWarning(this,ranged?2:dash?5:0,range,robotDirection);
                }
                else if(toward.magnitude>5) Move(toward.normalized*7.5f,dt);
                return;
            }
            if(robotState==6)
            {
                if(robotClock>0)return;
                if(warning!=null)warning.gameObject.SetActive(false);
                game.FireRobotPlasma(this,robotDirection);robotState=5;robotClock=.65f;robotAttacks++;return;
            }
            if(robotState==1 || robotState==2)
            {
                model.Swing(-Mathf.Clamp01(1-robotClock/(robotState==2?.55f:.4f)));
                if(robotClock>0) return;
                if(warning!=null) warning.gameObject.SetActive(false);
                robotState=robotState==2?4:3;robotClock=robotState==4?.7f:.28f;
                return;
            }
            if(robotState==3 || robotState==4)
            {
                bool dash=robotState==4;
                model.Swing(Mathf.Clamp01(1-robotClock/(dash?.7f:.28f)));
                if(dash) Move(robotDirection*34,dt);
                Vector3 delta=game.PlayerPosition-transform.position;float height=Mathf.Abs(delta.y);delta.y=0;
                bool inArc=delta.sqrMagnitude<.01f || Vector3.Dot(robotDirection,delta.normalized)>=Mathf.Cos(35*Mathf.Deg2Rad);
                if(!robotHit && height<2.5f && delta.magnitude<=(dash?3.2f:9) && (dash || inArc) && (dash || robotClock<=.16f) && game.ClearDefenseSight(AimPoint,game.PlayerPosition+Vector3.up,transform,null))
                { game.DefenderContact(dash?42:36);robotHit=true; }
                if(robotClock<=0) {robotState=5;robotClock=.55f;robotAttacks++;}
                return;
            }
            model.Swing(0);
            if(robotClock<=0) {robotState=0;robotClock=.2f;}
        }
    }
}
