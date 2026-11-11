using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void RefreshExit(Room room)
        {
            if (room == null || room.number >= 5 || room.unlocked) return;
            int totalEnemies = OfficeDefenseRules.Count(room.number);
            int defeated = totalEnemies - LivingDefenders(room.number);
            if (!OfficeProgressRules.ExitReady(room.destroyed, room.total, defeated, totalEnemies)) return;
            room.unlocked = true;
            room.gate.GetComponent<Collider>().enabled = false;
            room.gateRenderer.sharedMaterial = Mat(Cyan);
            room.gate.transform.localScale = new Vector3(3.3f, .12f, .3f);
            room.gate.transform.position = new Vector3(0, .06f, room.end);
            Notify("EXIT OPEN - PROPS AND ENEMIES 60% CLEARED", 3);
            audioBus.Play("door", .45f);
        }
        private void SpawnHealingPotions(Room room, OfficeLayout layout)
        {
            var random = new System.Random(Random.Range(0, int.MaxValue));
            var navigation = new OfficeNavigation(layout);
            foreach (var point in navigation.SpawnPoints(random.Next(2, 5), random, room.number == 5))
            {
                var pickup = new GameObject("Healing potion +30 HP").AddComponent<OfficeHealingPotion>();
                pickup.transform.SetParent(world);
                pickup.transform.position = new Vector3(point.x, .08f, room.start + point.z);
                pickup.game = this; pickup.room = room.number;
                var visual = new GameObject("Floating bottle").transform;
                visual.SetParent(pickup.transform, false); visual.localPosition = Vector3.up * .65f;
                pickup.visual = visual;
                Shape(visual, "Green healing bottle", Vector3.zero, new Vector3(.35f, .45f, .35f), new Color(.1f, .95f, .35f), false, PrimitiveType.Capsule);
                Shape(visual, "Bottle cap", new Vector3(0, .3f, 0), new Vector3(.2f, .12f, .2f), Color.white, false);
                Shape(visual, "Health cross vertical", new Vector3(0, 0, -.19f), new Vector3(.07f, .23f, .02f), Color.white, false);
                Shape(visual, "Health cross horizontal", new Vector3(0, 0, -.19f), new Vector3(.23f, .07f, .02f), Color.white, false);
            }
        }
        public bool TryCollectHealingPotion(int room)
        {
            if (!RunActive || CombatLocked || hp >= 100 || !IsCurrentRoom(room)) return false;
            float before = hp;
            hp = OfficeProgressRules.Heal(hp);
            Notify("HEALING POTION  +" + (hp - before).ToString("0") + " HP", 2);
            audioBus.Play("combo", .45f, 1.3f);
            return true;
        }
    }
}
