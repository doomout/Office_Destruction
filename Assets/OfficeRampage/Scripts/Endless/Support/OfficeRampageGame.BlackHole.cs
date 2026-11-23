using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Transform blackHoleRoot;
        private Vector3 blackHoleCenter;
        private float blackHoleLeft,blackHoleAge;
        private void ResetBlackHole(){if(blackHoleRoot!=null){blackHoleRoot.gameObject.SetActive(false);Destroy(blackHoleRoot.gameObject);}blackHoleRoot=null;blackHoleLeft=blackHoleAge=0;}
        private void StartBlackHole()
        {
            ResetBlackHole();blackHoleCenter=player.position+avatar.forward*10;blackHoleCenter.x=Mathf.Clamp(blackHoleCenter.x,-22,22);blackHoleCenter.z=Mathf.Clamp(blackHoleCenter.z,10,54);blackHoleCenter.y=3;
            blackHoleRoot=new GameObject("Endless black hole").transform;blackHoleRoot.SetParent(world,false);blackHoleRoot.position=blackHoleCenter;blackHoleLeft=5;
            var core=Shape(blackHoleRoot,"Black hole event horizon",Vector3.zero,Vector3.one*4,Color.black,false,PrimitiveType.Sphere);core.GetComponent<Renderer>().sharedMaterial=EndlessGlow(Color.black);
            for(int ring=0;ring<3;ring++){var points=new Vector3[65];for(int i=0;i<65;i++){float a=i*Mathf.PI/32;points[i]=new Vector3(Mathf.Cos(a)*(2.4f+ring*.55f),Mathf.Sin(a)*.5f*ring,Mathf.Sin(a)*(2.4f+ring*.55f));}EnergyLine(blackHoleRoot,"Gravity accretion ring",points,ring==1?Color.white:Color.magenta,.12f,true);}
            audioBus.Play("orbitalLaser",.45f,.55f);
        }
        private void TickBlackHole(float dt)
        {
            if(blackHoleRoot==null)return;blackHoleLeft=Mathf.Max(0,blackHoleLeft-dt);blackHoleAge+=dt;blackHoleRoot.Rotate(0,dt*100,dt*15);
            float speed=10+blackHoleAge*10;
            foreach(var enemy in defenders.ToArray())if(enemy!=null&&enemy.Alive)
            {
                var at=Vector3.MoveTowards(enemy.transform.position,blackHoleCenter,dt*speed);enemy.Teleport(at);enemy.Stun(.3f);
                if(Vector3.Distance(at,blackHoleCenter)<2)SkillDamage(()=>enemy.Hit(1000000));
            }
            foreach(var prop in endlessRoot.GetComponentsInChildren<OfficeProp>())if(prop!=null&&prop.health>0)
            {
                foreach(var col in prop.GetComponentsInChildren<Collider>())col.enabled=false;
                prop.transform.position=Vector3.MoveTowards(prop.transform.position,blackHoleCenter,dt*speed);prop.transform.Rotate(dt*70,dt*110,0);
                if(Vector3.Distance(prop.transform.position,blackHoleCenter)<2)SkillDamage(()=>prop.Hit(1000000,Vector3.up));
            }
            for(int i=endlessPickups.Count-1;i>=0;i--){var p=endlessPickups[i];if(p==null)continue;p.transform.position=Vector3.MoveTowards(p.transform.position,blackHoleCenter,dt*speed);if(Vector3.Distance(p.transform.position,blackHoleCenter)<2){p.gameObject.SetActive(false);Destroy(p.gameObject);endlessPickups.RemoveAt(i);}}
            if(blackHoleLeft<=0)
            {
                WipeEndlessEnemiesAndProps();foreach(var p in endlessPickups)if(p!=null){p.gameObject.SetActive(false);Destroy(p.gameObject);}endlessPickups.Clear();nearbySupply=null;
                GlowFx("Gravity collapse",blackHoleCenter,Vector3.one*5,Color.magenta,.6f,Vector3.zero,-5);
                ResetBlackHole();ChangeEndlessEnvironment();ResumeAfterSupportUltimate();audioBus.Play("explosion",.65f);
            }
        }
    }
}
