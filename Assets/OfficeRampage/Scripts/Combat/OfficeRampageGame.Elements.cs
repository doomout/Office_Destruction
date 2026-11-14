using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private int skillDamageDepth, normalStrikeWeapon=-1;
        private float thunderReady, thunderLeft, iceReady, iceLeft;
        public void SkillDamage(System.Action apply)
        { skillDamageDepth++; try { apply(); } finally { skillDamageDepth--; } }
        private void ResetElements()
        {
            thunderReady=thunderLeft=iceReady=iceLeft=0; normalStrikeWeapon=-1;
            if(weapon!=null) weapon.localScale=Vector3.one;
        }
        private void ChargeElements()
        {
            if(combo%3!=0 || skillDamageDepth>0) return;
            if(normalStrikeWeapon==1 && thunderReady<=0 && thunderLeft<=0)
            { thunderReady=10; Notify("[F] 토르의 망치 · 직선 번개 1회",3); }
            if(normalStrikeWeapon==2 && iceReady<=0 && iceLeft<=0)
            { iceReady=10; Notify("[F] 얼음 총알 · 10초 동안 사용",3); }
        }
        private void ActivateWeaponSkill()
        {
            if(!RunActive || CombatLocked) return;
            if(weaponIndex==0) { StartWhirlwind(); return; }
            if(weaponIndex==1 && thunderReady>0 && thunderLeft<=0)
            {
                thunderReady=0; thunderLeft=1.2f; combo=0; comboLeft=0;
                weapon.localScale=Vector3.one*10;
                FireThunder();
            }
            else if(weaponIndex==2 && iceReady>0 && iceLeft<=0)
            { iceReady=0; iceLeft=10; combo=0; comboLeft=0; audioBus.Play("combo",.6f); }
        }
        private void TickElements(float dt)
        {
            thunderReady=Mathf.Max(0,thunderReady-dt); iceReady=Mathf.Max(0,iceReady-dt);
            iceLeft=Mathf.Max(0,iceLeft-dt);
            if(thunderLeft>0)
            { thunderLeft=Mathf.Max(0,thunderLeft-dt); if(thunderLeft<=0) weapon.localScale=Vector3.one; }
            if(iceLeft>0 && weaponIndex==2)
                Fx("Ice muzzle frost",weapon.position+avatar.forward*.6f,Vector3.one*.08f,Cyan,.2f,Vector3.up*.3f);
        }
        private void FireThunder()
        {
            Vector3 origin=player.position+Vector3.up*1.2f, direction=avatar.forward; direction.y=0; direction.Normalize();
            float length=30;
            // Stop at solid scenery; destructible objects and enemies are pierced by the bolt.
            foreach(var hit in Physics.RaycastAll(origin,direction,length))
                if(!IsPlayerCollider(hit.collider) && hit.collider.GetComponentInParent<OfficeProp>()==null && hit.collider.GetComponentInParent<OfficeDefender>()==null)
                    length=Mathf.Min(length,hit.distance);
            SkillDamage(()=> {
                foreach(var prop in world.GetComponentsInChildren<OfficeProp>())
                    if(prop.room==current.number && InThunderLine(origin,direction,prop.transform.position+Vector3.up,length)) prop.Hit(180,direction);
                foreach(var enemy in defenders)
                    if(enemy!=null && enemy.Alive && !enemy.Dormant && enemy.room==current.number && InThunderLine(origin,direction,enemy.AimPoint,length)) enemy.Hit(180);
            });
            for(int strand=0;strand<3;strand++)
            {
                Vector3 previous=origin;
                for(float z=1;z<=length;z+=1)
                {
                    Vector3 next=origin+direction*z+Random.insideUnitSphere*.35f;
                    var bolt=Fx("Thor lightning",(previous+next)/2,new Vector3(.10f,.10f,Vector3.Distance(previous,next)),strand==0?Color.white:Cyan,.65f,Vector3.zero);
                    bolt.transform.rotation=Quaternion.LookRotation(next-previous); previous=next;
                }
            }
            shake=2; swing=1; audioBus.Play("explosion",.7f);
        }
        private static bool InThunderLine(Vector3 origin,Vector3 direction,Vector3 target,float length)
        {
            var delta=target-origin; float along=Vector3.Dot(delta,direction);
            return along>=0 && along<=length && (delta-direction*along).sqrMagnitude<=2.25f;
        }
        private void DrawElementSkills()
        {
            string text=thunderLeft>0?"토르의 망치!":iceLeft>0?"얼음 총알 · "+iceLeft.ToString("0.0")+"초":
                weaponIndex==1&&thunderReady>0?"[F] 토르의 망치 · "+thunderReady.ToString("0.0")+"초 안에 발동":
                weaponIndex==2&&iceReady>0?"[F] 얼음 총알 · "+iceReady.ToString("0.0")+"초 안에 발동":"";
            if(text.Length==0) return; EnsureMenuStyles();
            Box(new Rect(340,435,600,42),Navy); GUI.Label(new Rect(355,440,580,36),text,menuText);
        }
    }
}
