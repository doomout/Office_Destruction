using UnityEngine;
namespace OfficeRampage
{
    // Repeated hits refresh duration, but never stack DPS or postpone the next tick.
    public sealed class OfficeBurn : MonoBehaviour
    {
        public OfficeRampageGame game;
        private float remaining, tick = 1;
        private Transform flame;
        private Material material, hotMaterial, smokeMaterial;
        private Vector3 fireSize;
        private float visualAge;
        public bool Burning { get { return remaining > 0; } }
        public void Refresh() { remaining = 5; }
        private void Update() { Advance(Time.deltaTime); }
        private void Advance(float dt)
        {
            if (game == null) { Destroy(this); return; }
            if (!game.RunActive || game.CombatLocked) return;
            var enemy = GetComponent<OfficeDefender>();
            var prop = GetComponent<OfficeProp>();
            int room = enemy != null ? enemy.room : prop != null ? prop.room : -1;
            if (!game.IsCurrentRoom(room) || (enemy != null && !enemy.Alive)) { Destroy(this); return; }
            if (flame == null) BuildFlame();
            float elapsed = Mathf.Min(remaining, dt);
            remaining -= elapsed; tick -= elapsed;
            while (tick <= .00001f)
            {
                tick += 1;
                if (enemy != null) enemy.Hit(1);
                else if (prop != null) prop.Hit(1, Vector3.up);
                if (!game.RunActive || game.CombatLocked || !gameObject.activeInHierarchy) break;
            }
            visualAge += dt;
            if (flame != null)
                for (int i=0;i<flame.childCount;i++)
                {
                    var part=flame.GetChild(i);
                    if(part.name=="Smoke")
                    {
                        float rise=Mathf.Repeat(visualAge*.65f+i*.19f,1);
                        part.localPosition=new Vector3(Mathf.Sin(i*3.7f)*fireSize.x*.3f, fireSize.y+rise*2, Mathf.Cos(i*3.7f)*fireSize.z*.3f);
                        part.localScale=Vector3.one*(.25f+rise*.65f);
                    }
                    else if(part.name=="Fire")
                    {
                        float h=fireSize.y*(.7f+.3f*Mathf.Sin(visualAge*17+i*2));
                        part.localScale=new Vector3(.28f,h,.28f); var pos=part.localPosition; pos.y=h*.5f; part.localPosition=pos;
                    }
                }
            if (remaining <= 0) Destroy(this);
        }
        private void BuildFlame()
        {
            flame = new GameObject("Shotgun burn flames").transform;
            flame.SetParent(transform, false);
            Vector3 scale=transform.lossyScale;
            flame.localScale=new Vector3(1/Mathf.Max(.01f,scale.x),1/Mathf.Max(.01f,scale.y),1/Mathf.Max(.01f,scale.z));
            Bounds bounds=new Bounds(transform.position+Vector3.up,Vector3.one);
            bool hasBounds=false;
            foreach(var col in GetComponentsInChildren<Collider>())
                if(col.enabled && !col.isTrigger) { if(!hasBounds) { bounds=col.bounds; hasBounds=true; } else bounds.Encapsulate(col.bounds); }
            bool person=GetComponent<OfficeDefender>()!=null;
            fireSize=person?new Vector3(.65f,1.1f,.65f):new Vector3(Mathf.Clamp(bounds.size.x,.7f,4),Mathf.Clamp(bounds.size.y+1,1.3f,3),Mathf.Clamp(bounds.size.z,.7f,4));
            flame.position=new Vector3(bounds.center.x,person?transform.position.y+.35f:bounds.min.y+.1f,bounds.center.z);
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); material.color = new Color(1,.18f,.01f);
            hotMaterial=new Material(material); hotMaterial.color=new Color(1,.85f,.08f);
            smokeMaterial=new Material(material); smokeMaterial.color=new Color(.25f,.22f,.21f);
            for(int i=0;i<24;i++)
            {
                var part=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var collider=part.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
                part.transform.SetParent(flame,false);
                float angle=i*Mathf.PI*2/18;
                part.name=i<18?"Fire":"Smoke";
                part.transform.localPosition=new Vector3(Mathf.Sin(angle)*fireSize.x*.45f,.5f,Mathf.Cos(angle)*fireSize.z*.45f);
                part.transform.localScale=new Vector3(.28f,fireSize.y,.28f);
                part.GetComponent<Renderer>().sharedMaterial=i>=18?smokeMaterial:i%2==0?material:hotMaterial;
            }
            var glow=new GameObject("Fire glow").AddComponent<Light>(); glow.transform.SetParent(flame,false);
            glow.transform.localPosition=Vector3.up; glow.type=LightType.Point; glow.color=new Color(1,.35f,.04f); glow.range=4; glow.intensity=2;
        }

        private void OnDestroy()
        {
            if (flame != null) Destroy(flame.gameObject);
            if (material != null) Destroy(material);
            if (hotMaterial != null) Destroy(hotMaterial);
            if (smokeMaterial != null) Destroy(smokeMaterial);
        }
    }
}
