using UnityEngine;
namespace OfficeRampage
{
    // Red energy ribbons and concentric halos, deliberately distinct from orange burning particles.
    public sealed class OfficeRageAura : MonoBehaviour
    {
        private OfficeRampageGame game;
        private Material material;
        private LineRenderer[] ribbons;
        private float age;
        public void Initialize(OfficeRampageGame owner)
        {
            game=owner;material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor",new Color(1,.015f,.08f,.65f));
            material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",1);material.SetFloat("_ZWrite",0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
            ribbons=new LineRenderer[16];
            for(int i=0;i<ribbons.Length;i++)
            {
                var line=new GameObject(i<12?"Red energy ribbon":"Red aura halo").AddComponent<LineRenderer>();line.transform.SetParent(transform,false);
                line.useWorldSpace=false;line.sharedMaterial=material;line.positionCount=33;line.widthMultiplier=i<12?.12f:.07f;line.loop=i>=12;
                ribbons[i]=line;
            }
            Render();
        }
        private void Update() { if(game!=null && game.RunActive && !game.CombatLocked) { age+=Time.deltaTime;Render(); } }
        private void Render()
        {
            for(int i=0;i<ribbons.Length;i++)
            for(int j=0;j<33;j++)
            {
                float t=j/32f,angle=i<12?i*Mathf.PI/6+age*1.8f+t*1.2f:t*Mathf.PI*2;
                float radius=i<12?.45f+Mathf.Sin(t*Mathf.PI)*(.5f+.07f*Mathf.Sin(age*9+i)):.75f+.12f*Mathf.Sin(age*5+i);
                float y=i<12?t*2.8f:.25f+(i-12)*.65f;
                ribbons[i].SetPosition(j,new Vector3(Mathf.Cos(angle)*radius,y,Mathf.Sin(angle)*radius));
            }
        }
        private void OnDestroy() { if(material!=null) Destroy(material); }
    }
}
