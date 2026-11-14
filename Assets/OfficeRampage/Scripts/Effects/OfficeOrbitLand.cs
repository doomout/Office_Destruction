using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    // Stylized geographic outlines tessellated onto a sphere, with owned mesh cleanup.
    public sealed class OfficeOrbitLand : MonoBehaviour
    {
        private Mesh mesh;
        private readonly List<Vector3> vertices=new List<Vector3>();
        private readonly List<int> indices=new List<int>();
        private Vector3 center;
        public void Build(Vector2[] outline,Vector3 earth,Material material)
        {
            center=earth;var points=new Vector3[outline.Length];Vector3 middle=Vector3.zero;
            for(int i=0;i<points.Length;i++)
            {
                float lon=outline[i].x*Mathf.Deg2Rad,lat=outline[i].y*Mathf.Deg2Rad;
                points[i]=new Vector3(Mathf.Cos(lat)*Mathf.Cos(lon),Mathf.Sin(lat),Mathf.Cos(lat)*Mathf.Sin(lon));middle+=points[i];
            }
            middle.Normalize();
            for(int i=0;i<points.Length;i++) Triangle(middle,points[i],points[(i+1)%points.Length],3);
            mesh=new Mesh{name="Spherical continent"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        private void Triangle(Vector3 a,Vector3 b,Vector3 c,int depth)
        {
            if(depth>0)
            {
                Vector3 ab=(a+b).normalized,bc=(b+c).normalized,ca=(c+a).normalized;
                Triangle(a,ab,ca,depth-1);Triangle(ab,b,bc,depth-1);Triangle(ca,bc,c,depth-1);Triangle(ab,bc,ca,depth-1);return;
            }
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),a)<0) {var swap=b;b=c;c=swap;}
            int n=vertices.Count;vertices.Add(center+a*180.5f);vertices.Add(center+b*180.5f);vertices.Add(center+c*180.5f);
            indices.Add(n);indices.Add(n+1);indices.Add(n+2);
        }
        private void OnDestroy() {if(mesh!=null) Destroy(mesh);}
    }
}
