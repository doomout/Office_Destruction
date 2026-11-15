using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeBossRobot : MonoBehaviour
    {
        private OfficeRampageGame game;private OfficeDefender unit;private Vector3 previous;private float phase;
        private Transform sword;private Material bladeMaterial;
        public void Initialize(OfficeRampageGame owner,OfficeDefender boss)
        {
            game=owner;unit=boss;previous=transform.position;transform.localRotation=Quaternion.identity;
            sword=new GameObject("Giant energy sword").transform;sword.SetParent(transform.Find("Robot right arm"),false);
            sword.localPosition=new Vector3(0,-1.8f,.8f);
            bladeMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));bladeMaterial.color=Color.cyan;
            bladeMaterial.EnableKeyword("_EMISSION");bladeMaterial.SetColor("_EmissionColor",new Color(0,3,5));
            var blade=GameObject.CreatePrimitive(PrimitiveType.Cube);blade.name="Energy blade";blade.transform.SetParent(sword,false);
            blade.transform.localPosition=new Vector3(0,0,3.1f);blade.transform.localScale=new Vector3(.32f,.65f,6.2f);
            blade.GetComponent<Collider>().enabled=false;Destroy(blade.GetComponent<Collider>());blade.GetComponent<Renderer>().sharedMaterial=bladeMaterial;
            var light=blade.AddComponent<Light>();light.color=Color.cyan;light.range=9;light.intensity=3;
            Swing(0);
        }
        public void Swing(float progress)
        {
            if(sword==null) return;
            float angle=progress<0?Mathf.Lerp(0,-65,-progress):progress>0?Mathf.Lerp(-65,65,progress):0;
            sword.localRotation=Quaternion.Euler(0,angle,0);
        }
        private void OnDestroy() {if(bladeMaterial!=null) Destroy(bladeMaterial);}
        private void LateUpdate()
        {
            if(game==null || !game.RunActive || game.CombatLocked || !unit.Alive || unit.FrozenLeft>0 || unit.StunLeft>0) {previous=transform.position;return;}
            float distance=Vector3.Distance(previous,transform.position);previous=transform.position;phase+=distance*2;
            foreach(Transform child in transform)
            {
                float side=child.name.Contains("left")?-1:1;
                if(child.name.Contains("leg")) child.localRotation=Quaternion.Euler(Mathf.Sin(phase)*side*(distance>.001f?22:0),0,0);
                if(child.name.Contains("arm")) child.localRotation=child.name.Contains("right")?Quaternion.identity:Quaternion.Euler(-12-Mathf.Sin(phase)*side*12,0,0);
            }
        }
    }
}
