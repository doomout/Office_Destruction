using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform endlessPortal,endlessDemonLeftArm,endlessDemonRightArm;
        private float endlessPortalAge,endlessDemonRise,endlessDemonWalk;
        private Vector3 endlessDemonSpawn;
        private void ResetEndlessDemon()
        {
            if(endlessDemon!=null){endlessDemon.gameObject.SetActive(false);Destroy(endlessDemon.gameObject);}
            if(endlessPortal!=null){endlessPortal.gameObject.SetActive(false);Destroy(endlessPortal.gameObject);}
            endlessDemon=endlessPortal=endlessDemonLeftArm=endlessDemonRightArm=null;
            endlessDemonLeft=endlessDemonRise=endlessPortalAge=endlessDemonWalk=endlessDemonAttack=0;
        }
        private void BuildHellPortal(Vector3 at)
        {
            endlessPortal=new GameObject("Giant red hell portal").transform;endlessPortal.SetParent(world,false);endlessPortal.position=at;
            var disk=Shape(endlessPortal,"Dark portal opening",new Vector3(0,.04f,0),new Vector3(8,.025f,8),new Color(.055f,0,0),false,PrimitiveType.Cylinder);
            disk.GetComponent<Renderer>().sharedMaterial=EndlessGlow(new Color(.055f,0,0));
            for(int ring=0;ring<3;ring++)
            {
                float radius=3.1f+ring*.5f;var points=new Vector3[65];
                for(int i=0;i<points.Length;i++){float a=i*Mathf.PI/32;points[i]=new Vector3(Mathf.Cos(a)*radius,.09f+ring*.015f,Mathf.Sin(a)*radius);}
                EnergyLine(endlessPortal,"Burning portal ring",points,ring==1?Orange:Color.red,.11f+ring*.025f,true);
            }
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),side=Vector3.Cross(Vector3.up,radial);
                Vector3 center=radial*3.6f+Vector3.up*.13f;
                EnergyLine(endlessPortal,"Portal rune",new[]{center-radial*.2f,center+side*.18f,center+radial*.2f,center-side*.18f,center-radial*.2f},Orange,.07f,true);
            }
            endlessPortal.localScale=Vector3.one*.05f;
        }
        private void SummonEndlessDemon()
        {
            ResetEndlessDemon();endlessDemonLeft=10;endlessDemonRise=1.6f;
            Vector3 at=player.position+avatar.forward*7;
            at.x=Mathf.Clamp(at.x,-26,26);at.z=Mathf.Clamp(at.z,6,58);at.y=.08f;
            endlessDemonSpawn=at;BuildHellPortal(at);
            endlessDemon=new GameObject("Friendly giant demon").transform;endlessDemon.SetParent(world,false);
            endlessDemon.position=at-Vector3.up*7;endlessDemon.localScale=Vector3.one*2.4f;
            var red=new Color(.65f,.025f,.035f);
            Shape(endlessDemon,"Demon torso",Vector3.up*1.35f,new Vector3(.95f,1.1f,.65f),red,false,PrimitiveType.Capsule);
            Shape(endlessDemon,"Demon head",Vector3.up*2.05f,new Vector3(.65f,.6f,.6f),red,false,PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(endlessDemon,"Demon leg",new Vector3(side*.27f,.45f,0),new Vector3(.32f,.85f,.4f),red,false,PrimitiveType.Capsule);
                Shape(endlessDemon,"Demon claw foot",new Vector3(side*.27f,.12f,.15f),new Vector3(.4f,.23f,.65f),new Color(.16f,.01f,.02f),false);
                Shape(endlessDemon,"Demon horn",new Vector3(side*.3f,2.5f,0),new Vector3(.16f,.65f,.16f),Color.yellow,false).transform.localRotation=Quaternion.Euler(0,0,-side*24);
                Shape(endlessDemon,"Demon wing",new Vector3(side*.85f,1.55f,-.25f),new Vector3(.9f,1,.12f),new Color(.25f,.01f,.02f),false).transform.localRotation=Quaternion.Euler(0,side*25,side*25);
                var eye=Shape(endlessDemon,"Demon glowing eye",new Vector3(side*.15f,2.1f,.3f),new Vector3(.14f,.1f,.05f),Color.yellow,false);eye.GetComponent<Renderer>().sharedMaterial=EndlessGlow(Color.yellow);
                var arm=new GameObject("Demon attack arm").transform;arm.SetParent(endlessDemon,false);arm.localPosition=new Vector3(side*.6f,1.6f,0);
                Shape(arm,"Demon forearm",new Vector3(0,-.35f,0),new Vector3(.33f,.8f,.35f),red,false,PrimitiveType.Capsule);
                Shape(arm,"Demon claw",new Vector3(0,-.8f,.08f),new Vector3(.4f,.3f,.45f),Orange,false);
                if(side<0)endlessDemonLeftArm=arm;else endlessDemonRightArm=arm;
            }
            audioBus.Play("thunder",.65f);shake=.8f;
        }
        private void TickEndlessDemon(float dt)
        {
            if(endlessPortal!=null)
            {
                endlessPortalAge+=dt;float size=endlessPortalAge<.6f?Mathf.SmoothStep(.05f,1,endlessPortalAge/.6f):endlessPortalAge>2?Mathf.Clamp01((2.8f-endlessPortalAge)/.8f):1;
                endlessPortal.localScale=new Vector3(size,1,size);endlessPortal.Rotate(0,dt*24,0);
                if(endlessPortalAge>=2.8f){Destroy(endlessPortal.gameObject);endlessPortal=null;}
            }
            if(endlessDemon==null)return;
            if(endlessDemonRise>0)
            {
                endlessDemonRise=Mathf.Max(0,endlessDemonRise-dt);
                endlessDemon.position=endlessDemonSpawn-Vector3.up*7*(1-Mathf.SmoothStep(0,1,1-endlessDemonRise/1.6f));
                if(endlessDemonRise==0){audioBus.Play("explosion",.45f);for(int i=0;i<12;i++)GlowFx("Hell emergence ember",endlessDemonSpawn+Vector3.up*.2f,Vector3.one*.18f,Orange,.8f,new Vector3(Random.Range(-3,3),Random.Range(2,5),Random.Range(-3,3)));}
                return; // The player can keep fighting; the ten support seconds start after emergence.
            }
            endlessDemonLeft=Mathf.Max(0,endlessDemonLeft-dt);
            if(endlessDemonLeft<=0){ResetEndlessDemon();return;}
            OfficeDefender target=null;float distance=24;
            foreach(var enemy in defenders)if(enemy!=null&&enemy.Alive){float d=Vector3.Distance(enemy.transform.position,endlessDemon.position);if(d<distance){target=enemy;distance=d;}}
            Vector3 destination=target!=null?target.transform.position:player.position-avatar.right*5;
            destination.x=Mathf.Clamp(destination.x,-28,28);destination.z=Mathf.Clamp(destination.z,4,60);destination.y=.08f;
            Vector3 delta=destination-endlessDemon.position;delta.y=0;bool moving=delta.magnitude>2.2f;
            if(moving)endlessDemon.position=Vector3.MoveTowards(endlessDemon.position,destination,dt*9);
            if(delta.sqrMagnitude>.01f)endlessDemon.rotation=Quaternion.LookRotation(delta);
            endlessDemonWalk+=dt*(moving?8:2);endlessDemonAttack-=dt;
            float strike=endlessDemonAttack>.2f?Mathf.Sin((.5f-endlessDemonAttack)/.3f*Mathf.PI)*65:0;
            endlessDemonLeftArm.localRotation=Quaternion.Euler(-strike+Mathf.Sin(endlessDemonWalk)*12,0,0);
            endlessDemonRightArm.localRotation=Quaternion.Euler(-strike-Mathf.Sin(endlessDemonWalk)*12,0,0);
            if(target!=null&&Vector3.Distance(target.transform.position,endlessDemon.position)<3&&endlessDemonAttack<=0)
            {
                endlessDemonAttack=.5f;SkillDamage(()=>target.Hit(45));
                EnergyStroke("Demon claw strike",new[]{endlessDemon.position+Vector3.up*3.8f,target.AimPoint},Orange,.2f,.16f);audioBus.Play("swing",.2f);
            }
        }
    }
}
