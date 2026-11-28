using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        // Called after furniture, servers, cells and executive attendants are all present.
        private void ResolveStorySpawnOverlaps()
        {
            if(endlessMode||current==null||current.number<2||current.number>5)return;
            Physics.SyncTransforms();
            var placed=new List<Vector3>();
            foreach(var unit in defenders)
            {
                if(unit==null||unit.room!=current.number||!unit.Alive||!unit.gameObject.activeInHierarchy)continue;
                Vector3 desired=unit.transform.position,best=desired;float distance=float.MaxValue;
                if(StorySpawnClear(unit,desired,placed)){placed.Add(desired);continue;}
                for(float z=current.start+3;z<current.end-3;z+=.6f)
                    for(float x=-current.width*.5f+1.2f;x<current.width*.5f-1.2f;x+=.6f)
                    {
                        var candidate=new Vector3(x,desired.y,z);float d=(candidate-desired).sqrMagnitude;
                        if(d>=distance||!StorySpawnClear(unit,candidate,placed))continue;
                        // Use the existing connected walkable grid, so a free spot inside the locked cell isn't chosen.
                        var path=unit.navigation.Path(x,z-current.start,x,z-current.start);
                        var nearest=path[path.Count-1];
                        if(Mathf.Abs(nearest.x-x)>.45f||Mathf.Abs(nearest.z-(z-current.start))>.45f)continue;
                        best=candidate;distance=d;
                    }
                if(distance<float.MaxValue)unit.Teleport(best);
                else Debug.LogWarning("[OfficeSpawn] 빈 배치 위치를 찾지 못했습니다: "+unit.name);
                placed.Add(best);
            }
        }
        private bool StorySpawnClear(OfficeDefender unit,Vector3 at,List<Vector3> placed)
        {
            foreach(var other in placed)if((other-at).sqrMagnitude<1.44f)return false;
            foreach(var hit in Physics.OverlapCapsule(at+Vector3.up*.6f,at+Vector3.up*1.65f,.55f,~0,QueryTriggerInteraction.Ignore))
            {
                if(hit.transform.IsChildOf(unit.transform)||hit.GetComponentInParent<OfficeDefender>()!=null)continue;
                if(hit.bounds.max.y<=at.y+.12f)continue;
                return false;
            }
            return true;
        }
    }
}
