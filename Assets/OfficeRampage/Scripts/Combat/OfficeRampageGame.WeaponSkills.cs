using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private float whirlwindReady, whirlwindLeft, whirlwindTick, whirlwindAngle;
        private bool batStrike;

        private void ChargeWhirlwind()
        {
            if (!batStrike || combo % 3 != 0 || whirlwindLeft > 0 || whirlwindReady > 0) return;
            whirlwindReady = 10;
            Notify("[F] 휠윈드 준비 완료 · 10초 안에 발동하세요!", 3);
        }

        private void StartWhirlwind()
        {
            if (!RunActive || CombatLocked || weaponIndex != 0 || whirlwindReady <= 0 || whirlwindLeft > 0) return;
            combo=0; comboLeft=0; whirlwindReady = 0; whirlwindLeft = 10; whirlwindTick = .25f; whirlwindAngle = 0;
            audioBus.Play("combo", .6f);
        }

        private void TickWeaponSkills(float dt)
        {
            if (!RunActive || CombatLocked || EndlessMechActive) return;
            whirlwindReady = Mathf.Max(0, whirlwindReady - dt);
            TickElements(dt);
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) ActivateWeaponSkill();
            if (whirlwindLeft <= 0) return;
            // Fixed attack cadence: never apply damage once per rendered frame.
            float activeTime = Mathf.Min(dt, whirlwindLeft);
            whirlwindLeft = Mathf.Max(0, whirlwindLeft - dt);
            whirlwindAngle = (whirlwindAngle + activeTime * 900) % 360;
            whirlwindTick -= activeTime;
            while (whirlwindTick <= 0 && RunActive && !CombatLocked)
            {
                whirlwindTick += .25f;
                SkillDamage(() => SweepWeapon(player.position + Vector3.up * 1.35f, Vector3.forward, 3.4f, 360, 18, false));
                for (int i = 0; i < 16; i++)
                {
                    Vector3 ray = Quaternion.Euler(0, i * 22.5f + whirlwindAngle, 0) * Vector3.forward;
                    Fx("Whirlwind arc", player.position + Vector3.up + ray * 2.8f,
                        new Vector3(.12f, .12f, .45f), Cyan, .22f, ray * .8f);
                }
                audioBus.Play("swing", .23f, 1.4f);
            }
        }

        // Horizontal cone with a vertical limit; +/-50 degrees is a total 100-degree fan.
        private static bool InWeaponFan(Vector3 origin, Vector3 forward, Vector3 target, float range, float angle)
        {
            Vector3 delta = target - origin;
            if (Mathf.Abs(delta.y) > 2.5f) return false;
            delta.y = 0; forward.y = 0;
            if (delta.sqrMagnitude > range * range) return false;
            return angle >= 360 || delta.sqrMagnitude < .001f ||
                Vector3.Dot(forward.normalized, delta.normalized) >= Mathf.Cos(angle * .5f * Mathf.Deg2Rad) - .00001f;
        }

        private void SweepWeapon(Vector3 origin, Vector3 forward, float range, float angle, float damage, bool fire)
        {
            var seen = new HashSet<OfficeProp>();
            foreach (var col in Physics.OverlapSphere(origin, range + 2.5f))
            {
                if (!RunActive || CombatLocked) break;
                var prop = col.GetComponentInParent<OfficeProp>();
                if (prop == null || !prop.gameObject.activeInHierarchy || !seen.Add(prop) || prop.room != current.number) continue;
                Vector3 target = prop.transform.position + Vector3.up;
                if (!InWeaponFan(origin, forward, target, range, angle) || !ClearDefenseSight(origin, target, player, prop.transform)) continue;
                prop.Hit(damage, (target - origin).normalized);
                if (fire && prop != null && prop.health > 0 && prop.gameObject.activeInHierarchy) Ignite(prop.gameObject);
            }
            foreach (var enemy in defenders)
            {
                if (!RunActive || CombatLocked) break;
                if (enemy == null || !enemy.Alive || enemy.Dormant || enemy.room != current.number) continue;
                if (!InWeaponFan(origin, forward, enemy.AimPoint, range, angle) || !ClearDefenseSight(origin, enemy.AimPoint, player, enemy.transform)) continue;
                enemy.Hit(damage);
                if (fire && enemy != null && enemy.Alive) Ignite(enemy.gameObject);
            }
        }

        private void Ignite(GameObject target)
        {
            var burn = target.GetComponent<OfficeBurn>();
            if (burn == null) burn = target.AddComponent<OfficeBurn>();
            burn.game = this; burn.Refresh();
        }

        private void ShowWeaponFan(Vector3 origin, Vector3 forward, float range, bool fire)
        {
            forward.y = 0; forward.Normalize();
            Vector3 previous = origin + Quaternion.Euler(0, -50, 0) * forward * range;
            for (int i = 0; i <= 20; i++)
            {
                Vector3 ray = Quaternion.Euler(0, -50 + i * 5, 0) * forward;
                Vector3 end = origin + ray * range;
                Trail(previous, end, fire ? Orange : Cyan); previous = end;
                if (fire)
                    Fx("Shotgun fire", origin + ray * .5f, Vector3.one * .18f,
                        i % 2 == 0 ? Orange : Color.yellow, .3f, ray * (range / .3f), .6f, PrimitiveType.Sphere);
                if (i == 0 || i == 20 || (fire && i % 4 == 0)) Trail(origin, end, fire ? Orange : Cyan);
            }
        }

        private void DrawWeaponSkills()
        {
            DrawElementSkills();
            if (whirlwindLeft <= 0 && whirlwindReady <= 0) return;
            EnsureMenuStyles();
            Box(new Rect(340, 535, 600, 42), Navy);
            string text = whirlwindLeft > 0 ? "휠윈드 회전 중! " + whirlwindLeft.ToString("0.0") + "초" :
                "[F] 휠윈드 발동 · " + whirlwindReady.ToString("0.0") + "초 남음" + (weaponIndex == 0 ? "" : " · 1번 방망이 장착");
            GUI.Label(new Rect(355, 540, 580, 36), text, menuText);
        }
    }
}
