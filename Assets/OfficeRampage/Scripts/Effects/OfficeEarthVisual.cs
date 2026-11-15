using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeEarthVisual : MonoBehaviour
    {
        private Material surface,atmosphere;
        private LineRenderer rim;
        public void Initialize()
        {
            surface=new Material(Shader.Find("Universal Render Pipeline/Lit"));surface.color=Color.white;
            surface.SetTexture("_BaseMap",Resources.Load<Texture2D>("OfficeSpace/earth_daymap"));surface.SetFloat("_Smoothness",.1f);
            surface.EnableKeyword("_EMISSION");surface.SetTexture("_EmissionMap",surface.GetTexture("_BaseMap"));surface.SetColor("_EmissionColor",Color.white*.2f);
            GetComponent<Renderer>().sharedMaterial=surface;transform.localRotation=Quaternion.Euler(0,120,0);
            rim=new GameObject("Blue atmospheric rim").AddComponent<LineRenderer>();rim.transform.SetParent(transform,false);
            atmosphere=new Material(Shader.Find("Universal Render Pipeline/Unlit"));atmosphere.color=new Color(.18f,.55f,.9f);
            rim.sharedMaterial=atmosphere;rim.useWorldSpace=true;rim.positionCount=128;rim.loop=true;rim.widthMultiplier=1.3f;
        }
        private void LateUpdate()
        {
            if(rim==null || Camera.main==null) return;
            Vector3 normal=(Camera.main.transform.position-transform.position).normalized;
            Vector3 right=Vector3.Cross(normal,Vector3.up).normalized,up=Vector3.Cross(right,normal);
            for(int i=0;i<128;i++) {float a=i*Mathf.PI*2/128;rim.SetPosition(i,transform.position+(right*Mathf.Cos(a)+up*Mathf.Sin(a))*181);}
        }
        private void OnDestroy() {if(surface!=null) Destroy(surface);if(atmosphere!=null) Destroy(atmosphere);}
    }
}
