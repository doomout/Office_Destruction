using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeSecretaryIdle:MonoBehaviour
    {
        public OfficeRampageGame game;
        private Vector3 home;private float clock;
        private void Start(){home=transform.localPosition;}
        private void Update()
        {
            if(game==null || !game.RunActive || game.CombatLocked)return;
            clock+=Time.deltaTime;
            transform.localPosition=home+new Vector3(Mathf.Sin(clock*35)*.035f,Mathf.Abs(Mathf.Sin(clock*22))*.025f,0);
            transform.localRotation=Quaternion.Euler(0,-90,Mathf.Sin(clock*30)*3);
        }
    }
}
