using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private sealed class EndlessCar
        {
            public Transform root;public Vector3 direction;public float life=3;
            public HashSet<OfficeDefender> people=new HashSet<OfficeDefender>();public HashSet<OfficeProp> furniture=new HashSet<OfficeProp>();
        }
        private readonly List<EndlessCar> endlessCars=new List<EndlessCar>();
        private void TickEndlessTraffic(float dt)
        {
            if(endlessTrafficLeft>0)
            {
                endlessTrafficLeft=Mathf.Max(0,endlessTrafficLeft-dt);endlessCarClock-=dt;
                if(endlessCarClock<=0&&endlessCars.Count<8){SpawnEndlessCar();endlessCarClock=.75f;}
            }
            for(int i=endlessCars.Count-1;i>=0;i--)
            {
                var car=endlessCars[i];car.life-=dt;
                if(car.root==null||car.life<=0||endlessTrafficLeft<=0){if(car.root!=null)Destroy(car.root.gameObject);endlessCars.RemoveAt(i);continue;}
                Vector3 from=car.root.position;float travel=22*dt;car.root.position+=car.direction*travel;
                foreach(var hit in Physics.SphereCastAll(from+Vector3.up*.9f,1.3f,car.direction,travel))
                {
                    var enemy=hit.collider.GetComponentInParent<OfficeDefender>();var prop=hit.collider.GetComponentInParent<OfficeProp>();
                    if(enemy!=null&&enemy.Alive&&car.people.Add(enemy))SkillDamage(()=>enemy.Hit(90));
                    if(prop!=null&&car.furniture.Add(prop))SkillDamage(()=>prop.Hit(120,car.direction));
                }
                if(Mathf.Abs(car.root.position.x)>34||car.root.position.z < -2||car.root.position.z>66)car.life=0;
            }
        }
        private void SpawnEndlessCar()
        {
            Vector3 target=player.position;var alive=defenders.FindAll(u=>u!=null&&u.Alive);if(alive.Count>0)target=alive[Random.Range(0,alive.Count)].transform.position;
            float a=Random.value*Mathf.PI*2;Vector3 start=target+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*25;
            start.x=Mathf.Clamp(start.x,-29,29);start.z=Mathf.Clamp(start.z,3,61);start.y=.1f;
            Vector3 direction=target-start;direction.y=0;if(direction.sqrMagnitude<1)direction=Vector3.forward;direction.Normalize();
            var car=new EndlessCar{root=new GameObject("Friendly traffic car").transform,direction=direction};car.root.SetParent(world,false);car.root.position=start;car.root.rotation=Quaternion.LookRotation(direction);
            int model=Random.Range(0,3);Color color=new[]{Color.red,Color.yellow,Color.cyan,Color.white,new Color(.2f,.2f,.3f)}[Random.Range(0,5)];
            Shape(car.root,"Car chassis",Vector3.up*.7f,new Vector3(1.8f,.65f,3.4f),color,false);
            Shape(car.root,model==0?"Sedan cabin":model==1?"Van cabin":"Pickup cabin",new Vector3(0,1.25f,model==2?.6f:-.2f),new Vector3(1.55f,model==1?1.2f:.7f,model==1?2.7f:1.5f),color,false);
            Shape(car.root,"Windshield",new Vector3(0,1.35f,model==2?1.4f:.6f),new Vector3(1.35f,.45f,.05f),Navy,false);
            for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2)
            {
                Shape(car.root,"Wheel",new Vector3(side*.91f,.4f,end*1.05f),new Vector3(.6f,.18f,.6f),Navy,false,PrimitiveType.Cylinder).transform.localRotation=Quaternion.Euler(0,0,90);
                if(end>0)Shape(car.root,"Headlight",new Vector3(side*.6f,.8f,1.72f),new Vector3(.35f,.2f,.05f),Color.yellow,false);
            }
            endlessCars.Add(car);audioBus.Play("swing",.25f,.6f,start);
        }
    }
}
