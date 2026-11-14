using UnityEngine;
namespace OfficeRampage
{
    // Actual concave terrain and matching collider, rather than a dark disc on top of a flat floor.
    public sealed class OfficeCraterFloor : MonoBehaviour
    {
        private Mesh mesh;
        public void Build(float width,float start,float end,Vector3 center,float radius,Material material)
        {
            const int nx=100,nz=140;var vertices=new Vector3[(nx+1)*(nz+1)];var triangles=new int[nx*nz*6];int cursor=0;
            for(int z=0;z<=nz;z++) for(int x=0;x<=nx;x++)
            {
                float px=Mathf.Lerp(-width/2,width/2,x/(float)nx),pz=Mathf.Lerp(start-25,end+25,z/(float)nz);
                float d=(new Vector2(px-center.x,pz-center.z)).magnitude;
                float y=d<radius?-Mathf.Sqrt(radius*radius-d*d):0;
                vertices[z*(nx+1)+x]=new Vector3(px,y,pz);
                if(x==nx || z==nz) continue;int a=z*(nx+1)+x,b=a+nx+1;
                triangles[cursor++]=a;triangles[cursor++]=b;triangles[cursor++]=a+1;
                triangles[cursor++]=a+1;triangles[cursor++]=b;triangles[cursor++]=b+1;
            }
            mesh=new Mesh{name="Hemispherical impact terrain"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
            gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        private void OnDestroy() {if(mesh!=null) Destroy(mesh);}
    }
}
