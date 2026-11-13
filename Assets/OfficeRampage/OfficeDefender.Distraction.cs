using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        private Transform heart;
        private bool distracted;
        private float distractionClock, routeClock, panicOffset;
        private Vector3 roamTarget;
        private bool TickDistraction(float dt)
        {
            bool attracted=!dog && !female && !boss && companyRank!=OfficeCompanySettings.RepresentativeRank && game.TryGetLure(room,out _);
            var burn=GetComponent<OfficeBurn>(); bool panic=!dog && burn!=null && burn.Burning;
            if(!attracted && !panic)
            {
                if(distracted) { path=null; chooseTime=0; attackClock=Mathf.Max(attackClock,.7f); ResetDistractionPose(); }
                distracted=false; if(heart!=null) heart.gameObject.SetActive(false); return false;
            }
            if(!distracted) { routeClock=0; panicOffset=(float)random.NextDouble()*6; pendingAttack=-1; recovery=0; if(warning!=null) warning.gameObject.SetActive(false); }
            distracted=true; distractionClock+=dt;
            if(attracted)
            {
                game.TryGetLure(room,out Vector3 center);
                if(heart==null) heart=game.CreateHeartMarker(transform);
                heart.gameObject.SetActive(true); heart.localPosition=Vector3.up*(2.45f+Mathf.Sin(distractionClock*5)*.12f);
                if(Camera.main!=null) heart.rotation=Camera.main.transform.rotation;
                routeClock-=dt;
                if(routeClock<=0 || path==null)
                {
                    float angle=(float)random.NextDouble()*Mathf.PI*2;
                    float radius=1.1f+(float)random.NextDouble()*1.6f;
                    roamTarget=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;
                    path=navigation.Path(transform.position.x,transform.position.z-roomStart,roamTarget.x,roamTarget.z-roomStart);
                    waypoint=0; routeClock=1.4f;
                }
                if(waypoint<path.Count)
                {
                    Vector3 next=new Vector3(path[waypoint].x,transform.position.y,roomStart+path[waypoint].z)-transform.position;
                    if(next.magnitude<.2f) waypoint++;
                    else Move(next.normalized*Mathf.Min(4.5f,next.magnitude/Mathf.Max(dt,.001f)),dt);
                }
            }
            else
            {
                if(heart!=null) heart.gameObject.SetActive(false);
                // Short, alternating steps show panic without letting enemies leave the office.
                float phase=distractionClock*7+panicOffset;
                Move(new Vector3(Mathf.Sin(phase),0,Mathf.Cos(phase*1.3f))*1.4f,dt);
            }
            if(visual!=null)
            {
                visual.localRotation=Quaternion.Euler(0,0,Mathf.Sin(distractionClock*(panic?18:5))*(panic?12:5));
                foreach(Transform part in visual)
                    if(part.name=="Sleeve") part.localRotation=Quaternion.Euler(-100+Mathf.Sin(distractionClock*15+part.localPosition.x)*30,0,0);
            }
            return true;
        }
        private void ResetDistractionPose()
        {
            if(visual==null) return;
            visual.localRotation=Quaternion.identity;
            foreach(Transform part in visual) if(part.name=="Sleeve") part.localRotation=Quaternion.identity;
        }
    }
}
