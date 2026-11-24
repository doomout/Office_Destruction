using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private const int EarthquakeWeaponId=20;
        private Transform earthquakeRoot,earthquakeVolcano,earthquakeLava;
        private Mesh earthquakeMesh;
        private float earthquakeLeft,earthquakeAge;
        private bool earthquakeErupted;
        private readonly List<LineRenderer> earthquakeCracks=new List<LineRenderer>();
        private readonly List<Transform> earthquakeEmbers=new List<Transform>(),earthquakeSmoke=new List<Transform>();

        private void TickEarthquakeSupply(float dt)
        {
            if(!endlessMode||!RunActive||CombatLocked||paused||dt<=0)return;
            earthquakeSupplyClock=Mathf.Max(0,earthquakeSupplyClock-dt);
            if(earthquakeSupplyClock>0||EndlessUltimateActive)return;
            var previous=endlessPickups.Count>0?endlessPickups[endlessPickups.Count-1]:null;
            SpawnEndlessPickup(0,EarthquakeWeaponId,NearbyEndlessDrop());
            // A full pickup pool retries rather than losing the scheduled reward.
            var latest=endlessPickups.Count>0?endlessPickups[endlessPickups.Count-1]:null;
            if(latest!=null&&latest!=previous&&latest.kind==0&&latest.weaponId==EarthquakeWeaponId)earthquakeSupplyClock=30;
        }

        private void BuildEarthquakeRemote()
        {
            Shape(weapon,"Seismic controller",new Vector3(0,.12f,.45f),new Vector3(.65f,.23f,.8f),Navy,false);
            Shape(weapon,"Seismic trigger",new Vector3(0,.29f,.42f),new Vector3(.22f,.1f,.22f),Color.red,false,PrimitiveType.Cylinder);
            EnergyLine(weapon,"Seismograph",new[]{new Vector3(-.25f,.25f,.65f),new Vector3(-.1f,.25f,.65f),new Vector3(0,.25f,.8f),new Vector3(.08f,.25f,.55f),new Vector3(.17f,.25f,.65f),new Vector3(.27f,.25f,.65f)},Orange,.035f,true);
        }

        private void StartEarthquake()
        {
            if(!endlessMode||!RunActive||CombatLocked||EndlessUltimateActive)return;
            ResetEarthquake();earthquakeAge=0;earthquakeLeft=8;
            earthquakeRoot=new GameObject("Endless earthquake and volcano").transform;
            earthquakeRoot.SetParent(world,false);earthquakeRoot.position=new Vector3(0,0,32);
            // Fixed-size geometry: no colliders or growing per-frame object allocations.
            for(int ray=0;ray<12;ray++)
            {
                var points=new Vector3[9];float angle=ray*Mathf.PI/6;
                for(int i=0;i<points.Length;i++)
                {
                    float radius=i*4.1f,a=angle+(i%2==0?.045f:-.045f);
                    points[i]=new Vector3(Mathf.Cos(a)*radius,.035f,Mathf.Sin(a)*radius);
                }
                var dark=EnergyLine(earthquakeRoot,"Split ground fissure",points,new Color(.025f,.012f,.008f),.02f,true);
                // View-aligned ribbons remain visible from the overview camera.
                dark.alignment=LineAlignment.View;earthquakeCracks.Add(dark);
                for(int i=0;i<points.Length;i++)points[i].y=.065f;
                earthquakeCracks.Add(EnergyLine(earthquakeRoot,"Molten fissure",points,new Color(1,.19f,.015f),.01f,true));
            }
            BuildEarthquakeVolcano();
            for(int i=0;i<36;i++)
            {
                var ember=Shape(earthquakeRoot,"Volcanic lava bomb",Vector3.zero,Vector3.one*.8f,Orange,false,PrimitiveType.Sphere).transform;
                ember.GetComponent<Renderer>().sharedMaterial=EndlessGlow(i%3==0?Color.yellow:Orange);ember.gameObject.SetActive(false);earthquakeEmbers.Add(ember);
            }
            for(int i=0;i<10;i++)
            {
                var smoke=Shape(earthquakeRoot,"Volcanic ash plume",Vector3.zero,Vector3.one,new Color(.16f,.13f,.12f),false,PrimitiveType.Sphere).transform;
                smoke.gameObject.SetActive(false);earthquakeSmoke.Add(smoke);
            }
            audioBus.Play("thunder",.45f,.6f);invulnerability=Mathf.Max(invulnerability,.4f);
        }

        private void BuildEarthquakeVolcano()
        {
            const int count=48;var vertices=new Vector3[count*3];var triangles=new int[count*12];
            for(int i=0;i<count;i++)
            {
                float angle=i*Mathf.PI*2/count;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                vertices[i*3]=radial*(11+Mathf.Sin(i*1.7f)*.7f);
                vertices[i*3+1]=radial*3.5f+Vector3.up*(11+Mathf.Sin(i*1.3f)*.45f);
                vertices[i*3+2]=radial*2.6f+Vector3.up*9.5f;
                int a=i*3,b=((i+1)%count)*3,k=i*12;
                triangles[k]=a;triangles[k+1]=a+1;triangles[k+2]=b;
                triangles[k+3]=b;triangles[k+4]=a+1;triangles[k+5]=b+1;
                triangles[k+6]=a+1;triangles[k+7]=a+2;triangles[k+8]=b+1;
                triangles[k+9]=b+1;triangles[k+10]=a+2;triangles[k+11]=b+2;
            }
            earthquakeMesh=new Mesh{name="Volcano crater mesh"};earthquakeMesh.vertices=vertices;earthquakeMesh.triangles=triangles;earthquakeMesh.RecalculateNormals();earthquakeMesh.RecalculateBounds();
            earthquakeVolcano=new GameObject("Central rising volcano").transform;earthquakeVolcano.SetParent(earthquakeRoot,false);
            earthquakeVolcano.gameObject.AddComponent<MeshFilter>().sharedMesh=earthquakeMesh;
            earthquakeVolcano.gameObject.AddComponent<MeshRenderer>().sharedMaterial=Mat(new Color(.21f,.12f,.08f));
            earthquakeVolcano.localScale=new Vector3(1,.01f,1);
            earthquakeLava=Shape(earthquakeVolcano,"Glowing lava crater",new Vector3(0,9.6f,0),new Vector3(5.4f,.5f,5.4f),Orange,false,PrimitiveType.Sphere).transform;
            earthquakeLava.GetComponent<Renderer>().sharedMaterial=EndlessGlow(new Color(1,.38f,.025f));
        }

        private void TickEarthquake(float dt)
        {
            if(earthquakeLeft<=0||earthquakeRoot==null||!endlessMode||!RunActive||CombatLocked||paused||dt<=0)return;
            earthquakeAge+=dt;earthquakeLeft=Mathf.Max(0,8-earthquakeAge);
            invulnerability=Mathf.Max(invulnerability,.4f);
            foreach(var enemy in defenders)if(enemy!=null&&enemy.Alive)enemy.Stun(.3f);
            float split=Mathf.SmoothStep(0,1,earthquakeAge/2);
            for(int i=0;i<earthquakeCracks.Count;i++)earthquakeCracks[i].widthMultiplier=split*(i%2==0?2.2f:.7f);
            float rise=Mathf.SmoothStep(0,1,(earthquakeAge-1.5f)/2.5f);
            earthquakeVolcano.localScale=new Vector3(1,Mathf.Max(.01f,rise),1);
            if(earthquakeAge>=4&&!earthquakeErupted)
            {
                earthquakeErupted=true;WipeEndlessEnemiesAndProps();flash=.2f;
                foreach(var p in endlessPickups)if(p!=null){p.gameObject.SetActive(false);Destroy(p.gameObject);}
                endlessPickups.Clear();nearbySupply=null;
                audioBus.Play("explosion",.8f,.7f);
            }
            if(earthquakeErupted)
            {
                float age=earthquakeAge-4;
                for(int i=0;i<earthquakeEmbers.Count;i++)
                {
                    var ember=earthquakeEmbers[i];ember.gameObject.SetActive(true);
                    float t=Mathf.Repeat(age+i*.073f,2.8f),angle=i*2.39996f,r=t*(4+i%5);
                    ember.localPosition=new Vector3(Mathf.Cos(angle)*r,10+19*t-8*t*t,Mathf.Sin(angle)*r);
                    ember.localScale=Vector3.one*(.7f+(i%3)*.35f);ember.Rotate(dt*110,dt*75,0);
                }
                for(int i=0;i<earthquakeSmoke.Count;i++)
                {
                    var smoke=earthquakeSmoke[i];smoke.gameObject.SetActive(true);float t=Mathf.Repeat(age+i*.3f,4);
                    smoke.localPosition=new Vector3(Mathf.Sin(i*2)*t*1.4f,12+t*5,Mathf.Cos(i*2)*t*1.4f);
                    smoke.localScale=Vector3.one*(3+t*2);
                }
            }
            if(earthquakeLeft<=0)
            {
                ResetEarthquake();ChangeEndlessEnvironment();ResumeAfterSupportUltimate();
            }
        }
        private void UpdateEarthquakeCamera()
        {
            float strength=earthquakeAge<4?.22f:.4f;
            view.fieldOfView=65;view.transform.position=new Vector3(Mathf.Sin(earthquakeAge*39)*strength,38+Mathf.Sin(earthquakeAge*47)*strength,-34);
            view.transform.LookAt(new Vector3(0,7,32));
        }
        private void ResetEarthquake()
        {
            if(earthquakeRoot!=null){earthquakeRoot.gameObject.SetActive(false);Destroy(earthquakeRoot.gameObject);}
            if(earthquakeMesh!=null)Destroy(earthquakeMesh);
            earthquakeRoot=earthquakeVolcano=earthquakeLava=null;earthquakeMesh=null;
            earthquakeCracks.Clear();earthquakeEmbers.Clear();earthquakeSmoke.Clear();earthquakeAge=earthquakeLeft=0;earthquakeErupted=false;
        }
    }
}
