using System.Linq;
using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeFloorPrefab:MonoBehaviour
    {
        public string floorName;
        public Transform floorSurface,playerSpawn,bossSpawn,secretarySpawn,secretaryDoor;
        public Transform[] enemySpawns,guardSpawns;
        [HideInInspector] public Transform[] partitions;
        public OfficeSlidingDoor elevator,dogDoor,guardDoor;
        public Transform[] previewRoots;
        public float Width=>floorSurface.localScale.x;
        public float Length=>floorSurface.localScale.z;
        public Vector3 Local(Transform t)=>transform.InverseTransformPoint(t.position);
        public OfficeFixedFloor ReadLayout()
        {
            var result=new OfficeFixedFloor{width=Width,length=Length,name=floorName,oil=new OfficeFixedFloor.Spot[0],routes=new OfficeFixedFloor.Aisle[0]};
            result.furniture=GetComponentsInChildren<OfficeProp>().Select(p=>{var b=Footprint(p.transform);var at=Local(p.transform);return new OfficeFixedFloor.Spot{x=at.x,z=at.z,kind=p.kind,yaw=p.transform.eulerAngles.y,footprintX=b.center.x,footprintZ=b.center.z,footprintWidth=b.size.x,footprintDepth=b.size.z};}).ToArray();
            // Derive navigation blockers from the saved geometry, including newly duplicated walls.
            result.walls=GetComponentsInChildren<Collider>().Where(c=>c.enabled&&!c.isTrigger&&c.GetComponentInParent<OfficeProp>(true)==null&&c.GetComponentInParent<OfficeSlidingDoor>(true)==null)
                .Select(c=>Footprint(c.transform)).Where(b=>b.min.y<2&&b.max.y>.3f)
                .Select(b=>new OfficeFixedFloor.Aisle{x=b.center.x,z=b.center.z,width=b.size.x,depth=b.size.z}).ToArray();
            result.enemies=enemySpawns.Where(p=>p!=null).Select(Spot).ToArray();
            result.potions=GetComponentsInChildren<OfficeHealingPotion>().Select(p=>Spot(p.transform)).ToArray();
            return result;
        }
        private OfficeFixedFloor.Spot Spot(Transform t){var p=Local(t);return new OfficeFixedFloor.Spot{x=p.x,z=p.z,yaw=t.eulerAngles.y};}
        private Bounds Footprint(Transform owner)
        {
            var result=new Bounds(Local(owner),Vector3.zero);bool first=true;
            foreach(var c in owner.GetComponentsInChildren<Collider>())
            {
                if(!c.enabled || c.isTrigger)continue;
                Bounds b;
                if(c is BoxCollider box)b=new Bounds(box.center,box.size);
                else if(c is SphereCollider sphere)b=new Bounds(sphere.center,Vector3.one*sphere.radius*2);
                else if(c is CapsuleCollider capsule){var size=Vector3.one*capsule.radius*2;size[capsule.direction]=capsule.height;b=new Bounds(capsule.center,size);}
                else {var mesh=c.GetComponent<MeshFilter>();if(mesh==null||mesh.sharedMesh==null)continue;b=mesh.sharedMesh.bounds;}
                for(int i=0;i<8;i++){var corner=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var p=transform.InverseTransformPoint(c.transform.TransformPoint(corner));if(first){result=new Bounds(p,Vector3.zero);first=false;}else result.Encapsulate(p);}
            }
            return result;
        }
    }
}
