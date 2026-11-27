using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private IEnumerator ReleasePaperBurst(OfficeDefender unit,Vector3 direction,float range)
        {
            unit.paperBurstActive=true;
            // Lock the warned direction for the entire volley, allowing a sideways dodge.
            direction=direction.sqrMagnitude>.001f?direction.normalized:unit.transform.forward;
            Vector3 origin=unit.AimPoint+direction*.5f;
            try
            {
                for(int sheet=0;sheet<5;sheet++)
                {
                    float wait=sheet==0?0:.3f;
                    while(true)
                    {
                        if(unit==null||!unit.gameObject.activeInHierarchy||!unit.Alive||unit.Dormant||unit.PaperBurstInterrupted||!IsCurrentRoom(unit.room)||ended||menuPage!=MenuPage.Game||CombatLocked||unit.FrozenLeft>0||unit.StunLeft>0)yield break;
                        if(RunActive&&wait<=0)break;
                        yield return null;
                        if(RunActive)wait-=Time.deltaTime;
                    }
                    SpawnProjectile(origin,direction,unit.transform,false,false,14,range,unit.boss?18:10,unit.RangeScale);
                    unit.visual.GetComponent<OfficeCharacterVisual>()?.PlayAction("holding-right-shoot");
                    audioBus.EnemyAttack("paper",origin,.18f);
                }
            }
            finally{if(unit!=null)unit.paperBurstActive=false;}
        }
    }
}
