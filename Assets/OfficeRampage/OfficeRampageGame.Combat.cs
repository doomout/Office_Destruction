using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private bool giftRequested, bfgReady, bfgUsed, combatLocked, desert;
        private string cinematicText = "";
        private float flash;
        private readonly Queue<GameObject> combatEffects = new Queue<GameObject>();
        public bool CombatLocked { get { return combatLocked; } }
        private void ResetExpansion()
        {
            giftRequested = bfgReady = bfgUsed = combatLocked = desert = false;
            cinematicText = ""; flash = 0; combatEffects.Clear();
        }
        public bool IsPlayerCollider(Collider col) { return col.transform == player || col.transform.IsChildOf(player); }
        private GameObject Fx(string name, Vector3 at, Vector3 size, Color color, float seconds, Vector3 velocity, float growth = 0, PrimitiveType type = PrimitiveType.Cube)
        {
            while (combatEffects.Count >= 160) { var old = combatEffects.Dequeue(); if (old != null) Destroy(old); }
            var obj = Shape(world, name, at, size, color, false, type);
            var fx = obj.AddComponent<OfficeVisualEffect>(); fx.game = this; fx.life = seconds; fx.velocity = velocity; fx.growth = growth;
            combatEffects.Enqueue(obj); return obj;
        }
        private void BuildExpandedWeapon()
        {
            Color steel = new Color(.5f, .62f, .68f);
            if (weaponIndex == 0 || weaponIndex == 1)
            {
                Shape(weapon, "Grip", new Vector3(0, .05f, .3f), new Vector3(.1f, .1f, .8f), Orange, false);
                if (weaponIndex == 0)
                {
                    var bat = Shape(weapon, "Baseball bat", new Vector3(0, .05f, .85f), new Vector3(.18f, .55f, .18f), new Color(.72f, .48f, .22f), false, PrimitiveType.Capsule);
                    bat.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
                else Shape(weapon, "Hammer head", new Vector3(0, .05f, .8f), new Vector3(.7f, .36f, .36f), steel, false);
            }
            else if (weaponIndex == 4)
            {
                Shape(weapon, "Whip handle", new Vector3(0, .05f, .25f), new Vector3(.12f, .12f, .5f), Orange, false);
                for (int i = 0; i < 7; i++) Shape(weapon, "Whip coil", new Vector3(Mathf.Sin(i) * .13f, -.05f * i, .5f + i * .08f), new Vector3(.04f, .04f, .2f), new Color(.4f, .22f, .1f), false);
            }
            else
            {
                bool heavy = weaponIndex >= 6;
                Shape(weapon, "Receiver", new Vector3(0, .08f, .2f), new Vector3(heavy ? .48f : .2f, heavy ? .4f : .2f, weaponIndex == 2 ? .35f : .65f), steel, false);
                Shape(weapon, "Grip", new Vector3(0, -.1f, .1f), new Vector3(.13f, .3f, .18f), Navy, false);
                Shape(weapon, "Barrel", new Vector3(0, .1f, .65f), new Vector3(heavy ? .3f : .09f, heavy ? .3f : .09f, weaponIndex == 2 ? .35f : .9f), Navy, false);
                if (weaponIndex == 3) Shape(weapon, "Magazine", new Vector3(0, -.17f, .35f), new Vector3(.14f, .42f, .2f), Orange, false);
                if (weaponIndex == 5) Shape(weapon, "Second barrel", new Vector3(.11f, .1f, .65f), new Vector3(.08f, .08f, .9f), steel, false);
                if (weaponIndex == 7)
                    for (int i = 0; i < 4; i++) Shape(weapon, "BFG energy cell", new Vector3((i % 2 == 0 ? -1 : 1) * .3f, .12f, .25f + i / 2 * .35f), new Vector3(.13f, .27f, .23f), Color.green, false);
            }
        }
        private void AttackExpanded()
        {
            if (combatLocked) return;
            if (weaponIndex == 7)
            {
                if (!bfgReady || bfgUsed || current.number != 5 || bossUnit == null || bossUnit.PhaseTwo) return;
                bfgUsed = true; bfgReady = false; StartCoroutine(NuclearTransition()); return;
            }
            cooldown = cooldowns[weaponIndex]; swing = 1; shake = weaponIndex == 3 ? .25f : .8f;
            audioBus.Weapon(weaponIndex <= 1 || weaponIndex == 4 ? 0 : weaponIndex == 6 ? 2 : 1);
            float range = OfficeCombatRules.Ranges[weaponIndex];
            Vector3 origin = player.position + Vector3.up * 1.35f;
            Vector3 aimPoint = view.transform.position + view.transform.forward * range;
            if (AimHit(view.transform.position, view.transform.forward, range, out var aim)) aimPoint = aim.point;
            Vector3 direction = (aimPoint - origin).normalized;
            if (weaponIndex <= 1)
            {
                float damage = weaponIndex == 0 ? 40 : 90;
                var seen = new HashSet<OfficeProp>();
                foreach (var col in Physics.OverlapSphere(origin, range))
                {
                    var prop = col.GetComponentInParent<OfficeProp>();
                    if (prop == null || !seen.Add(prop)) continue;
                    Vector3 delta = prop.transform.position + Vector3.up - origin;
                    if (Vector3.Dot(avatar.forward, delta.normalized) > .25f && ClearDefenseSight(origin, prop.transform.position + Vector3.up, player, prop.transform)) prop.Hit(damage, avatar.forward);
                }
                foreach (var enemy in defenders)
                    if (enemy != null && enemy.Alive && Vector3.Distance(origin, enemy.AimPoint) <= range && Vector3.Dot(avatar.forward, (enemy.AimPoint - origin).normalized) > .25f && ClearDefenseSight(origin, enemy.AimPoint, player, enemy.transform)) enemy.Hit(damage);
                return;
            }
            if (weaponIndex == 6)
            {
                SpawnProjectile(origin + direction * .5f, direction, player, true, true, 25, range, 120, 1);
                return;
            }
            int pellets = weaponIndex == 5 ? 9 : 1;
            float power = weaponIndex == 2 ? 32 : weaponIndex == 3 ? 12 : weaponIndex == 4 ? 55 : 16;
            for (int i = 0; i < pellets; i++)
            {
                Vector3 shot = (direction + (pellets > 1 ? Random.insideUnitSphere * .085f : Vector3.zero)).normalized;
                Vector3 end = origin + shot * range;
                if (AimHit(origin, shot, range, out var hit))
                {
                    end = hit.point;
                    var prop = hit.collider.GetComponentInParent<OfficeProp>(); if (prop != null) prop.Hit(power, shot);
                    var enemy = hit.collider.GetComponentInParent<OfficeDefender>(); if (enemy != null) enemy.Hit(power);
                }
                if (weaponIndex == 4)
                {
                    Vector3 previous = origin;
                    for (int n = 1; n <= 8; n++)
                    {
                        Vector3 point = Vector3.Lerp(origin, end, n / 8f) + Vector3.up * Mathf.Sin(n * Mathf.PI / 8) * .7f;
                        Trail(previous, point, Orange); previous = point;
                    }
                }
                else Trail(origin, end, weaponIndex == 3 ? Color.yellow : Cyan);
            }
        }
        public void ResolvePlayerBlast(Vector3 center, float range, float damage)
        {
            if (!RunActive || CombatLocked) return;
            Fx("Rocket blast", center, Vector3.one, Orange, .25f, Vector3.zero, 15, PrimitiveType.Sphere);
            audioBus.Play("explosion", .6f, 1, center);
            var seen = new HashSet<OfficeProp>();
            foreach (var col in Physics.OverlapSphere(center, range))
            {
                var prop = col.GetComponentInParent<OfficeProp>();
                if (prop != null && seen.Add(prop) && ClearDefenseSight(center, prop.transform.position + Vector3.up, player, prop.transform)) prop.Hit(damage, (prop.transform.position - center).normalized);
            }
            foreach (var enemy in defenders)
                if (enemy != null && enemy.Alive && Vector3.Distance(center, enemy.AimPoint) < range && ClearDefenseSight(center, enemy.AimPoint, player, enemy.transform)) enemy.Hit(damage);
        }
        private void SpawnProjectile(Vector3 origin, Vector3 direction, Transform owner, bool friendly, bool rocket, float speed, float range, float damage, float scale)
        {
            var obj = Shape(world, rocket ? "Rocket" : "Flying A4", origin, rocket ? new Vector3(.2f, .2f, .7f) : new Vector3(.42f, .035f, .59f) * scale, rocket ? Orange : Color.white, false);
            obj.transform.rotation = Quaternion.LookRotation(direction);
            var projectile = obj.AddComponent<OfficeProjectile>(); projectile.game = this; projectile.owner = owner;
            projectile.direction = direction; projectile.friendly = friendly; projectile.explosive = rocket;
            projectile.speed = speed; projectile.range = range; projectile.damage = damage; projectile.radius = .12f * scale;
        }
        public void ShowDefenderWarning(OfficeDefender unit, int kind, float range, Vector3 forward)
        {
            if (unit.warning != null) Destroy(unit.warning.gameObject);
            unit.warning = new GameObject("Attack range warning").transform; unit.warning.SetParent(world);
            unit.warning.position = unit.transform.position + Vector3.up * .045f;
            unit.warning.rotation = Quaternion.LookRotation(forward);
            Color color = kind == 1 ? Cyan : kind == 2 ? new Color(1, .85f, .25f) : Orange;
            if (kind == 2) Shape(unit.warning, "Paper trajectory", new Vector3(0, 0, range / 2), new Vector3(.12f, .025f, range), color, false);
            else
                for (int i = 0; i <= 12; i++)
                {
                    float angle = Mathf.Lerp(-35, 35, i / 12f);
                    Vector3 ray = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    var stripe = Shape(unit.warning, "Cone telegraph", ray * range / 2, new Vector3(.035f, .025f, range), color, false);
                    stripe.transform.localRotation = Quaternion.Euler(0, angle, 0);
                    Shape(unit.warning, "Range edge", ray * range, new Vector3(.25f, .035f, .2f), color, false);
                }
        }
        public void AnimateDefenderAttack(OfficeDefender unit, float t)
        {
            foreach (Transform part in unit.visual)
            {
                if (part.name == "Baton") part.localRotation = Quaternion.Euler(-18 - Mathf.Sin(t * Mathf.PI) * 110, 0, 0);
                if (part.name == "A4 hand") part.localPosition = new Vector3(.48f, 1.1f + Mathf.Sin(t * Mathf.PI) * .65f, .22f + t * .35f);
                if (part.name == "Sleeve" && part.localPosition.x > 0) part.localRotation = Quaternion.Euler(-Mathf.Sin(t * Mathf.PI) * 100, 0, 0);
            }
        }
        public void ReleaseDefenderAttack(OfficeDefender unit, int kind, Vector3 direction)
        {
            float range = (kind == 0 ? 2.4f : kind == 1 ? 6 : 16) * unit.RangeScale;
            if (kind == 2)
            {
                Vector3 origin = unit.AimPoint + direction * .5f;
                SpawnProjectile(origin, direction, unit.transform, false, false, 10, range, unit.boss ? 18 : 10, unit.RangeScale);
                audioBus.Play("swing", .25f, 1.5f, origin); return;
            }
            Vector3 delta = player.position - unit.transform.position;
            float height = Mathf.Abs(delta.y); delta.y = 0;
            if (delta.magnitude <= range && height < 1.5f * unit.RangeScale && Vector3.Dot(direction, delta.normalized) >= Mathf.Cos(35 * Mathf.Deg2Rad) && ClearDefenseSight(unit.AimPoint, player.position + Vector3.up, unit.transform, null))
                DefenderContact(unit.boss ? 20 : unit.dog ? 12 : 10);
            audioBus.Play(kind == 0 ? "swing" : "wall", .3f, kind == 0 ? 1 : 1.8f, unit.AimPoint);
            for (int i = 0; i <= 12; i++)
            {
                Vector3 ray = Quaternion.Euler(0, Mathf.Lerp(-35, 35, i / 12f), 0) * direction;
                Fx(kind == 0 ? "Baton arc" : "Sonic wave", unit.AimPoint + ray * .5f, Vector3.one * .14f, kind == 0 ? Orange : Cyan, .4f, ray * range / .4f, .3f, PrimitiveType.Sphere);
            }
        }
        public void BossRaged() { Notify("CEO RAGE! SPEED +50% / RANGE +50%", 4); audioBus.Play("explosion", .5f); }
        public void RequestSecretWeapon()
        {
            if (giftRequested || bfgUsed) return;
            giftRequested = true; StartCoroutine(SecretaryGift());
        }
        private IEnumerator SceneWait(float seconds)
        {
            while (seconds > 0) { if (RunActive) seconds -= Time.deltaTime; yield return null; }
        }
        private IEnumerator SecretaryGift()
        {
            combatLocked = true; cinematicText = "비서: 이걸 쓰세요! BFG 18, 딱 한 발이에요!";
            var secretary = new GameObject("Hidden secretary").transform; secretary.SetParent(world);
            Vector3 destination = player.position + avatar.forward * 2;
            Vector3 start = new Vector3(current.width / 2 - 1, .08f, Mathf.Clamp(player.position.z + 4, current.start + 2, current.end - 2));
            secretary.position = start; BuildPerson(secretary, false);
            Shape(secretary, "Skirt", new Vector3(0, .65f, 0), new Vector3(.7f, .5f, .45f), new Color(.38f, .12f, .4f), false);
            Shape(secretary, "Long hair", new Vector3(0, 1.6f, -.2f), new Vector3(.48f, .7f, .12f), Navy, false);
            var gift = Shape(secretary, "BFG 18 case", new Vector3(0, 1, .5f), new Vector3(.8f, .35f, .6f), Color.green, false);
            float t = 0;
            while (t < 1.5f)
            { if (RunActive) { t += Time.deltaTime; secretary.position = Vector3.Lerp(start, destination, t / 1.5f); secretary.LookAt(new Vector3(player.position.x, secretary.position.y, player.position.z)); } yield return null; }
            gift.SetActive(false); bfgReady = true; SetWeapon(7); audioBus.Play("combo", .7f);
            cinematicText = "BFG 18 획득! 왼쪽 클릭으로 발사 · 1회 사용";
            t = 0;
            while (t < 1.3f)
            { if (RunActive) { t += Time.deltaTime; secretary.position = Vector3.Lerp(destination, start, t / 1.3f); secretary.LookAt(start + Vector3.forward); } yield return null; }
            Destroy(secretary.gameObject); combatLocked = false; cinematicText = "";
            cooldown = .5f; Notify("BFG 18 READY - LEFT CLICK / ONE SHOT", 6);
        }
        private IEnumerator NuclearTransition()
        {
            combatLocked = true; cinematicText = "BFG 18 — 사무실 철거 승인!";
            audioBus.Play("rocket", .8f); shake = 3;
            var core = Fx("BFG energy", player.position + Vector3.up * 1.4f, Vector3.one * .7f, Color.green, 2, Vector3.zero, 2, PrimitiveType.Sphere);
            Vector3 origin = core.transform.position, target = bossUnit.AimPoint; float t = 0;
            while (t < .8f) { if (RunActive) { t += Time.deltaTime; core.transform.position = Vector3.Lerp(origin, target, t / .8f); } yield return null; }
            flash = .85f; audioBus.Play("explosion", 1f); shake = 8;
            Fx("Mushroom stem", target, new Vector3(2, 1, 2), Orange, 2.5f, Vector3.up * 3, 5, PrimitiveType.Sphere);
            Fx("Mushroom crown", target + Vector3.up * 5, new Vector3(6, 3, 6), Color.yellow, 2.5f, Vector3.up, 12, PrimitiveType.Sphere);
            yield return SceneWait(1.2f);
            Transform bossTransform = bossUnit.transform; bossTransform.SetParent(world, true);
            for (int i = world.childCount - 1; i >= 0; i--)
            {
                var child = world.GetChild(i);
                if (child == player || child == view.transform || child == bossTransform || child.GetComponent<Light>() != null) continue;
                child.gameObject.SetActive(false); Destroy(child.gameObject);
            }
            oilPuddles.Clear(); desert = true;
            current.width *= 1.25f; float length = (current.end - current.start) * 1.25f; current.end = current.start + length;
            current.layoutName = "DESERT / PHASE 2";
            var arena = new GameObject("Desert arena").transform; arena.SetParent(world);
            Shape(arena, "Sand floor", new Vector3(0, -.3f, current.start + length / 2), new Vector3(current.width + 50, .6f, length + 50), new Color(.78f, .61f, .35f));
            for (int side = -1; side <= 1; side += 2)
            {
                Boundary(arena, new Vector3(side * current.width / 2, 5, current.start + length / 2), new Vector3(.4f, 10, length));
                Boundary(arena, new Vector3(0, 5, side < 0 ? current.start : current.end), new Vector3(current.width, 10, .4f));
                for (float z = current.start; z <= current.end; z += 3)
                    Shape(arena, "Boundary rocks", new Vector3(side * (current.width / 2 + .7f), .4f, z), new Vector3(1.5f, .8f, 1.4f), new Color(.49f, .36f, .23f), false, PrimitiveType.Sphere);
                for (float x = -current.width / 2; x <= current.width / 2; x += 3)
                    Shape(arena, "Boundary rocks", new Vector3(x, .4f, side < 0 ? current.start - .7f : current.end + .7f), new Vector3(1.5f, .8f, 1.4f), new Color(.49f, .36f, .23f), false, PrimitiveType.Sphere);
                for (int i = 0; i < 6; i++) Shape(arena, "Distant dune", new Vector3(side * (current.width / 2 + 12), -1, current.start + i * 9), new Vector3(18, 7, 14), new Color(.73f, .53f, .29f), false, PrimitiveType.Sphere);
            }
            RenderSettings.fogColor = new Color(.82f, .69f, .48f); view.backgroundColor = new Color(.68f, .8f, .88f);
            controller.enabled = false; player.position = new Vector3(0, .1f, current.start + 5); controller.enabled = true; vertical = 0;
            bossUnit.Teleport(new Vector3(0, .1f, current.start + length * .6f));
            yaw = 0; pitch = 13; flash = .3f;
            SpawnHealingPotions(current, new OfficeLayout { width = current.width, length = length });
            cinematicText = "대표: 돈이면 다 돼! 돈 갑옷, 내려와라!";
            for (int i = 0; i < 70; i++)
            {
                Vector3 v = Random.onUnitSphere * 4 + Vector3.up * 4;
                var bill = Fx("Flying money", bossUnit.AimPoint, new Vector3(.38f, .02f, .18f), new Color(.3f, .75f, .38f), 4, v);
                bill.GetComponent<OfficeVisualEffect>().spin = Random.onUnitSphere * 240;
            }
            var armor = new GameObject("Money armor").transform; armor.SetParent(bossUnit.visual, false); armor.localPosition = Vector3.up * 12;
            Shape(armor, "Gold breastplate", new Vector3(0, 1.1f, 0), new Vector3(.8f, .8f, .5f), new Color(.85f, .66f, .13f), false);
            for (int side = -1; side <= 1; side += 2)
                Shape(armor, "Money shoulder", new Vector3(side * .5f, 1.42f, 0), new Vector3(.38f, .3f, .48f), new Color(.25f, .65f, .3f), false);
            t = 0;
            while (t < 2) { if (RunActive) { t += Time.deltaTime; armor.localPosition = Vector3.up * Mathf.Lerp(12, 0, t / 2); } yield return null; }
            bossUnit.EnterPhaseTwo(new OfficeNavigation(new OfficeLayout { width = current.width, length = length }), current.width, current.end);
            cinematicText = "2페이즈! 대표 최대 체력의 50% 회복"; audioBus.Play("thud", .8f);
            yield return SceneWait(1);
            combatLocked = false; cinematicText = ""; flash = 0; invulnerability = 2; SetWeapon(0);
        }
        private void Boundary(Transform root, Vector3 position, Vector3 size)
        { var wall = Shape(root, "Invisible arena boundary", position, size, Navy); wall.GetComponent<Renderer>().enabled = false; }
        private void DrawExpansionOverlay()
        {
            if (desert && !combatLocked)
                GUI.Label(new Rect(440, 145, 520, 35), "PHASE 2 / MONEY ARMOR", labelStyle);
            if (flash > 0)
            {
                Box(new Rect(0, 0, 1280, 720), new Color(1, 1, .85f, flash));
                if (Event.current.type == EventType.Repaint && RunActive) flash = Mathf.Max(0, flash - Time.unscaledDeltaTime * .6f);
            }
            if (combatLocked)
            {
                EnsureMenuStyles(); Box(new Rect(150, 250, 980, 95), Navy);
                GUI.Label(new Rect(180, 270, 920, 65), cinematicText, menuText);
            }
        }
    }
}
