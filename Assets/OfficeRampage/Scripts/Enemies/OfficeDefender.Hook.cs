using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        private int hookState;
        private float hookCooldown=2.5f,hookTime,hookTravel;
        private Vector3 hookDirection,hookPosition;
        private Transform hookVisual;
        private bool TickBossHook(float dt)
        {
            if(!boss||game.IsEndless)return false;
            hookCooldown-=dt;
            Vector3 target=game.PlayerPosition+Vector3.up;
            if(hookState==0)
            {
                float distance=Vector3.Distance(AimPoint,target);
                if(hookCooldown>0||pendingAttack>=0||paperBurstActive||recovery>0||robotState!=0||distance<6||distance>45||!game.ClearDefenseSight(AimPoint,target,transform,null))return false;
                hookState=1;hookTime=Enraged?.8f:1f;hookTravel=0;
                hookDirection=(target-AimPoint).normalized;
                Vector3 facing=hookDirection;facing.y=0;transform.rotation=Quaternion.LookRotation(facing);
                game.ShowDefenderWarning(this,7,45,facing);
                visual.GetComponent<OfficeCharacterVisual>()?.PlayAction("holding-right-shoot");
                return true;
            }
            hookTime-=dt;
            if(hookState==1)
            {
                if(hookTime>0)return true;
                if(warning!=null)warning.gameObject.SetActive(false);
                hookState=2;hookTime=1.5f;hookPosition=AimPoint;
                hookVisual=game.CreateBossHookVisual();
                game.UpdateBossHookVisual(hookVisual,AimPoint,hookPosition);
                return true;
            }
            if(hookState==2)
            {
                Vector3 previous=hookPosition;
                float step=Mathf.Min(36*dt,45-hookTravel);
                hookPosition+=hookDirection*step;hookTravel+=step;
                if(!game.ClearDefenseSight(previous,hookPosition,transform,null)){CancelBossHook();return true;}
                Vector3 segment=hookPosition-previous;
                float t=segment.sqrMagnitude>.0001f?Mathf.Clamp01(Vector3.Dot(target-previous,segment)/segment.sqrMagnitude):0;
                if(Vector3.Distance(previous+segment*t,target)<.85f)
                {hookState=3;hookTime=1.3f;game.DefenderContact(8);}
                else if(hookTravel>=45||hookTime<=0){CancelBossHook();return true;}
            }
            if(hookState==3)
            {
                Vector3 offset=game.PlayerPosition-transform.position;offset.y=0;
                float stop=PhaseTwo?5:2.3f;
                if(offset.magnitude<=stop+.2f||hookTime<=0){CancelBossHook();return true;}
                Vector3 destination=transform.position+offset.normalized*stop;
                if(!game.PullPlayerTowardBoss(destination,dt)){CancelBossHook();return true;}
                hookPosition=game.PlayerPosition+Vector3.up;
            }
            if(hookVisual!=null)game.UpdateBossHookVisual(hookVisual,AimPoint,hookPosition);
            return true;
        }
        public void CancelBossHook()
        {
            if(hookState!=0){hookCooldown=Enraged?5:7;recovery=.65f;robotClock=Mathf.Max(robotClock,.65f);if(warning!=null)warning.gameObject.SetActive(false);}
            hookState=0;
            if(hookVisual!=null){hookVisual.gameObject.SetActive(false);Destroy(hookVisual.gameObject);hookVisual=null;}
        }
        public void CancelCombatForCinematic()
        {
            CancelBossHook();pendingAttack=-1;windup=0;recovery=0;attackClock=1;
            robotState=0;robotClock=1;robotHit=false;
            if(warning!=null)warning.gameObject.SetActive(false);
            visual.GetComponent<OfficeBossRobot>()?.Swing(0);
        }
        private void OnDisable(){CancelBossHook();}
    }
}
