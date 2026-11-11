using System.Collections.Generic;
using UnityEngine;

namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private readonly List<OfficeDefender> defenders = new List<OfficeDefender>();
        private OfficeDefender bossUnit;
        private GameObject bossChair;
        private Font defenderFont;
        private bool finalRoomSealed;
        public Vector3 PlayerPosition { get { return player.position; } }
        public bool IsCurrentRoom(int room)
        {
            if (current == null || player == null) return false;
            var bounds = rooms.Find(r => r.number == room);
            return bounds != null && player.position.z >= bounds.start - .1f && player.position.z <= bounds.end + .1f;
        }

        private int LivingDefenders(int room, bool dogsOnly = false)
        {
            int count = 0;
            foreach (var enemy in defenders)
                if (enemy != null && enemy.Alive && enemy.room == room && !enemy.boss && (!dogsOnly || enemy.dog)) count++;
            return count;
        }

        private void SetupDefenders(Room room, OfficeLayout layout, Transform root)
        {
            var navigation = new OfficeNavigation(layout);
            var random = new System.Random(Random.Range(0, int.MaxValue));
            var spawnPositions = navigation.SpawnPoints(OfficeDefenseRules.Count(room.number), random, room.number == 5);
            for (int i = 0; i < OfficeDefenseRules.Count(room.number); i++)
            {
                var p = spawnPositions[i];
                var spawn = new Vector3(p.x, .08f, room.start + p.z);
                CreateDefender(root, room, navigation, spawn, OfficeDefenseRules.Name(room.number, i),
                    OfficeDefenseRules.Health(room.number, i, random), room.number == 5, false);
            }
            if (room.number != 5) return;
            bossChair = new GameObject("CEO chair");
            bossChair.transform.SetParent(root, false);
            bossChair.transform.position = new Vector3(0, 0, room.end - 5);
            Shape(bossChair.transform, "Seat", new Vector3(0, .55f, 0), new Vector3(1.3f, .2f, 1.1f), Navy, false);
            Shape(bossChair.transform, "Backrest", new Vector3(0, 1.2f, .48f), new Vector3(1.3f, 1.4f, .2f), Navy, false);
            Shape(bossChair.transform, "Chair base", new Vector3(0, .25f, 0), new Vector3(.2f, .5f, .2f), Color.gray, false);
            bossUnit = CreateDefender(root, room, navigation, bossChair.transform.position + Vector3.up * .08f,
                "대표", OfficeDefenseRules.BossHealth, false, true);
            bossUnit.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        private OfficeDefender CreateDefender(Transform root, Room room, OfficeNavigation navigation,
            Vector3 position, string name, int health, bool dog, bool isBoss)
        {
            var unit = new GameObject(name).AddComponent<OfficeDefender>();
            unit.transform.SetParent(root); unit.transform.position = position;
            unit.game = this; unit.navigation = navigation; unit.room = room.number;
            unit.roomStart = room.start; unit.roomEnd = room.end; unit.roomWidth = room.width;
            unit.dog = dog; unit.boss = isBoss; unit.guard = room.number == 1;
            unit.displayName = name;
            unit.visual = new GameObject("Body").transform;
            unit.visual.SetParent(unit.transform, false);
            if (dog) BuildDog(unit.visual);
            else
            {
                BuildPerson(unit.visual, isBoss);
                Color uniform = unit.guard ? new Color(.13f, .16f, .27f) : room.number == 2 ? new Color(.24f, .43f, .47f) :
                    room.number == 3 ? new Color(.42f, .3f, .15f) : new Color(.37f, .12f, .2f);
                foreach (Transform part in unit.visual)
                    if (part.name == "Jacket" || part.name == "Sleeve") part.GetComponent<Renderer>().sharedMaterial = Mat(uniform);
                if (unit.guard || isBoss)
                {
                    if (unit.guard)
                    {
                        Shape(unit.visual, "Security cap", new Vector3(0, 1.94f, .04f), new Vector3(.52f, .13f, .5f), Navy, false);
                        Shape(unit.visual, "Security badge", new Vector3(-.19f, 1.2f, .2f), new Vector3(.09f, .12f, .03f), Color.yellow, false);
                    }
                    var baton = Shape(unit.visual, "Baton", new Vector3(.48f, .95f, .43f), new Vector3(.11f, .11f, .9f), new Color(.32f, .16f, .07f), false);
                    baton.transform.localRotation = Quaternion.Euler(-18, 0, 0);
                }
            }
            var label = new GameObject("Rank and health").AddComponent<TextMesh>();
            label.transform.SetParent(unit.transform, false);
            label.transform.localPosition = Vector3.up * (dog ? 1.25f : 2.4f);
            label.anchor = TextAnchor.MiddleCenter; label.fontSize = 36; label.characterSize = .075f;
            label.color = isBoss ? Orange : dog ? new Color(1, .8f, .35f) : Color.white;
            if (defenderFont == null) defenderFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Arial" }, 36);
            label.font = defenderFont;
            label.GetComponent<Renderer>().sharedMaterial = defenderFont.material;
            unit.label = label;
            if (isBoss)
            {
                unit.warning = Shape(root, "CEO charge warning", Vector3.zero, new Vector3(1.8f, .025f, 9), Orange, false).transform;
                unit.warning.gameObject.SetActive(false);
                unit.flames = new GameObject("Raging flames").transform;
                unit.flames.SetParent(unit.visual, false);
                for (int i = 0; i < 24; i++)
                {
                    float angle = i * Mathf.PI / 5;
                    Shape(unit.flames, "Flame", new Vector3(Mathf.Sin(angle) * .47f, .25f + (i % 6) * .33f, Mathf.Cos(angle) * .4f),
                        new Vector3(.15f, .5f, .15f), i % 2 == 0 ? Orange : Color.yellow, false, PrimitiveType.Sphere);
                }
                var glow = new GameObject("Rage glow").AddComponent<Light>();
                glow.transform.SetParent(unit.flames, false); glow.transform.localPosition = Vector3.up * 1.7f;
                glow.type = LightType.Point; glow.color = Orange; glow.range = 3; glow.intensity = 2;
                unit.flames.gameObject.SetActive(false);
            }
            if (!dog && (room.number >= 3 || isBoss)) Shape(unit.visual, "A4 hand", new Vector3(.48f, 1.1f, .22f), new Vector3(.21f, .02f, .297f), Color.white, false);
            unit.Initialize(health, isBoss);
            defenders.Add(unit);
            return unit;
        }

        private void BuildDog(Transform root)
        {
            var fur = new Color(.32f, .19f, .1f);
            Shape(root, "Dog torso", new Vector3(0, .5f, 0), new Vector3(.5f, .45f, .9f), fur, false);
            Shape(root, "Dog head", new Vector3(0, .74f, .42f), new Vector3(.42f, .42f, .42f), fur, false);
            Shape(root, "Muzzle", new Vector3(0, .65f, .69f), new Vector3(.29f, .2f, .24f), Navy, false);
            Shape(root, "Collar", new Vector3(0, .58f, .31f), new Vector3(.54f, .15f, .15f), Orange, false);
            for (int s = -1; s <= 1; s += 2)
            {
                Shape(root, "Ear", new Vector3(s * .16f, 1, .35f), new Vector3(.12f, .3f, .17f), Navy, false);
                Shape(root, "Eye", new Vector3(s * .14f, .8f, .64f), new Vector3(.055f, .055f, .02f), Orange, false);
                foreach (float z in new[] { -.3f, .3f })
                    Shape(root, "Paw", new Vector3(s * .19f, .22f, z), new Vector3(.15f, .4f, .18f), fur, false);
            }
            var tail = Shape(root, "Tail", new Vector3(0, .65f, -.59f), new Vector3(.12f, .12f, .5f), fur, false);
            tail.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }

        public void SetBossPose(OfficeDefender unit, float standing)
        {
            unit.visual.localPosition = Vector3.up * Mathf.Lerp(-.35f, 0, standing);
            foreach (Transform part in unit.visual)
            {
                float side = Mathf.Sign(part.localPosition.x);
                if (part.name == "Trouser")
                {
                    part.localPosition = Vector3.Lerp(new Vector3(side * .19f, .65f, .3f), new Vector3(side * .19f, .36f, 0), standing);
                    part.localScale = Vector3.Lerp(new Vector3(.25f, .35f, .6f), new Vector3(.25f, .7f, .3f), standing);
                }
                if (part.name == "Shoe") part.localPosition = Vector3.Lerp(new Vector3(side * .19f, .45f, .6f), new Vector3(side * .19f, .08f, .08f), standing);
                if (part.name == "Furious face" && standing > 0) part.GetComponent<Renderer>().sharedMaterial = Mat(new Color(.95f, .16f, .09f));
            }
        }

        public bool ClearDefenseSight(Vector3 origin, Vector3 target, Transform ignore, Transform receiver)
        {
            Vector3 delta = target - origin;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                Transform t = hit.collider.transform;
                if (t == ignore || (ignore != null && t.IsChildOf(ignore)) || t == player ||
                    t == receiver || (receiver != null && t.IsChildOf(receiver))) continue;
                if (hit.distance < delta.magnitude - .05f) return false;
            }
            return true;
        }

        public void DefenderContact(float damage) { if (RunActive) Hurt(damage); }
        public void DefenderImpact(OfficeDefender unit)
        {
            audioBus.Impact(3, false, unit.AimPoint);
            Impact(unit.AimPoint, unit.boss ? Orange : Cyan, -unit.transform.forward, 2);
        }
        public void DefenderDefeated(OfficeDefender unit)
        {
            score += unit.boss ? 5000 : unit.dog ? 300 : 200;
            if (unit.boss) { End(true); return; }
            if (OfficeDefenseRules.CanWakeBoss(unit.room, LivingDefenders(5, true), bossFight))
            {
                bossFight = true;
                if (bossChair != null) bossChair.SetActive(false);
                bossUnit.Wake(); audioBus.ChangeRoom(6);
                Notify("ALL GUARD DOGS DEFEATED - CEO BATTLE!", 4);
            }
        }

        private void SealFinalRoom()
        {
            if (current.number != 5 || finalRoomSealed || player.position.z <= current.start + 2) return;
            finalRoomSealed = true;
            var seal = Shape(world, "Final room entry seal", new Vector3(0, 2.5f, current.start), new Vector3(current.width, 5f, .35f), Orange);
            seal.GetComponent<Renderer>().enabled = false;
        }
    }
}
