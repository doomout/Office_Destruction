using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private int skillDamageDepth, normalStrikeWeapon=-1;
        private float thunderReady, thunderLeft, iceReady, iceLeft, thunderTick;
        private int thunderPulse;
        public bool IsRaisingHammer(Transform target) { return target==avatar && thunderLeft>0; }
        public void SkillDamage(System.Action apply)
        { skillDamageDepth++; try { apply(); } finally { skillDamageDepth--; } }
        private void ResetElements()
        {
            thunderReady=thunderLeft=iceReady=iceLeft=thunderTick=0; thunderPulse=0; normalStrikeWeapon=-1;
            if(weapon!=null) weapon.localScale=Vector3.one;
        }
        private void ChargeElements()
        {
            if(combo%3!=0 || skillDamageDepth>0) return;
            if(normalStrikeWeapon==1 && thunderReady<=0 && thunderLeft<=0)
            { thunderReady=10; Notify("[F] 토르의 망치 · 3초 낙뢰 · 사람 3초 기절",3); }
            if(normalStrikeWeapon==2 && iceReady<=0 && iceLeft<=0)
            { iceReady=10; Notify("[F] 얼음 총알 · 10초 동안 사용",3); }
        }
        private void ActivateWeaponSkill()
        {
            if(!RunActive || CombatLocked) return;
            if(weaponIndex==0) { StartWhirlwind(); return; }
            if(weaponIndex==1 && thunderReady>0 && thunderLeft<=0)
            {
                thunderReady=0; thunderLeft=3f; combo=0; comboLeft=0;
                weapon.localScale=Vector3.one; thunderTick=.5f; thunderPulse=0;
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
            {
                float active=Mathf.Min(dt,thunderLeft); thunderLeft=Mathf.Max(0,thunderLeft-dt); thunderTick-=active;
                while(thunderTick<=0 && thunderPulse<6) { thunderTick+=.5f; FireThunder(); }
            }
            if(iceLeft>0 && weaponIndex==2)
                Fx("Ice muzzle frost",weapon.position+avatar.forward*.6f,Vector3.one*.08f,Cyan,.2f,Vector3.up*.3f);
        }
        private void FireThunder()
        {
            // Six waves, three separate strikes per wave. No combo or timer rewards from skills.
            var targets=new System.Collections.Generic.List<OfficeDefender>();
            Vector3 origin=player.position+Vector3.up;
            foreach(var enemy in defenders)
                if(enemy!=null && enemy.Alive && !enemy.Dormant && enemy.room==current.number &&
                    Vector3.Distance(origin,enemy.AimPoint)<=14 && ClearDefenseSight(origin,enemy.AimPoint,player,enemy.transform)) targets.Add(enemy);
            for(int i=0;i<3;i++)
            {
                float angle=(thunderPulse*57+i*120)*Mathf.Deg2Rad;
                Vector3 at=targets.Count>0 ? targets[(thunderPulse*3+i)%targets.Count].transform.position :
                    player.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(4+i*2);
                at.x=Mathf.Clamp(at.x,-current.width/2+1,current.width/2-1);
                at.z=Mathf.Clamp(at.z,current.start+1,current.end-1);
                SkillDamage(()=> {
                    foreach(var enemy in defenders)
                        if(enemy!=null && enemy.Alive && !enemy.Dormant && enemy.room==current.number &&
                            Vector3.Distance(enemy.transform.position,at)<=1.7f && ClearDefenseSight(at+Vector3.up,enemy.AimPoint,player,enemy.transform))
                        { enemy.Hit(12); if(enemy.Alive && !enemy.dog) enemy.Stun(3); }
                    foreach(var prop in world.GetComponentsInChildren<OfficeProp>())
                        if(prop.room==current.number && Vector3.Distance(prop.transform.position,at)<=1.7f) prop.Hit(18,Vector3.down);
                });
                Vector3 previous=at+Vector3.up*10;
                for(int j=1;j<=10;j++)
                {
                    Vector3 next=at+Vector3.up*(10-j)+new Vector3(Random.Range(-.35f,.35f),0,Random.Range(-.35f,.35f));
                    var bolt=Fx("Thor lightning",(previous+next)/2,new Vector3(.11f,.11f,Vector3.Distance(previous,next)),j%2==0?Color.white:Cyan,.4f,Vector3.zero);
                    bolt.transform.rotation=Quaternion.LookRotation(next-previous); previous=next;
                }
                Fx("Thunder impact",at+Vector3.up*.1f,new Vector3(1.4f,.12f,1.4f),Cyan,.35f,Vector3.zero,1,PrimitiveType.Sphere);
            }
            thunderPulse++; shake=.65f; audioBus.Play("thunder",.45f);
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
