using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeBossRobot : MonoBehaviour
    {
        private OfficeRampageGame game;private OfficeDefender unit;private Vector3 previous;private float phase;
        public void Initialize(OfficeRampageGame owner,OfficeDefender boss) {game=owner;unit=boss;previous=transform.position;}
        private void LateUpdate()
        {
            if(game==null || !game.RunActive || game.CombatLocked || !unit.Alive || unit.FrozenLeft>0 || unit.StunLeft>0) {previous=transform.position;return;}
            float distance=Vector3.Distance(previous,transform.position);previous=transform.position;phase+=distance*2;
            foreach(Transform child in transform)
            {
                float side=child.name.Contains("left")?-1:1;
                if(child.name.Contains("leg")) child.localRotation=Quaternion.Euler(Mathf.Sin(phase)*side*(distance>.001f?22:0),0,0);
                if(child.name.Contains("arm")) child.localRotation=Quaternion.Euler(-12-Mathf.Sin(phase)*side*12,0,0);
            }
        }
    }
}
