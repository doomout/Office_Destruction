using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OfficeRampage
{
    /// <summary>Self-contained prototype. The saved scene contains only this bootstrap.</summary>
    public sealed partial class OfficeRampageGame : MonoBehaviour
    {
        private sealed class Room
        {
            public int number, total, destroyed, required;
            public float start, end, width;
            public GameObject gate;
            public Renderer gateRenderer;
            public OfficeSlidingDoor door;
            public bool unlocked;
            public string layoutName;
        }

        private readonly List<Room> rooms = new List<Room>();
        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        private readonly Queue<GameObject> debris = new Queue<GameObject>();
        private readonly string[] weaponNames = OfficeCombatRules.Weapons;
        private readonly float[] cooldowns = OfficeCombatRules.Cooldowns;
        private Transform world, player, avatar, weapon;
        private CharacterController controller;
        private Camera view;
        private Room current;
        private float remaining, hp, yaw, pitch, vertical, cooldown, comboLeft, noticeLeft;
        private float invulnerability, swing, shake;
        private int weaponIndex, combo, score;
        private bool ended, won, paused, bossFight;
        private string notice = "";
        private GUIStyle titleStyle, labelStyle, smallStyle, centeredStyle;
        private float originalTimeScale;
        private OfficeAudio audioBus;
        private readonly List<Vector3> oilPuddles = new List<Vector3>();
        private bool slowedByOil;
        public bool RunActive { get { return menuPage == MenuPage.Game && !ended && !paused; } }
        private readonly int[] layoutOrder = { 0, 1, 2, 3, 4 };

        private static readonly Color Navy = new Color(.035f, .07f, .12f);
        private static readonly Color Cyan = new Color(.12f, .85f, .86f);
        private static readonly Color Orange = new Color(1f, .36f, .12f);

        private void Start()
        {
            originalTimeScale = Time.timeScale;
            InitializeMenu();
        }

        private void ResetRun()
        {
            StopAllCoroutines(); ResetExpansion();
            runCompany = (companySettings ?? OfficeCompanySettings.CreateDefault()).Copy();
            System.Array.Clear(spawnedEnemies, 0, spawnedEnemies.Length);
            menuPage = MenuPage.Game;
            Time.timeScale = 1;
            if (audioBus == null) audioBus = gameObject.AddComponent<OfficeAudio>();
            audioBus.ResetRun();
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            rooms.Clear();
            defenders.Clear(); bossUnit = null; bossChair = null; finalRoomSealed = false;
            oilPuddles.Clear(); slowedByOil = false;
            debris.Clear();
            world = new GameObject("Generated Office Rampage").transform;
            world.SetParent(transform);
            remaining = OfficeMenuRules.Duration(difficulty); hp = 100; yaw = 0; pitch = 13; vertical = 0;
            score = combo = weaponIndex = 0;
            cooldown = comboLeft = invulnerability = swing = shake = 0;
            ended = won = paused = bossFight = false;
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
            Notify("물건 60% 파괴와 적 60% 처치를 달성하면 출구가 열립니다.", 5);
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
            bool arena = number == 5;
            var layout = arena ? OfficeLayout.CreateExecutive() : OfficeLayout.Create(layoutOrder[number - 1], Random.Range(0, int.MaxValue));
            var room = new Room { number = number, start = start,
                width = layout.width, end = start + layout.length, layoutName = layout.name };
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
            if (!arena) CreateSlidingDoor(room, root);
            else
            {
                room.gate = Shape(root, "Boardroom wall", new Vector3(0, 1.55f, room.end), new Vector3(3.3f, 3.1f, .3f), Orange);
                room.gateRenderer = room.gate.GetComponent<Renderer>();
            }
            var sign = new GameObject("Office sign").AddComponent<TextMesh>();
            sign.transform.SetParent(root);
            sign.transform.position = new Vector3(0, 3.57f, room.end - .26f);
            sign.transform.rotation = Quaternion.identity;
            sign.text = arena ? "대표실" : "사무실 " + number + " / 5";
            sign.anchor = TextAnchor.MiddleCenter;
            sign.characterSize = .16f;
            sign.fontSize = 48;
            sign.color = Color.white;

            if (layout != null)
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
                    SpawnProp(root, room, new Vector3(point.x, 0, start + point.z), point.kind);
                foreach (var patch in layout.oil)
                {
                    var center = new Vector3(patch.x, 0, start + patch.z);
                    oilPuddles.Add(center);
                    Shape(root, "Oil puddle - 50 percent slow", center + Vector3.up * .015f,
                        new Vector3(1.3f, .012f, 1.5f), new Color(.025f, .025f, .04f), false, PrimitiveType.Cylinder);
                    Shape(root, "Oil surface sheen", center + new Vector3(.1f, .03f, .08f),
                        new Vector3(.85f, .003f, .85f), new Color(.14f, .1f, .22f), false, PrimitiveType.Cylinder);
                    var warning = new GameObject("Oil warning").AddComponent<TextMesh>();
                    warning.transform.SetParent(root);
                    warning.transform.position = center + new Vector3(0, .04f, 0);
                    warning.transform.rotation = Quaternion.Euler(90, 0, 0);
                    warning.text = "기름"; warning.anchor = TextAnchor.MiddleCenter;
                    warning.fontSize = 32; warning.characterSize = .11f; warning.color = new Color(1, .75f, .2f);
                }
                room.required = OfficeProgressRules.Required(room.total);
            }
            foreach (Transform part in root)
                if (part.name == "Side wall" || part.name == "Exit wall" || part.name == "Entry wall" || part.name == "Entrance" || part.name == "Window")
                { var renderer = part.GetComponent<Renderer>(); if (renderer != null) renderer.enabled = false; }
            for (int side = -1; side <= 1; side += 2)
                Shape(root, "Floor edge", new Vector3(side * room.width / 2, .025f, middle), new Vector3(.12f, .05f, length), Cyan, false);
            SetupDefenders(room, layout, root);
            SpawnHealingPotions(room, layout);
            ApplyKoreanWorldFont(root);
            return room;
        }

        private void SpawnProp(Transform root, Room room, Vector3 position, int kind)
        {
            string[] names = { "Desk", "Computer - explosive", "Water cooler - heal", "Copier - explosive", "Filing cabinet", "Potted plant" };
            Color wood = new Color(.62f, .39f, .2f);
            Color shell = new Color(.73f, .8f, .8f);
            var prop = new GameObject(names[kind]).AddComponent<OfficeProp>();
            prop.transform.SetParent(root);
            prop.transform.position = position;
            prop.transform.rotation = Quaternion.Euler(0, Random.Range(-12f, 12f), 0);
            prop.game = this; prop.room = room.number;
            prop.kind = kind;
            prop.soundMaterial = kind == OfficeRules.Plant ? 0 : kind;
            prop.health = kind == OfficeRules.Cabinet ? 90 : kind == OfficeRules.Plant ? 25 : kind == 0 ? 55 : 40;
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
            else if (kind == OfficeRules.Copier)
            {
                Shape(t, "Printer cabinet", new Vector3(0, .55f, 0), new Vector3(1.2f, 1.1f, 1), shell);
                Shape(t, "Scanner lid", new Vector3(0, 1.18f, 0), new Vector3(1.3f, .16f, 1.05f), Navy);
                Shape(t, "Paper output", new Vector3(0, .82f, -.54f), new Vector3(.72f, .13f, .12f), Navy);
                Shape(t, "Control panel", new Vector3(.35f, 1.29f, -.26f), new Vector3(.3f, .05f, .24f), Cyan);
                for (int i = 0; i < 2; i++)
                    Shape(t, "Paper drawer", new Vector3(0, .23f + i * .27f, -.51f), new Vector3(.96f, .2f, .04f), new Color(.48f, .57f, .6f));
            }
            else if (kind == OfficeRules.Cabinet)
            {
                Shape(t, "Steel cabinet", new Vector3(0, 1.05f, 0), new Vector3(1.1f, 2.1f, .7f), new Color(.3f, .42f, .46f));
                for (int i = 0; i < 4; i++)
                {
                    Shape(t, "Drawer", new Vector3(0, .28f + i * .5f, -.37f), new Vector3(.98f, .43f, .04f), shell, false);
                    Shape(t, "Drawer handle", new Vector3(0, .3f + i * .5f, -.41f), new Vector3(.32f, .06f, .04f), Navy, false);
                }
            }
            else if (kind == OfficeRules.Plant)
            {
                prop.debrisColor = new Color(.24f, .58f, .18f);
                Shape(t, "Plant pot", new Vector3(0, .3f, 0), new Vector3(.7f, .3f, .7f), new Color(.65f, .29f, .15f), true, PrimitiveType.Cylinder);
                Shape(t, "Stem", new Vector3(0, .9f, 0), new Vector3(.09f, .7f, .09f), wood);
                for (int i = 0; i < 5; i++)
                {
                    float angle = i * Mathf.PI * .4f;
                    Shape(t, "Leaf", new Vector3(Mathf.Sin(angle) * .25f, 1 + (i % 2) * .2f, Mathf.Cos(angle) * .25f),
                        new Vector3(.48f, .42f, .48f), new Color(.2f, .55f + i * .025f, .18f), true, PrimitiveType.Sphere);
                }
            }
            if (OfficeRules.IsElectronic(kind) || kind == OfficeRules.WaterCooler)
            {
                var label = new GameObject("Break effect label").AddComponent<TextMesh>();
                label.transform.SetParent(t, false);
                label.transform.localPosition = new Vector3(0, kind == OfficeRules.WaterCooler ? 2.1f : 1.95f, 0);
                label.text = kind == OfficeRules.WaterCooler ? "체력 +5" : "체력 -5";
                label.anchor = TextAnchor.MiddleCenter; label.fontSize = 36; label.characterSize = .09f;
                label.color = kind == OfficeRules.WaterCooler ? new Color(.2f, 1, .5f) : Orange;
            }
        }

        private bool IsStandingOnOil()
        {
            Vector3 p = player.position;
            foreach (var oil in oilPuddles)
                if (OfficeRules.OnOil(p.x, p.z, p.y, oil.x, oil.z, .65f, .75f)) return true;
            return false;
        }

        private void SetWeapon(int index)
        {
            if (whirlwindLeft > 0) return;
            if (index < 0 || index > 7 || (index == 7 && (!bfgReady || bfgUsed))) return;
            weaponIndex = index;
            for (int i = weapon.childCount - 1; i >= 0; i--)
            { weapon.GetChild(i).gameObject.SetActive(false); Destroy(weapon.GetChild(i).gameObject); }
            weapon.localPosition = new Vector3(.48f, .95f, .25f);
            weapon.localRotation = Quaternion.identity;
            BuildExpandedWeapon();
        }

        private void Update()
        {
            var keys = Keyboard.current;
            if (menuPage != MenuPage.Game)
            {
                if (keys != null && keys.escapeKey.wasPressedThisFrame) menuPage = MenuPage.Main;
                return;
            }
            var mouse = Mouse.current;
            if (keys != null && keys.mKey.wasPressedThisFrame) { audioBus.Muted = !audioBus.Muted; SaveMenuSettings(); }
            if (keys != null && keys.rKey.wasPressedThisFrame) { ResetRun(); return; }
            if (keys != null && keys.escapeKey.wasPressedThisFrame && !ended)
            {
                paused = !paused; Time.timeScale = paused ? 0 : 1; LockCursor(!paused);
                audioBus.SetPaused(paused);
            }
            if (ended || paused) return;
            float dt = Time.deltaTime;
            if (CombatLocked) { UpdateCamera(); return; }
            if (OfficeProgressRules.TimerRuns(current.number))
            {
                remaining = Mathf.Max(0, remaining - dt);
                if (remaining <= 0) { End(false); return; }
            }
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
                if (keys.digit4Key.wasPressedThisFrame) SetWeapon(3);
                if (keys.digit5Key.wasPressedThisFrame) SetWeapon(4);
                if (keys.digit6Key.wasPressedThisFrame) SetWeapon(5);
                if (keys.digit7Key.wasPressedThisFrame) SetWeapon(6);
                if (keys.digit8Key.wasPressedThisFrame && bfgReady && !bfgUsed) SetWeapon(7);
            }
            input = Vector2.ClampMagnitude(input, 1);
            if (controller.isGrounded && vertical < 0) vertical = -2;
            if (keys != null && keys.spaceKey.wasPressedThisFrame && controller.isGrounded) vertical = 7;
            vertical -= 21 * dt;
            float speed = keys != null && keys.leftShiftKey.isPressed ? 8 : 5.6f;
            slowedByOil = IsStandingOnOil();
            speed = OfficeRules.MovementSpeed(speed, slowedByOil);
            Vector3 move = Quaternion.Euler(0, yaw, 0) * new Vector3(input.x, 0, input.y) * speed;
            var collision = controller.Move((move + Vector3.up * vertical) * dt);
            slowedByOil = IsStandingOnOil();
            if ((collision & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            TickWeaponSkills(dt);
            avatar.rotation = Quaternion.Euler(0, yaw + (whirlwindLeft > 0 ? whirlwindAngle : 0), 0);
            swing = Mathf.MoveTowards(swing, 0, dt * 5);
            weapon.localRotation = Quaternion.Euler(weaponIndex == 4 ? swing * 100 - 20 : -swing * 60, -swing * 25, 0);
            UpdateCamera();
            TickDecoy(dt);
            if (mouse != null && (weaponIndex == 4 ? mouse.leftButton.wasPressedThisFrame : mouse.leftButton.isPressed) && Cursor.lockState == CursorLockMode.Locked && cooldown <= 0) Attack();
            if (ended || CombatLocked) return;
            SealFinalRoom();
            if (current.number < 5 && current.unlocked && current.door.IsOpen && player.position.z > current.end - 2 && Mathf.Abs(player.position.x) < 1.8f)
            {
                current = BuildRoom(current.number + 1, current.end);
                audioBus.ChangeRoom(current.number);
                Notify(current.number == 5 ? "경비견 5마리를 모두 처치하세요." : "사무실 " + current.number + " / 5 · 적의 공격을 조심하세요!", 4);
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
            if (endingPlaying) { UpdateEndingCamera(); return; }
            if (mushroomCamera)
            {
                view.fieldOfView = 56;
                view.transform.position = mushroomCenter + new Vector3(-18, 11, -25);
                view.transform.LookAt(mushroomCenter + Vector3.up * 9.5f);
                return;
            }
            view.fieldOfView = 72;
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

        private void Attack() { AttackExpanded(); }

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

        public void PropDestroyed(int roomNumber, int kind, Vector3 position)
        {
            if (ended) return;
            var room = rooms.Find(r => r.number == roomNumber);
            room.destroyed++;
            combo = comboLeft > 0 ? combo + 1 : 1;
            comboLeft = 4;
            ChargeWhirlwind();
            score += 100 + Mathf.Min(combo, 20) * 15;
            if (combo % 3 == 0) { remaining += 5; Notify("콤보 " + combo + " · 제한시간 +5초", 2); audioBus.Play("combo", .45f); }
            RefreshExit(room);
            float previousHealth = hp;
            hp = OfficeRules.HealthAfterBreak(hp, kind);
            if (OfficeRules.IsElectronic(kind))
            {
                // Per-object cost, independent of range and the boss-contact invulnerability timer.
                // This visual explosion does not recursively damage other props.
                Impact(position, Orange, Vector3.up, 10);
                audioBus.Play("explosion", .55f, Random.Range(.95f, 1.1f), position);
                var flash = Shape(world, "Electronic explosion", position, Vector3.one * 1.6f, Orange, false, PrimitiveType.Sphere);
                Destroy(flash, .12f);
                shake = 1.5f;
                Notify("전자기기 폭발! 체력 -5", 1.8f);
                if (hp <= 0) End(false);
            }
            else if (kind == OfficeRules.WaterCooler)
            {
                Impact(position, new Color(.2f, .9f, .6f), Vector3.up, 7);
                audioBus.Play("combo", .4f, 1.25f);
                Notify(hp > previousHealth ? "정수기 · 체력 +" + (hp - previousHealth).ToString("0") + " 체력" : "정수기 · 체력이 가득 찼습니다.", 1.8f);
            }
        }

        private void Hurt(float damage)
        {
            if (invulnerability > 0 || ended || CombatLocked) return;
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
            if (!focus && menuPage == MenuPage.Game && !ended && world != null) { paused = true; Time.timeScale = 0; LockCursor(false); audioBus.SetPaused(true); }
        }
        private void OnDestroy()
        {
            SaveMenuSettings();
            if (menuFont != null) Destroy(menuFont);
            Time.timeScale = originalTimeScale;
            LockCursor(false);
            foreach (var material in materials.Values) if (material != null) Destroy(material);
            if (defenderFont != null) Destroy(defenderFont);
        }

        private void Box(Rect rect, Color color)
        {
            GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
        }

        private void DrawWeaponSlots()
        {
            EnsureMenuStyles();
            GUI.Label(new Rect(345, 540, 900, 38), weaponNames[weaponIndex] + (weaponIndex == 7 ? " / 1발" : weaponIndex == 4 ? " / 남은 횟수 " + (2-decoysUsed[current.number]) : " / 무제한"), menuText);
            for (int i = 0; i < 8; i++)
            {
                float x = 345 + i * 114;
                bool locked = i == 7 && (!bfgReady || bfgUsed);
                Box(new Rect(x, 583, 108, 54), i == weaponIndex ? new Color(.08f, .42f, .43f) : Navy);
                GUI.Label(new Rect(x + 4, 587, 104, 45), (i + 1) + " " + (locked ? (bfgUsed ? "사용완료" : "잠김") : weaponNames[i]), menuSmall);
            }
        }

        private void OnGUI()
        {
            if (menuPage != MenuPage.Game) { DrawFrontEnd(); return; }
            if (world == null) return;
            EnsureMenuStyles();
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { font = menuFont, fontSize = 30, fontStyle = FontStyle.Bold };
                labelStyle = new GUIStyle(GUI.skin.label) { font = menuFont, fontSize = 19, fontStyle = FontStyle.Bold };
                smallStyle = new GUIStyle(GUI.skin.label) { font = menuFont, fontSize = 14 };
                centeredStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
                titleStyle.normal.textColor = labelStyle.normal.textColor = smallStyle.normal.textColor = centeredStyle.normal.textColor = Color.white;
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            if (endingPlaying || (ended && won)) { DrawEndingScreen(); GUI.matrix = previous; return; }
            Box(new Rect(22, 20, 350, 182), new Color(.02f, .04f, .07f, .92f));
            GUI.Label(new Rect(38, 28, 300, 40), "사무실 파괴", titleStyle);
            GUI.Label(new Rect(38, 74, 300, 28), bossFight ? "대표 사무실" : "사무실 " + current.number + " / 5", labelStyle);
            string objective = current.number == 5 ? (bossFight ? "대표 체력 " + (bossUnit != null ? Mathf.CeilToInt(bossUnit.Health) : 0) : "남은 호위: " + LivingDefenders(5) + " / " + EnemyTotal(5)) : "파괴 " + current.destroyed + " / " + current.required + (current.unlocked ? " · 출구 열림" : " · 출구 잠김");
            GUI.Label(new Rect(38, 108, 300, 24), objective, smallStyle);
            GUI.Label(new Rect(38, 144, 340, 24), current.layoutName + " / " + current.width + " x " + (current.end - current.start) + " 미터", smallStyle);
            if (current.number < 5) GUI.Label(new Rect(38, 177, 320, 24), "처치 " + (EnemyTotal(current.number) - LivingDefenders(current.number)) + " / " + OfficeProgressRules.Required(EnemyTotal(current.number)) + " 필요", smallStyle);
            Box(new Rect(545, 20, 190, 86), OfficeProgressRules.TimerRuns(current.number) && remaining < 20 ? new Color(.65f, .1f, .07f, .95f) : Navy);
            GUI.Label(new Rect(555, 22, 170, 28), OfficeProgressRules.TimerRuns(current.number) ? "남은 시간" : "대표전", centeredStyle);
            int seconds = Mathf.CeilToInt(remaining);
            GUI.Label(new Rect(555, 54, 190, 42), OfficeProgressRules.TimerRuns(current.number) ? (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00") : "시간 무제한", titleStyle);
            Box(new Rect(1018, 20, 240, 86), Navy);
            GUI.Label(new Rect(1034, 29, 215, 30), "점수  " + score.ToString("N0"), labelStyle);
            GUI.Label(new Rect(1034, 65, 215, 26), "콤보 " + combo + " / 3회마다 +5초", smallStyle);
            Box(new Rect(1034, 94, 208 * Mathf.Clamp01(comboLeft / 4), 4), Orange);
            if (noticeLeft > 0 && !ended) GUI.Label(new Rect(260, 205, 760, 40), notice, centeredStyle);
            if (bossFight && !ended)
            {
                Box(new Rect(390, 119, 500, 18), Navy);
                Box(new Rect(390, 119, 500 * Mathf.Clamp01(bossUnit != null ? bossUnit.Health / bossUnit.MaxHealth : 0), 18), Orange);
            }
            GUI.Label(new Rect(627, 342, 30, 32), "+", titleStyle);
            Box(new Rect(22, 617, 320, 80), Navy);
            GUI.Label(new Rect(38, 627, 290, 28), "체력  " + Mathf.CeilToInt(hp), labelStyle);
            if (slowedByOil && !ended) GUI.Label(new Rect(38, 578, 300, 32), "기름! 이동속도 50%", labelStyle);
            Box(new Rect(38, 667, 285, 10), new Color(.2f, .25f, .3f));
            Box(new Rect(38, 667, 285 * hp / 100, 10), invulnerability > 0 ? Orange : Cyan);
            DrawWeaponSlots();
            GUI.Label(new Rect(354, 644, 480, 28), "WASD 이동 / 마우스 조준 / 왼쪽 클릭 공격 / 스페이스 점프", smallStyle);
            GUI.Label(new Rect(375, 674, 800, 24), "1~8 무기 / 시프트 달리기 / Esc 일시정지 / R 재시작 / M 음소거", smallStyle);
            DrawExpansionOverlay();
            if (ended || paused)
            {
                Box(new Rect(0, 0, 1280, 720), new Color(.01f, .025f, .045f, .88f));
                GUI.Label(new Rect(430, 245, 550, 55), paused ? "잠깐 쉬어가세요" : won ? "대표를 쓰러뜨렸습니다!" : "오늘은 여기까지…", titleStyle);
                GUI.Label(new Rect(340, 325, 600, 40), paused ? "Esc를 누르면 계속합니다." : "점수 " + score.ToString("N0") + "  |  " + (won ? "대표 처치" : hp <= 0 ? "체력 소진" : "시간 초과"), centeredStyle);
                GUI.Label(new Rect(340, 380, 600, 40), "R을 누르면 첫 사무실부터 다시 시작합니다.", centeredStyle);
                if (MenuButton(new Rect(510, 450, 260, 48), paused ? "계속하기" : "재게임"))
                {
                    if (paused) { paused = false; Time.timeScale = 1; LockCursor(true); audioBus.SetPaused(false); }
                    else ResetRun();
                }
                DrawReturnToMenu();
                GUI.changed = false;
                GUI.Label(new Rect(420, 525, 150, 28), "음악", smallStyle);
                audioBus.MusicVolume = GUI.HorizontalSlider(new Rect(560, 534, 270, 20), audioBus.MusicVolume, 0, .6f);
                GUI.Label(new Rect(420, 565, 150, 28), "효과음", smallStyle);
                audioBus.EffectsVolume = GUI.HorizontalSlider(new Rect(560, 574, 270, 20), audioBus.EffectsVolume, 0, 1);
                GUI.Label(new Rect(420, 608, 430, 28), audioBus.Muted ? "M: 소리 꺼짐" : "M: 전체 소리 끄기", smallStyle);
                if (GUI.changed) SaveMenuSettings();
            }
            GUI.matrix = previous;
        }
    }
}
