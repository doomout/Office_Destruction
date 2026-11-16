using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        public void FireRobotPlasma(OfficeDefender unit,Vector3 forward)
        {
            Vector3 origin=unit.AimPoint+forward*1.7f;
            // Lock horizontal aim during the telegraph; the projectiles remain dodgeable.
            float distance=Mathf.Max(1,Vector3.ProjectOnPlane(PlayerPosition-origin,Vector3.up).magnitude);
            Vector3 direction=(forward+Vector3.up*((PlayerPosition.y+1-origin.y)/distance)).normalized;
            for(int i=-1;i<=1;i++)
            {
                Vector3 ray=Quaternion.Euler(0,i*7,0)*direction;
                var obj=Shape(world,"Robot plasma bolt",origin,new Vector3(.5f,.5f,1.5f),Cyan,false,PrimitiveType.Sphere);
                obj.transform.rotation=Quaternion.LookRotation(ray);
                var light=obj.AddComponent<Light>();light.color=Cyan;light.intensity=2;light.range=4;
                var shot=obj.AddComponent<OfficeProjectile>();shot.game=this;shot.owner=unit.transform;shot.direction=ray;shot.speed=36;shot.range=70;shot.damage=24;shot.radius=.3f;
            }
            audioBus.Play("orbitalLaser",.35f,1.5f,origin);
        }
    }
}
