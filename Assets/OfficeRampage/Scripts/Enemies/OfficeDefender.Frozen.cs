using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        public float FrozenLeft { get; private set; }
        private Transform iceShell;
        private Material iceMaterial;
        public void Freeze(float seconds)
        {
            if(!Alive || Dormant) return;
            FrozenLeft=seconds; pendingAttack=-1; recovery=0;
            if(warning!=null) warning.gameObject.SetActive(false);
            if(iceShell!=null) return;
            iceShell=new GameObject("Frozen ice crystals").transform; iceShell.SetParent(transform,false);
            iceMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")); iceMaterial.color=new Color(.35f,.85f,1);
            for(int i=0;i<8;i++)
            {
                var crystal=GameObject.CreatePrimitive(PrimitiveType.Cube); crystal.transform.SetParent(iceShell,false);
                var collider=crystal.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
                float a=i*Mathf.PI/4;
                crystal.transform.localPosition=new Vector3(Mathf.Cos(a)*.45f,.65f,Mathf.Sin(a)*.45f);
                crystal.transform.localScale=new Vector3(.15f,1.3f,.15f); crystal.transform.localRotation=Quaternion.Euler(12,i*45,15);
                crystal.GetComponent<Renderer>().sharedMaterial=iceMaterial;
            }
        }
        private bool TickFrozen(float dt)
        {
            if(FrozenLeft<=0) return false;
            FrozenLeft=Mathf.Max(0,FrozenLeft-dt);
            if(FrozenLeft<=0)
            { if(iceShell!=null) Destroy(iceShell.gameObject); if(iceMaterial!=null) Destroy(iceMaterial); iceShell=null; iceMaterial=null; attackClock=Mathf.Max(attackClock,.6f); }
            return true;
        }
        private void OnDestroy() { if(iceMaterial!=null) Destroy(iceMaterial); }
    }
}
