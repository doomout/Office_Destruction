using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private float sprintEnergy=10,runCycle,windClock;
        private bool sprinting,sprintExhausted;
        private void ResetSprint() { sprintEnergy=10; runCycle=windClock=0; sprinting=sprintExhausted=false; }
        private float SprintSpeed(bool held,bool moving,float dt)
        {
            if(!RunActive || CombatLocked) { sprinting=false; return 5.6f; }
            sprinting=held && moving && !sprintExhausted && sprintEnergy>0;
            float activeTime=sprinting?Mathf.Min(dt,sprintEnergy):0;
            if(sprinting)
            {
                sprintEnergy=Mathf.Max(0,sprintEnergy-dt);
                if(sprintEnergy<=0) sprintExhausted=true;
            }
            else
            {
                sprintEnergy=Mathf.Min(10,sprintEnergy+dt*2*StorySprintRecovery);
                if(!held && sprintEnergy>=2) sprintExhausted=false;
            }
            // A partial final frame cannot grant more than ten seconds of boosted movement.
            return 5.6f+(dt>0?activeTime/dt:0)*2.4f;
        }
        private void AnimatePlayerRun(float dt,Vector3 displacement)
        {
            displacement.y=0; bool moving=displacement.sqrMagnitude>.00001f;
            float amplitude=moving?(sprinting?48:24):0;
            if(moving) runCycle+=dt*(sprinting?18:9);
            foreach(Transform part in avatar)
            {
                float side=Mathf.Sign(part.localPosition.x);
                float angle=Mathf.Sin(runCycle+(side<0?Mathf.PI:0))*amplitude;
                Quaternion rotation=Quaternion.Euler(angle,0,0);
                if(part.name=="Trouser")
                { part.localRotation=rotation; part.localPosition=new Vector3(side*.19f,.71f,0)+rotation*Vector3.down*.35f; }
                if(part.name=="Shoe")
                { part.localRotation=rotation; part.localPosition=new Vector3(side*.19f,.71f,0)+rotation*new Vector3(0,-.63f,.08f); }
            }
            windClock-=dt;
            if(sprinting && moving && windClock<=0)
            {
                windClock=.09f;
                for(int i=0;i<3;i++)
                {
                    Vector3 at=player.position+Vector3.up*(.5f+i*.45f)+avatar.right*(i%2==0?.55f:-.55f);
                    var wind=Fx("Sprint wind",at,new Vector3(.035f,.035f,.9f),Cyan,.22f,-displacement.normalized*5);
                    wind.transform.rotation=Quaternion.LookRotation(displacement.normalized);
                }
            }
        }
        private void DrawSprintHUD()
        {
            DrawCompactSprint();
        }
    }
}
