using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeEarthSplit:MonoBehaviour
    {
        public float Progress{get;private set;}
        private Transform left,right,scar;
        private Material core;
        private readonly List<Mesh> meshes=new List<Mesh>();
        public void Initialize(Material surface)
        {
            core=new Material(Shader.Find("Universal Render Pipeline/Unlit"));core.color=new Color(1,.22f,.015f);
            left=Half(-1,surface);right=Half(1,surface);
            var beam=GameObject.CreatePrimitive(PrimitiveType.Cylinder);beam.name="Pole to pole fracture";beam.transform.SetParent(transform,false);
            Destroy(beam.GetComponent<Collider>());beam.GetComponent<Renderer>().sharedMaterial=core;scar=beam.transform;scar.localScale=new Vector3(5,195,5);
            SetProgress(0);
        }
        private Transform Half(int side,Material surface)
        {
            var part=new GameObject(side<0?"Left Earth hemisphere":"Right Earth hemisphere").transform;part.SetParent(transform,false);
            const int rows=40,cols=48;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            for(int row=0;row<=rows;row++)for(int col=0;col<=cols;col++)
            {
                float latitude=Mathf.Lerp(-Mathf.PI/2,Mathf.PI/2,(float)row/rows);
                float longitude=Mathf.Lerp(-Mathf.PI/2,Mathf.PI/2,(float)col/cols)+(side<0?Mathf.PI:0);
                vertices.Add(new Vector3(Mathf.Cos(latitude)*Mathf.Cos(longitude),Mathf.Sin(latitude),Mathf.Cos(latitude)*Mathf.Sin(longitude))*180);
                uv.Add(new Vector2(longitude/(2*Mathf.PI)+.5f,(float)row/rows));
                if(row<rows&&col<cols){int a=row*(cols+1)+col,b=a+cols+1;triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            }
            Mesh mesh=new Mesh{name="Hemisphere surface"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;part.gameObject.AddComponent<MeshRenderer>().sharedMaterial=surface;
            var cap=new GameObject("Exposed molten core").transform;cap.SetParent(part,false);
            vertices.Clear();triangles.Clear();vertices.Add(Vector3.zero);
            for(int i=0;i<=96;i++){float angle=i*Mathf.PI*2/96;vertices.Add(new Vector3(0,Mathf.Cos(angle)*179.5f,Mathf.Sin(angle)*179.5f));if(i<96){triangles.AddRange(side>0?new[]{0,i+2,i+1}:new[]{0,i+1,i+2});triangles.AddRange(side>0?new[]{0,i+1,i+2}:new[]{0,i+2,i+1});}}
            var disk=new Mesh{name="Cut face"};disk.SetVertices(vertices);disk.SetTriangles(triangles,0);disk.RecalculateNormals();disk.RecalculateBounds();meshes.Add(disk);
            cap.gameObject.AddComponent<MeshFilter>().sharedMesh=disk;cap.gameObject.AddComponent<MeshRenderer>().sharedMaterial=core;
            return part;
        }
        public void SetProgress(float value)
        {
            Progress=Mathf.Clamp01(value);float k=Mathf.SmoothStep(0,1,Progress);
            left.localPosition=Vector3.left*95*k;right.localPosition=Vector3.right*95*k;
            left.localRotation=Quaternion.Euler(0,-12*k,8*k);right.localRotation=Quaternion.Euler(0,12*k,-8*k);
            scar.localScale=new Vector3(Mathf.Lerp(5,1,k),195+30*k,Mathf.Lerp(5,1,k));
        }
        private void OnDestroy(){foreach(var mesh in meshes)Destroy(mesh);if(core!=null)Destroy(core);}
    }
}
