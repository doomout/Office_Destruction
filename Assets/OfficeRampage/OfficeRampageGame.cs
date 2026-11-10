using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OfficeRampage
{
    /// <summary>Self-contained prototype. The saved scene contains only this bootstrap.</summary>
    public sealed class OfficeRampageGame : MonoBehaviour
    {
        private sealed class Room
        {
            public int number, total, destroyed, required;
            public float start, end, width;
            public GameObject gate;
            public Renderer gateRenderer;
            public bool unlocked;
            public string layoutName;
        }

        private readonly List<Room> rooms = new List<Room>();
        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        private readonly Queue<GameObject> debris = new Queue<GameObject>();
        private readonly string[] weaponNames = { "SLEDGEHAMMER", "SHOTGUN", "ROCKET LAUNCHER" };
        private readonly float[] cooldowns = { .48f, .7f, 1.15f };
        private Transform world, player, avatar, weapon, boss;
        private CharacterController controller;
        private Camera view;
        private Room current;
        private float remaining, hp, yaw, pitch, vertical, cooldown, comboLeft, noticeLeft;
        private float bossHealth, bossPhaseTime, invulnerability, swing, shake;
        private Vector3 bossChargeDirection;
        private int weaponIndex, combo, score, bossPhase;
        private bool ended, won, paused, bossFight;
        private string notice = "";
        private Transform bossWarning;
        private GUIStyle titleStyle, labelStyle, smallStyle, centeredStyle;
        private float originalTimeScale;
        private OfficeAudio audioBus;
        private readonly int[] layoutOrder = { 0, 1, 2, 3, 4 };

        private static readonly Color Navy = new Color(.035f, .07f, .12f);
        private static readonly Color Cyan = new Color(.12f, .85f, .86f);
        private static readonly Color Orange = new Color(1f, .36f, .12f);

        private void Start()
        {
            originalTimeScale = Time.timeScale;
            ResetRun();
        }

        private void ResetRun()
        {
            Time.timeScale = 1;
            if (audioBus == null) audioBus = gameObject.AddComponent<OfficeAudio>();
            audioBus.ResetRun();
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            rooms.Clear();
            debris.Clear();
            world = new GameObject("Generated Office Rampage").transform;
            world.SetParent(transform);
            remaining = 120; hp = 100; yaw = 0; pitch = 13; vertical = 0;
            score = combo = weaponIndex = bossPhase = 0;
            cooldown = comboLeft = invulnerability = swing = shake = 0;
            ended = won = paused = bossFight = false;
            boss = bossWarning = null;
            for (int i = layoutOrder.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = layoutOrder[i]; layoutOrder[i] = layoutOrder[j]; layoutOrder[j] = temp;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .62f, .72f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = Navy;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 45;
            RenderSettings.fogEndDistance = 100;
            var sun = new GameObject("Office daylight").AddComponent<Light>();
            sun.transform.SetParent(world);
            sun.type = LightType.Directional;
            sun.intensity = 1.5f;
            sun.transform.rotation = Quaternion.Euler(55, -25, 0);

            player = new GameObject("Angry employee").transform;
            player.SetParent(world);
            player.position = new Vector3(0, .08f, 3);
            controller = player.gameObject.AddComponent<CharacterController>();
            controller.height = 1.85f;
            controller.radius = .36f;
            controller.center = Vector3.up * .95f;
            controller.stepOffset = .3f;
            avatar = new GameObject("Employee suit").transform;
            avatar.SetParent(player, false);
            BuildPerson(avatar, false);
            weapon = new GameObject("Weapon").transform;
            weapon.SetParent(avatar, false);
            SetWeapon(0);

            view = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            view.tag = "MainCamera";
            view.transform.SetParent(world);
            view.nearClipPlane = .08f;
            view.farClipPlane = 120;
            view.fieldOfView = 72;
            view.backgroundColor = Navy;
            view.clearFlags = CameraClearFlags.SolidColor;
            current = BuildRoom(1, 0);
            Notify("DESTROY 60% OF THE OFFICE TO OPEN THE EXIT", 5);
            LockCursor(true);
            UpdateCamera();
        }

        private Material Mat(Color color)
        {
            if (materials.TryGetValue(color, out var material)) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            material.SetFloat("_Smoothness", .28f);
            materials.Add(color, material);
            return material;
        }

        private GameObject Shape(Transform parent, string name, Vector3 pos, Vector3 size, Color color,
            bool solid = true, PrimitiveType type = PrimitiveType.Cube)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
            obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = Mat(color);
            if (!solid)
            {
                var collider = obj.GetComponent<Collider>();
                collider.enabled = false;
                Destroy(collider);
            }
            return obj;
        }

        private void BuildPerson(Transform root, bool isBoss)
        {
            Color suit = isBoss ? new Color(.23f, .08f, .15f) : new Color(.12f, .22f, .29f);
            Color skin = new Color(.95f, .62f, .42f);
            Shape(root, "Jacket", new Vector3(0, 1.05f, 0), new Vector3(.66f, .72f, .36f), suit, false);
            Shape(root, "Furious face", new Vector3(0, 1.64f, 0), new Vector3(.43f, .44f, .42f), skin, false);
            Shape(root, "Hair", new Vector3(0, 1.88f, -.03f), new Vector3(.46f, .12f, .43f), Navy, false);
            for (int s = -1; s <= 1; s += 2)
            {
                Shape(root, "Sleeve", new Vector3(s * .43f, 1.08f, .03f), new Vector3(.2f, .65f, .23f), suit, false);
                Shape(root, "Fist", new Vector3(s * .43f, .74f, .03f), Vector3.one * .22f, skin, false);
                Shape(root, "Trouser", new Vector3(s * .19f, .36f, 0), new Vector3(.25f, .7f, .3f), Navy, false);
                Shape(root, "Shoe", new Vector3(s * .19f, .08f, .08f), new Vector3(.28f, .16f, .46f), Color.black, false);
                var brow = Shape(root, "Angry eyebrow", new Vector3(s * .11f, 1.73f, .222f), new Vector3(.15f, .045f, .03f), Color.black, false);
                brow.transform.localRotation = Quaternion.Euler(0, 0, s * 22);
                Shape(root, "Eye", new Vector3(s * .11f, 1.66f, .224f), new Vector3(.05f, .05f, .025f), Color.white, false);
            }
            Shape(root, "Tie", new Vector3(0, 1.17f, .193f), new Vector3(.1f, .4f, .03f), Orange, false);
        }

        private Room BuildRoom(int number, float start)
        {
            bool arena = number == 6;
            var layout = arena ? null : OfficeLayout.Create(layoutOrder[number - 1], Random.Range(0, int.MaxValue));
            var room = new Room { number = number, start = start,
                width = arena ? 24 : layout.width, end = start + (arena ? 28 : layout.length),
                layoutName = arena ? "CEO ARENA" : layout.name };
            rooms.Add(room);
            var root = new GameObject(arena ? "CEO arena" : "Office " + number).transform;
            root.SetParent(world);
            float length = room.end - room.start;
            float middle = (room.start + room.end) / 2;
            var floor = number % 2 == 0 ? new Color(.18f, .24f, .28f) : new Color(.23f, .28f, .32f);
            Shape(root, "Floor", new Vector3(0, -.25f, middle), new Vector3(room.width, .5f, length), floor);
            // Only mark the entrance and exit: the player must navigate the partitions.
            foreach (float z in new[] { start + 1, start + 2, room.end - 2, room.end - 1 })
                Shape(root, "Door guide", new Vector3(0, .012f, z), new Vector3(.12f, .018f, .65f), Cyan, false);
            for (int s = -1; s <= 1; s += 2)
            {
                Shape(root, "Side wall", new Vector3(s * (room.width / 2 + .15f), 2, middle), new Vector3(.3f, 4, length), new Color(.55f, .64f, .67f));
                for (float z = start + 3; z < room.end - 1; z += 5)
                {
                    Shape(root, "Window", new Vector3(s * (room.width / 2 - .025f), 2.3f, z), new Vector3(.04f, 1.9f, 3), new Color(.15f, .4f, .53f), false);
                    Shape(root, "Wall light", new Vector3(s * (room.width / 2 - .05f), 3.65f, z), new Vector3(.12f, .1f, 3), Cyan, false);
                }
                float segment = (room.width - 3.4f) / 2;
                Shape(root, "Exit wall", new Vector3(s * (1.7f + segment / 2), 2, room.end), new Vector3(segment, 4, .35f), Navy);
                if (number > 1)
                    Shape(root, "Entry wall", new Vector3(s * (1.7f + segment / 2), 2, start), new Vector3(segment, 4, .3f), Navy);
            }
            if (number == 1)
                Shape(root, "Entrance", new Vector3(0, 2, start), new Vector3(room.width, 4, .3f), Navy);
            Shape(root, "Exit lintel", new Vector3(0, 3.65f, room.end), new Vector3(3.4f, .7f, .4f), Cyan);
            room.gate = Shape(root, arena ? "Boardroom wall" : "Locked exit", new Vector3(0, 1.55f, room.end), new Vector3(3.3f, 3.1f, .3f), Orange);
            room.gateRenderer = room.gate.GetComponent<Renderer>();
            var sign = new GameObject("Office sign").AddComponent<TextMesh>();
            sign.transform.SetParent(root);
            sign.transform.position = new Vector3(0, 3.57f, room.end - .26f);
            sign.transform.rotation = Quaternion.identity;
            sign.text = arena ? "THE CEO" : "OFFICE 0" + number + " / 05";
            sign.anchor = TextAnchor.MiddleCenter;
            sign.characterSize = .16f;
            sign.fontSize = 48;
            sign.color = Color.white;

            if (!arena)
            {
                foreach (var wall in layout.walls)
                {
                    Vector3 position = new Vector3(wall.x, 1.8f, start + wall.z);
                    // No OfficeProp component: all weapons and the player collide with these walls.
                    Shape(root, "Indestructible partition", position,
                        new Vector3(wall.width, 3.6f, wall.depth), new Color(.26f, .32f, .37f));
                    Shape(root, "Partition warning cap", position + Vector3.up * 1.72f,
                        new Vector3(wall.width, .12f, wall.depth + .025f), Orange, false);
                    foreach (float face in new[] { -.24f, .24f })
                        Shape(root, "Reinforced wall stripe", new Vector3(wall.x, .4f, start + wall.z + face),
                            new Vector3(wall.width, .12f, .02f), Orange, false);
                }
                foreach (var point in layout.furniture)
                    SpawnProp(root, room, new Vector3(point.x, 0, start + point.z), Random.Range(0, 4));
                room.required = Mathf.CeilToInt(room.total * .6f);
            }
            return room;
        }

        private void SpawnProp(Transform root, Room room, Vector3 position, int kind)
        {
            string[] names = { "Desk", "Computer", "Water cooler", "Printer" };
            Color wood = new Color(.62f, .39f, .2f);
            Color shell = new Color(.73f, .8f, .8f);
            var prop = new GameObject(names[kind]).AddComponent<OfficeProp>();
            prop.transform.SetParent(root);
            prop.transform.position = position;
            prop.transform.rotation = Quaternion.Euler(0, Random.Range(-12f, 12f), 0);
            prop.game = this; prop.room = room.number;
            prop.soundMaterial = kind;
            prop.health = kind == 0 ? 55 : 40;
            prop.debrisColor = kind == 0 ? wood : shell;
            room.total++;
            var t = prop.transform;
            if (kind <= 1)
            {
                Shape(t, "Desktop", new Vector3(0, .86f, 0), new Vector3(2.3f, .16f, 1.35f), wood);
                for (int x = -1; x <= 1; x += 2)
                    for (int z = -1; z <= 1; z += 2)
                        Shape(t, "Desk leg", new Vector3(x * .95f, .4f, z * .5f), new Vector3(.12f, .8f, .12f), Navy);
                if (kind == 1)
                {
                    Shape(t, "Monitor", new Vector3(0, 1.4f, .22f), new Vector3(1.1f, .72f, .12f), Navy);
                    Shape(t, "Screen", new Vector3(0, 1.4f, .15f), new Vector3(.96f, .57f, .02f), Cyan, false);
                    Shape(t, "Stand", new Vector3(0, 1.02f, .22f), new Vector3(.14f, .3f, .14f), Navy);
                    Shape(t, "Keyboard", new Vector3(0, .98f, -.35f), new Vector3(.8f, .06f, .27f), shell);
                    Shape(t, "Computer tower", new Vector3(.8f, 1.2f, .15f), new Vector3(.35f, .6f, .6f), Navy);
                }
                else
                    Shape(t, "Paper stack", new Vector3(.6f, 1, 0), new Vector3(.45f, .12f, .6f), shell);
            }
            else if (kind == 2)
            {
                Shape(t, "Cooler body", new Vector3(0, .62f, 0), new Vector3(.8f, 1.24f, .8f), shell);
                Shape(t, "Water bottle", new Vector3(0, 1.55f, 0), new Vector3(.55f, .38f, .55f), new Color(.12f, .5f, .85f), true, PrimitiveType.Cylinder);
                Shape(t, "Tap", new Vector3(0, .92f, -.46f), new Vector3(.23f, .12f, .16f), Cyan);
            }
            else
            {
                Shape(t, "Printer cabinet", new Vector3(0, .55f, 0), new Vector3(1.2f, 1.1f, 1), shell);
                Shape(t, "Scanner lid", new Vector3(0, 1.18f, 0), new Vector3(1.3f, .16f, 1.05f), Navy);
                Shape(t, "Paper output", new Vector3(0, .82f, -.54f), new Vector3(.72f, .13f, .12f), Navy);
                Shape(t, "Control panel", new Vector3(.35f, 1.29f, -.26f), new Vector3(.3f, .05f, .24f), Cyan);
            }
        }

        private void SetWeapon(int index)
        {
            weaponIndex = index;
            for (int i = weapon.childCount - 1; i >= 0; i--)
            { weapon.GetChild(i).gameObject.SetActive(false); Destroy(weapon.GetChild(i).gameObject); }
            weapon.localPosition = new Vector3(.48f, .95f, .25f);
            weapon.localRotation = Quaternion.identity;
            if (index == 0)
            {
                Shape(weapon, "Hammer handle", new Vector3(0, .12f, .25f), new Vector3(.09f, .1f, .9f), new Color(.5f, .3f, .16f), false);
                Shape(weapon, "Hammer head", new Vector3(0, .12f, .73f), new Vector3(.6f, .3f, .3f), new Color(.45f, .52f, .58f), false);
            }
            else
            {
                Shape(weapon, "Stock", Vector3.zero, new Vector3(.2f, .24f, .55f), Orange, false);
                Shape(weapon, "Barrel", new Vector3(0, .04f, .5f), new Vector3(index == 1 ? .16f : .33f, index == 1 ? .16f : .33f, .9f), Navy, false);
                Shape(weapon, "Barrel band", new Vector3(0, .04f, .84f), Vector3.one * (index == 1 ? .2f : .38f), Cyan, false);
            }
        }

        private void Update()
        {
            var keys = Keyboard.current;
            var mouse = Mouse.current;
            if (keys != null && keys.mKey.wasPressedThisFrame) audioBus.Muted = !audioBus.Muted;
            if (keys != null && keys.rKey.wasPressedThisFrame) { ResetRun(); return; }
            if (keys != null && keys.escapeKey.wasPressedThisFrame && !ended)
            {
                paused = !paused; Time.timeScale = paused ? 0 : 1; LockCursor(!paused);
                audioBus.SetPaused(paused);
            }
            if (ended || paused) return;
            float dt = Time.deltaTime;
            remaining = Mathf.Max(0, remaining - dt);
            if (remaining <= 0) { End(false); return; }
            cooldown -= dt; comboLeft -= dt; noticeLeft -= dt; invulnerability -= dt;
            if (comboLeft <= 0) combo = 0;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * .12f;
                pitch = Mathf.Clamp(pitch - delta.y * .1f, -30, 65);
            }
            Vector2 input = Vector2.zero;
            if (keys != null)
            {
                input.x = (keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0);
                input.y = (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0);
                if (keys.digit1Key.wasPressedThisFrame) SetWeapon(0);
                if (keys.digit2Key.wasPressedThisFrame) SetWeapon(1);
                if (keys.digit3Key.wasPressedThisFrame) SetWeapon(2);
            }
            input = Vector2.ClampMagnitude(input, 1);
            if (controller.isGrounded && vertical < 0) vertical = -2;
            if (keys != null && keys.spaceKey.wasPressedThisFrame && controller.isGrounded) vertical = 7;
            vertical -= 21 * dt;
            float speed = keys != null && keys.leftShiftKey.isPressed ? 8 : 5.6f;
            Vector3 move = Quaternion.Euler(0, yaw, 0) * new Vector3(input.x, 0, input.y) * speed;
            var collision = controller.Move((move + Vector3.up * vertical) * dt);
            if ((collision & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            avatar.rotation = Quaternion.Euler(0, yaw, 0);
            swing = Mathf.MoveTowards(swing, 0, dt * 5);
            weapon.localRotation = Quaternion.Euler(-swing * 60, -swing * 25, 0);
            UpdateCamera();
            if (mouse != null && mouse.leftButton.isPressed && Cursor.lockState == CursorLockMode.Locked && cooldown <= 0) Attack();
            if (bossFight) UpdateBoss(dt);
            else if (current.unlocked && player.position.z > current.end - 2 && Mathf.Abs(player.position.x) < 1.8f)
            {
                current = BuildRoom(current.number + 1, current.end);
                audioBus.ChangeRoom(current.number);
                if (current.number == 6) BeginBoss();
                else Notify("OFFICE " + current.number + " / 5  -  KEEP THE COMBO ALIVE!", 3);
            }
            if (player.position.y < -8)
            {
                controller.enabled = false;
                player.position = new Vector3(0, .2f, current.start + 3);
                controller.enabled = true; vertical = 0;
                Hurt(10);
            }
        }

        private void UpdateCamera()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pivot = player.position + Vector3.up * 1.55f;
            Vector3 offset = rotation * new Vector3(.65f, .25f, -3.5f);
            float distance = offset.magnitude;
            // Player visuals have no colliders; ignore the player's controller in camera casts.
            foreach (var hit in Physics.SphereCastAll(pivot, .16f, offset.normalized, distance, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.gameObject != player.gameObject) distance = Mathf.Min(distance, Mathf.Max(.25f, hit.distance - .12f));
            view.transform.position = pivot + offset.normalized * distance;
            view.transform.rotation = rotation;
            shake = Mathf.MoveTowards(shake, 0, Time.deltaTime * 2);
            view.transform.position += Random.insideUnitSphere * shake * .06f;
        }

        private bool AimHit(Vector3 origin, Vector3 direction, float range, out RaycastHit closest)
        {
            closest = default;
            float distance = range + 1;
            foreach (var hit in Physics.RaycastAll(origin, direction, range, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == player.gameObject || hit.distance >= distance) continue;
                closest = hit; distance = hit.distance;
            }
            return distance <= range;
        }

        private void Attack()
        {
            cooldown = cooldowns[weaponIndex]; swing = 1; shake = .8f;
            audioBus.Weapon(weaponIndex);
            Vector3 origin = player.position + Vector3.up * 1.35f;
            Vector3 aimPoint = view.transform.position + view.transform.forward * 55;
            if (AimHit(view.transform.position, view.transform.forward, 55, out var aim)) aimPoint = aim.point;
            Vector3 direction = (aimPoint - origin).normalized;
            if (weaponIndex == 0)
            {
                var targets = new HashSet<OfficeProp>();
                foreach (var col in Physics.OverlapSphere(origin + avatar.forward * 1.3f, 1.65f))
                {
                    var prop = col.GetComponentInParent<OfficeProp>();
                    if (prop == null || !targets.Add(prop)) continue;
                    Vector3 toward = prop.transform.position + Vector3.up - origin;
                    if (Vector3.Dot(avatar.forward, toward.normalized) > .05f &&
                        (!AimHit(origin, toward.normalized, toward.magnitude, out var obstacle) || obstacle.collider.GetComponentInParent<OfficeProp>() == prop))
                        prop.Hit(60, avatar.forward);
                }
                if (boss != null && Vector3.Distance(origin, boss.position + Vector3.up) < 3.4f && Vector3.Dot(avatar.forward, (boss.position - player.position).normalized) > .1f) HitBoss(55);
                if (AimHit(origin, avatar.forward, 2.8f, out var wallHit) && wallHit.collider.GetComponentInParent<OfficeProp>() == null && wallHit.collider.transform != boss)
                    audioBus.Play("wall", .4f, .85f, wallHit.point);
                return;
            }
            if (weaponIndex == 1)
            {
                for (int i = 0; i < 7; i++)
                {
                    Vector3 pellet = (direction + (i == 0 ? Vector3.zero : Random.insideUnitSphere * .055f)).normalized;
                    Vector3 end = origin + pellet * 35;
                    if (AimHit(origin, pellet, 35, out var hit))
                    {
                        end = hit.point;
                        var prop = hit.collider.GetComponentInParent<OfficeProp>();
                        if (prop != null) prop.Hit(18, pellet);
                        if (boss != null && hit.collider.transform == boss) HitBoss(12);
                    }
                    Trail(origin, end, Cyan);
                }
            }
            else
            {
                Vector3 end = origin + direction * 40;
                if (AimHit(origin, direction, 40, out var hit)) end = hit.point;
                Trail(origin, end, Orange);
                var targets = new HashSet<OfficeProp>();
                foreach (var col in Physics.OverlapSphere(end, 3.5f))
                {
                    var prop = col.GetComponentInParent<OfficeProp>();
                    if (prop != null && targets.Add(prop))
                    {
                        Vector3 toProp = prop.transform.position + Vector3.up - (end - direction * .15f);
                        if (!AimHit(end - direction * .15f, toProp.normalized, toProp.magnitude, out var block) || block.collider.GetComponentInParent<OfficeProp>() == prop)
                            prop.Hit(100, (prop.transform.position - end).normalized);
                    }
                }
                if (boss != null && Vector3.Distance(boss.position + Vector3.up, end) < 4) HitBoss(85);
                Impact(end, Orange, -direction, 10);
                audioBus.Play("explosion", .8f, Random.Range(.9f, 1.05f), end);
            }
        }

        private void Trail(Vector3 start, Vector3 end, Color color)
        {
            var line = Shape(world, "Shot trail", (start + end) * .5f, new Vector3(.035f, .035f, Vector3.Distance(start, end)), color, false);
            line.transform.rotation = Quaternion.LookRotation((end - start).normalized);
            Destroy(line, .07f);
        }

        public void Impact(Vector3 position, Color color, Vector3 force, int count)
        {
            for (int i = 0; i < count; i++)
            {
                while (debris.Count >= 70) { var old = debris.Dequeue(); if (old != null) Destroy(old); }
                var part = Shape(world, "Debris", position + Random.insideUnitSphere * .35f, Vector3.one * Random.Range(.1f, .24f), color, false);
                var body = part.AddComponent<Rigidbody>();
                body.linearVelocity = force * Random.Range(2, 5) + Random.onUnitSphere * 3 + Vector3.up * 3;
                body.angularVelocity = Random.insideUnitSphere * 10;
                debris.Enqueue(part);
                Destroy(part, 1.4f);
            }
        }

        public void PropDestroyed(int roomNumber)
        {
            if (ended) return;
            var room = rooms.Find(r => r.number == roomNumber);
            room.destroyed++;
            combo = comboLeft > 0 ? combo + 1 : 1;
            comboLeft = 4;
            score += 100 + Mathf.Min(combo, 20) * 15;
            if (combo % 3 == 0) { remaining += 5; Notify("COMBO " + combo + "   +5 SECONDS", 2); audioBus.Play("combo", .45f); }
            if (!room.unlocked && room.destroyed >= room.required)
            {
                room.unlocked = true;
                room.gate.GetComponent<Collider>().enabled = false;
                room.gateRenderer.sharedMaterial = Mat(Cyan);
                room.gate.transform.localScale = new Vector3(3.3f, .12f, .3f);
                room.gate.transform.position = new Vector3(0, .06f, room.end);
                Notify("EXIT OPEN - FIND THE CYAN DOOR", 3);
                audioBus.Play("door", .45f);
            }
        }

        private void BeginBoss()
        {
            bossFight = true; bossHealth = 900; bossPhaseTime = 1.5f; bossPhase = 0;
            boss = new GameObject("CEO - final boss").transform;
            boss.SetParent(world);
            boss.position = new Vector3(0, 0, current.start + 18);
            BuildPerson(boss, true);
            boss.localScale = Vector3.one * 1.5f;
            var hitbox = boss.gameObject.AddComponent<CapsuleCollider>();
            hitbox.height = 2; hitbox.radius = .5f; hitbox.center = Vector3.up;
            bossWarning = Shape(world, "Charge warning", boss.position, new Vector3(1.7f, .025f, 8), Orange, false).transform;
            bossWarning.gameObject.SetActive(false);
            Notify("FINAL BOSS: THE CEO - DODGE THE ORANGE CHARGE!", 5);
        }

        private void UpdateBoss(float dt)
        {
            if (boss == null) return;
            Vector3 toward = player.position - boss.position; toward.y = 0;
            bossPhaseTime -= dt;
            if (bossPhase == 0)
            {
                if (toward.sqrMagnitude > .01f) boss.rotation = Quaternion.LookRotation(toward);
                if (toward.magnitude > 2.1f) boss.position += toward.normalized * 2.6f * dt;
                if (bossPhaseTime <= 0)
                {
                    bossPhase = 1; bossPhaseTime = .85f; bossChargeDirection = toward.normalized;
                    bossWarning.gameObject.SetActive(true);
                    bossWarning.position = boss.position + bossChargeDirection * 5 + Vector3.up * .03f;
                    bossWarning.rotation = Quaternion.LookRotation(bossChargeDirection.sqrMagnitude > .01f ? bossChargeDirection : Vector3.forward);
                    bossWarning.localScale = new Vector3(1.8f, .025f, 11);
                }
            }
            else if (bossPhase == 1)
            {
                if (bossPhaseTime <= 0) { bossPhase = 2; bossPhaseTime = .65f; bossWarning.gameObject.SetActive(false); }
            }
            else if (bossPhase == 2)
            {
                boss.position += bossChargeDirection * 17 * dt;
                if (bossPhaseTime <= 0) { bossPhase = 3; bossPhaseTime = 1.5f; }
            }
            else if (bossPhaseTime <= 0) { bossPhase = 0; bossPhaseTime = 2; }
            Vector3 p = boss.position;
            p.x = Mathf.Clamp(p.x, -current.width / 2 + 1, current.width / 2 - 1);
            p.z = Mathf.Clamp(p.z, current.start + 2, current.end - 2);
            boss.position = p;
            if (Vector3.Distance(player.position, boss.position) < 2 && bossPhase != 3) Hurt(bossPhase == 2 ? 25 : 12);
            // Crossing the threshold seals the arena; the boss fight stays one-on-one.
            if (player.position.z > current.start + 2 && current.gate.name != "Arena sealed")
            {
                Shape(world, "Arena entry seal", new Vector3(0, 1.7f, current.start), new Vector3(3.4f, 3.4f, .35f), Orange);
                current.gate.name = "Arena sealed";
            }
        }

        private void HitBoss(float damage)
        {
            if (!bossFight || ended || boss == null) return;
            bossHealth = Mathf.Max(0, bossHealth - damage);
            audioBus.Impact(3, false, boss.position + Vector3.up);
            Impact(boss.position + Vector3.up * 1.7f, Orange, -boss.forward, 2);
            if (bossHealth <= 0) { score += 5000; End(true); }
        }

        private void Hurt(float damage)
        {
            if (invulnerability > 0 || ended) return;
            hp = Mathf.Max(0, hp - damage); invulnerability = 1; shake = 2;
            audioBus.Play("thud", .7f, .8f);
            if (hp <= 0) End(false);
        }

        private void Notify(string message, float duration) { notice = message; noticeLeft = duration; }
        public void PlayPropSound(int material, bool broken, Vector3 position) { audioBus.Impact(material, broken, position); }
        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
        private void End(bool victory)
        {
            ended = true; won = victory; LockCursor(false);
            audioBus.Finish(victory);
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && !ended && world != null) { paused = true; Time.timeScale = 0; LockCursor(false); audioBus.SetPaused(true); }
        }
        private void OnDestroy()
        {
            Time.timeScale = originalTimeScale;
            LockCursor(false);
            foreach (var material in materials.Values) if (material != null) Destroy(material);
        }

        private void Box(Rect rect, Color color)
        {
            GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
        }

        private void DrawWeaponSlots()
        {
            GUI.Label(new Rect(842, 570, 416, 28), weaponNames[weaponIndex] + "  /  UNLIMITED", smallStyle);
            string[] captions = { "HAMMER", "SHOTGUN", "ROCKET" };
            for (int i = 0; i < 3; i++)
            {
                float x = 842 + i * 142;
                bool selected = weaponIndex == i;
                Color ink = selected ? Color.white : new Color(.55f, .65f, .7f);
                Box(new Rect(x, 600, 132, 98), selected ? Cyan : new Color(.2f, .28f, .33f));
                Box(new Rect(x + 2, 602, 128, 94), selected ? new Color(.06f, .19f, .24f) : Navy);
                GUI.Label(new Rect(x + 9, 604, 22, 24), (i + 1).ToString(), smallStyle);
                // Vector-like silhouettes stay sharp at every Game view resolution.
                if (i == 0)
                {
                    Box(new Rect(x + 62, 627, 8, 37), Orange);
                    Box(new Rect(x + 43, 615, 46, 18), ink);
                    Box(new Rect(x + 39, 613, 8, 22), ink);
                    Box(new Rect(x + 85, 613, 8, 22), ink);
                    Box(new Rect(x + 61, 651, 10, 14), ink);
                }
                else if (i == 1)
                {
                    Box(new Rect(x + 52, 624, 62, 5), ink);
                    Box(new Rect(x + 52, 631, 62, 5), ink);
                    Box(new Rect(x + 37, 628, 40, 13), ink);
                    Box(new Rect(x + 70, 637, 24, 7), Orange);
                    Box(new Rect(x + 17, 637, 27, 13), Orange);
                    Box(new Rect(x + 41, 638, 10, 21), ink);
                    Box(new Rect(x + 53, 642, 14, 3), ink);
                }
                else
                {
                    Box(new Rect(x + 23, 625, 81, 19), ink);
                    Box(new Rect(x + 18, 621, 9, 27), Orange);
                    Box(new Rect(x + 100, 621, 8, 27), Orange);
                    Box(new Rect(x + 108, 628, 9, 13), ink);
                    Box(new Rect(x + 48, 644, 9, 16), ink);
                    Box(new Rect(x + 78, 644, 8, 13), ink);
                    Box(new Rect(x + 61, 617, 5, 8), ink);
                    Box(new Rect(x + 56, 613, 19, 5), Cyan);
                }
                GUI.Label(new Rect(x + 9, 670, 120, 23), captions[i], smallStyle);
                if (selected) Box(new Rect(x + 2, 694, 128, 4), Cyan);
            }
        }

        private void OnGUI()
        {
            if (world == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold };
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
                smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                centeredStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
                titleStyle.normal.textColor = labelStyle.normal.textColor = smallStyle.normal.textColor = centeredStyle.normal.textColor = Color.white;
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            Box(new Rect(22, 20, 350, 154), new Color(.02f, .04f, .07f, .92f));
            GUI.Label(new Rect(38, 28, 300, 40), "OFFICE / RAMPAGE", titleStyle);
            GUI.Label(new Rect(38, 74, 300, 28), bossFight ? "FINAL FLOOR / THE CEO" : "OFFICE 0" + current.number + " / 05", labelStyle);
            string objective = bossFight ? "Dodge, then strike during recovery" : "DESTROY " + current.destroyed + " / " + current.required + (current.unlocked ? "  |  EXIT OPEN" : "  |  EXIT LOCKED");
            GUI.Label(new Rect(38, 108, 300, 24), objective, smallStyle);
            GUI.Label(new Rect(38, 144, 340, 24), current.layoutName + " / " + current.width + " x " + (current.end - current.start) + " m", smallStyle);
            Box(new Rect(545, 20, 190, 86), remaining < 20 ? new Color(.65f, .1f, .07f, .95f) : Navy);
            GUI.Label(new Rect(555, 22, 170, 28), "TIME REMAINING", centeredStyle);
            int seconds = Mathf.CeilToInt(remaining);
            GUI.Label(new Rect(590, 54, 160, 42), (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00"), titleStyle);
            Box(new Rect(1018, 20, 240, 86), Navy);
            GUI.Label(new Rect(1034, 29, 215, 30), "SCORE  " + score.ToString("N0"), labelStyle);
            GUI.Label(new Rect(1034, 65, 215, 26), "COMBO " + combo + "  /  3 = +5s", smallStyle);
            Box(new Rect(1034, 94, 208 * Mathf.Clamp01(comboLeft / 4), 4), Orange);
            if (noticeLeft > 0 && !ended) GUI.Label(new Rect(260, 185, 760, 40), notice, centeredStyle);
            if (bossFight && !ended)
            {
                Box(new Rect(390, 119, 500, 18), Navy);
                Box(new Rect(390, 119, 500 * Mathf.Clamp01(bossHealth / 900), 18), Orange);
            }
            GUI.Label(new Rect(627, 342, 30, 32), "+", titleStyle);
            Box(new Rect(22, 617, 320, 80), Navy);
            GUI.Label(new Rect(38, 627, 290, 28), "HEALTH  " + Mathf.CeilToInt(hp), labelStyle);
            Box(new Rect(38, 667, 285, 10), new Color(.2f, .25f, .3f));
            Box(new Rect(38, 667, 285 * hp / 100, 10), invulnerability > 0 ? Orange : Cyan);
            DrawWeaponSlots();
            GUI.Label(new Rect(354, 644, 480, 28), "WASD Move / Mouse Aim / LMB Attack / Space Jump", smallStyle);
            GUI.Label(new Rect(375, 674, 460, 24), "Shift Sprint / Esc Pause / R Restart / M Mute", smallStyle);
            if (ended || paused)
            {
                Box(new Rect(0, 0, 1280, 720), new Color(.01f, .025f, .045f, .88f));
                GUI.Label(new Rect(430, 245, 550, 55), paused ? "TAKE A BREATHER" : won ? "YOU'RE THE BOSS NOW." : "BAD DAY AT THE OFFICE.", titleStyle);
                GUI.Label(new Rect(340, 325, 600, 40), paused ? "ESC TO RESUME" : "SCORE " + score.ToString("N0") + "  |  " + (won ? "CEO DEFEATED" : hp <= 0 ? "YOU WERE DEFEATED" : "TIME'S UP"), centeredStyle);
                GUI.Label(new Rect(340, 380, 600, 40), "PRESS R FOR A NEW RANDOM RUN", centeredStyle);
                if (GUI.Button(new Rect(510, 450, 260, 48), paused ? "RESUME" : "PLAY AGAIN"))
                {
                    if (paused) { paused = false; Time.timeScale = 1; LockCursor(true); audioBus.SetPaused(false); }
                    else ResetRun();
                }
                GUI.Label(new Rect(420, 525, 150, 28), "MUSIC", smallStyle);
                audioBus.MusicVolume = GUI.HorizontalSlider(new Rect(560, 534, 270, 20), audioBus.MusicVolume, 0, .6f);
                GUI.Label(new Rect(420, 565, 150, 28), "SOUND EFFECTS", smallStyle);
                audioBus.EffectsVolume = GUI.HorizontalSlider(new Rect(560, 574, 270, 20), audioBus.EffectsVolume, 0, 1);
                GUI.Label(new Rect(420, 608, 430, 28), audioBus.Muted ? "M: SOUND MUTED" : "M: MUTE ALL SOUND", smallStyle);
            }
            GUI.matrix = previous;
        }
    }
}
