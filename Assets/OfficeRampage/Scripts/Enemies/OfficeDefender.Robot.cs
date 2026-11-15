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
                bool dash=robotAttacks%3==2;
                float range=dash?22:8;
                if(robotClock<=0 && toward.magnitude<=range && game.ClearDefenseSight(AimPoint,game.PlayerPosition+Vector3.up,transform,null))
                {
                    robotDirection=toward.sqrMagnitude>.01f?toward.normalized:transform.forward;
                    transform.rotation=Quaternion.LookRotation(robotDirection);
                    robotState=dash?2:1;robotClock=dash?1: .85f;robotHit=false;
                    game.ShowDefenderWarning(this,dash?5:0,range,robotDirection);
                }
                else if(toward.magnitude>5) Move(toward.normalized*5.4f,dt);
                return;
            }
            if(robotState==1 || robotState==2)
            {
                model.Swing(-Mathf.Clamp01(1-robotClock/(robotState==2?1:.85f)));
                if(robotClock>0) return;
                if(warning!=null) warning.gameObject.SetActive(false);
                robotState=robotState==2?4:3;robotClock=robotState==4?.7f:.55f;
                return;
            }
            if(robotState==3 || robotState==4)
            {
                bool dash=robotState==4;
                model.Swing(Mathf.Clamp01(1-robotClock/(dash?.7f:.55f)));
                if(dash) Move(robotDirection*22,dt);
                Vector3 delta=game.PlayerPosition-transform.position;float height=Mathf.Abs(delta.y);delta.y=0;
                bool inArc=delta.sqrMagnitude<.01f || Vector3.Dot(robotDirection,delta.normalized)>=Mathf.Cos(35*Mathf.Deg2Rad);
                if(!robotHit && height<2.5f && delta.magnitude<=(dash?3.2f:8) && (dash || inArc) && (dash || robotClock<=.3f) && game.ClearDefenseSight(AimPoint,game.PlayerPosition+Vector3.up,transform,null))
                { game.DefenderContact(dash?28:24);robotHit=true; }
                if(robotClock<=0) {robotState=5;robotClock=1.1f;robotAttacks++;}
                return;
            }
            model.Swing(0);
            if(robotClock<=0) {robotState=0;robotClock=.4f;}
        }
    }
}
