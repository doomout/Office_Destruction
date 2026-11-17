using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        private static readonly float[] EndlessAvoidAngles={0f,45,-45,90,-90};
        private void PatrolEndless(float dt,Vector3 toward)
        {
            if(toward.magnitude<1.5f)return;
            Vector3 direction=toward.normalized;
            foreach(float angle in EndlessAvoidAngles)
            {
                var candidate=Quaternion.Euler(0,angle,0)*direction;bool blocked=false;
                foreach(var hit in Physics.SphereCastAll(transform.position+Vector3.up*.8f,.33f,candidate,1.2f))
                    if(hit.transform!=transform&&!hit.transform.IsChildOf(transform)&&!game.IsPlayerCollider(hit.collider)){blocked=true;break;}
                if(!blocked){Move(candidate*(dog?6:3.2f),dt);return;}
            }
        }
    }
}
