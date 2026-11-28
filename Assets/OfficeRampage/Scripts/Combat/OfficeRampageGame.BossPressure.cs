using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void ClearBossEntrance()
        {
            if(bossUnit==null||endlessMode)return;
            Physics.SyncTransforms();
            Vector3 from=bossUnit.transform.position;
            Vector3 forward=bossUnit.transform.forward;
            var corridor=new Bounds(from+forward*3+Vector3.up, new Vector3(7,3,10));
            // Remove only props in the CEO's immediate fighting space, not the secretary room or walls.
            foreach(var prop in world.GetComponentsInChildren<OfficeProp>())
            {
                if(prop.room!=OfficeStoryFloors.Boss)continue;
                bool blocks=false;
                foreach(var hit in prop.GetComponentsInChildren<Collider>())if(corridor.Intersects(hit.bounds)){blocks=true;break;}
                if(!blocks)continue;
                Impact(prop.transform.position+Vector3.up,Orange,Vector3.up,5);
                prop.gameObject.SetActive(false);Destroy(prop.gameObject);
            }
            Physics.SyncTransforms();
        }
        public Transform CreateBossHookVisual()
        {
            var root=new GameObject("CEO capture cable").transform;root.SetParent(world,false);
            Shape(root,"Cable",Vector3.zero,new Vector3(.10f,.10f,1),Orange,false);
            Shape(root,"Hook",Vector3.zero,Vector3.one*.45f,Cyan,false,PrimitiveType.Sphere);
            audioBus.EnemyAttack("swing",bossUnit.AimPoint);
            return root;
        }
        public void UpdateBossHookVisual(Transform root,Vector3 from,Vector3 to)
        {
            var cable=root.GetChild(0);Vector3 delta=to-from;
            cable.position=(from+to)*.5f;
            if(delta.sqrMagnitude>.0001f)cable.rotation=Quaternion.LookRotation(delta);
            cable.localScale=new Vector3(.10f,.10f,Mathf.Max(.01f,delta.magnitude));
            root.GetChild(1).position=to;
        }
        public bool PullPlayerTowardBoss(Vector3 destination,float dt)
        {
            if(!RunActive||CombatLocked||!controller.enabled)return false;
            Vector3 delta=destination-player.position;delta.y=0;
            Vector3 before=player.position;
            controller.Move(Vector3.ClampMagnitude(delta,19*dt));
            // Let collision stop the pull. Never teleport through a wall or desk.
            return (player.position-before).sqrMagnitude>.000001f;
        }
        private void ClearBossCombatForCinematic()
        {
            foreach(var unit in defenders)if(unit!=null)unit.CancelCombatForCinematic();
            foreach(var projectile in world.GetComponentsInChildren<OfficeProjectile>())
            {projectile.gameObject.SetActive(false);Destroy(projectile.gameObject);}
            while(combatEffects.Count>0)
            {var fx=combatEffects.Dequeue();if(fx!=null){fx.SetActive(false);Destroy(fx);}}
        }
    }
}
