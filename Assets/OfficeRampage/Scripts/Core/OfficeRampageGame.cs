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
        private float hp, yaw, pitch, vertical, cooldown, comboLeft, noticeLeft;
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
            ResetStoryProgression(); ResetStoryObjectives(); ResetEndlessState(); StopAllCoroutines(); submissionPlaying=submissionEnded=false; ResetExpansion(); ResetSprint();
            if (nameEditor != null) nameEditor.Close();
            runCompany = (companySettings ?? OfficeCompanySettings.CreateDefault()).Copy();
            System.Array.Clear(spawnedEnemies, 0, spawnedEnemies.Length);
            menuPage = MenuPage.Game;
            Time.timeScale = 1;
            if (audioBus == null) audioBus = gameObject.AddComponent<OfficeAudio>();
            audioBus.ResetRun();
            if (world != null) { world.gameObject.SetActive(false); Destroy(world.gameObject); }
            rooms.Clear();
            defenders.Clear(); bossUnit = null; bossChair = null; finalRoomSealed = false; securityWaveStarted=false; securityWavePending=false;
            oilPuddles.Clear(); slowedByOil = false;
            debris.Clear();
            world = new GameObject("Generated Office Rampage").transform;
            world.SetParent(transform);
            hp = 100; yaw = 0; pitch = 13; vertical = 0;
            score = combo = weaponIndex = 0;
            cooldown = comboLeft = invulnerability = swing = shake = 0;
            ended = won = paused = bossFight = false;
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
            current = endlessMode?BuildEndlessArena():BuildRoom(1, 0); ApplyFloorEntry();
            Notify(endlessMode?"무한 파괴 시작! 바닥 장비 앞에서 E로 줍기":"1층 로비 · 보안 단말기에서 E로 퇴근을 요청하세요.", 5);
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
            OfficeCharacterVisual.Attach(root,this,isBoss);
        }

        private Room BuildRoom(int number, float start)
        {
            var prefab=Resources.Load<OfficeFloorPrefab>("OfficeFloors/Floor"+number);
            if(prefab!=null)return BuildPrefabRoom(number,start,prefab);
            activeFloor=null;
            bool arena = number == OfficeStoryFloors.Boss;
            var layout = OfficeFixedFloor.Load(number).CreateLayout();
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
            Shape(root, "Entrance", new Vector3(0, 2, start), new Vector3(room.width, 4, .3f), Navy);
            Shape(root, "Exit lintel", new Vector3(0, 3.65f, room.end), new Vector3(3.4f, .7f, .4f), Cyan);
            if (!arena) { CreateSlidingDoor(room, root); BuildElevatorCab(root, room); }
            else
            {
                room.gate = Shape(root, "Boardroom wall", new Vector3(0, 1.55f, room.end), new Vector3(3.3f, 3.1f, .3f), Orange);
                room.gateRenderer = room.gate.GetComponent<Renderer>();
            }
            var sign = new GameObject("Office sign").AddComponent<TextMesh>(); OfficeGameFont.Apply(sign);
            sign.transform.SetParent(root);
            sign.transform.position = new Vector3(0, 3.57f, room.end - .26f);
            sign.transform.rotation = Quaternion.identity;
            sign.text = arena ? "대표실" : number + "층 · 엘리베이터";
            sign.anchor = TextAnchor.MiddleCenter;
            sign.characterSize = arena ? .16f : .10f;
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
                    SpawnProp(root, room, new Vector3(point.x, 0, start + point.z), point.kind, point.yaw);
                foreach (var patch in layout.oil)
                {
                    var center = new Vector3(patch.x, 0, start + patch.z);
                    oilPuddles.Add(center);
                    Shape(root, "Oil puddle - 50 percent slow", center + Vector3.up * .015f,
                        new Vector3(1.3f, .012f, 1.5f), new Color(.025f, .025f, .04f), false, PrimitiveType.Cylinder);
                    Shape(root, "Oil surface sheen", center + new Vector3(.1f, .03f, .08f),
                        new Vector3(.85f, .003f, .85f), new Color(.14f, .1f, .22f), false, PrimitiveType.Cylinder);
                    var warning = new GameObject("Oil warning").AddComponent<TextMesh>(); OfficeGameFont.Apply(warning);
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
            BuildOfficeShell(room,root);
            BuildFloorIdentity(room,root);
            SetupDefenders(room, layout, root);
            SpawnHealingPotions(room, layout);
            ApplyKoreanWorldFont(root);
            return room;
        }

        private void SpawnProp(Transform root, Room room, Vector3 position, int kind, float rotation = 0)
        {
            string[] names = { "Desk", "Computer - explosive", "Water cooler - heal", "Copier - explosive", "Filing cabinet", "Potted plant" };
            Color wood = new Color(.62f, .39f, .2f);
            Color shell = new Color(.73f, .8f, .8f);
            var prop = new GameObject(names[kind]).AddComponent<OfficeProp>();
            prop.transform.SetParent(root);
            prop.transform.position = position;
            prop.transform.rotation = Quaternion.Euler(0, rotation, 0);
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
                var label = new GameObject("Break effect label").AddComponent<TextMesh>(); OfficeGameFont.Apply(label);
                label.transform.SetParent(t, false);
                label.transform.localPosition = new Vector3(0, kind == OfficeRules.WaterCooler ? 2.1f : 1.95f, 0);
                label.text = kind == OfficeRules.WaterCooler ? "체력 +5" : endlessMode?"폭발 · 적 피해":"체력 -5";
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
            if(endlessMode&&!endlessEquipAllowed)return;
            if (whirlwindLeft > 0 || thunderLeft>0) return;
            if (index < 0 || index > 7 || (index == 7 && (!bfgReady || bfgUsed) && !orbitalReady)) return;
            weapon.localScale=Vector3.one;
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
                if(menuPage==MenuPage.Intro) TickPrologue(Time.unscaledDeltaTime);
                if (nameEditor != null && nameEditor.Active) { if (keys != null && keys.escapeKey.wasPressedThisFrame) nameEditor.Close(); return; }
                if (keys != null && keys.escapeKey.wasPressedThisFrame) { if(menuPage==MenuPage.Intro) ReturnToMainMenu(); else menuPage = MenuPage.Main; }
                return;
            }
            if(endlessMode&&ended&&nameEditor!=null&&nameEditor.Active)return;
            var mouse = Mouse.current;
            if (keys != null && keys.mKey.wasPressedThisFrame) { audioBus.Muted = !audioBus.Muted; SaveMenuSettings(); }
            if (keys != null && keys.escapeKey.wasPressedThisFrame && !ended)
            {
                paused = !paused; SyncStoryPause();
            }
            if (ended || paused) return;
            float dt = Time.deltaTime;
            TickStoryCinematicInput(dt);
            if(meteorActive){TickMeteor(dt);UpdateCamera();return;}
            if(endlessMode)TickEndless(dt);else TickOrbitalReward(dt);
            if (CombatLocked) { UpdateCamera(); return; }
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
                if (keys.digit8Key.wasPressedThisFrame && ((bfgReady && !bfgUsed) || orbitalReady)) SetWeapon(7);
            }
            input = Vector2.ClampMagnitude(input, 1);
            if (controller.isGrounded && vertical < 0) vertical = -2;
            if (keys != null && keys.spaceKey.wasPressedThisFrame && controller.isGrounded) vertical = 7;
            vertical -= 21 * dt;
            float speed = SprintSpeed(keys != null && keys.leftShiftKey.isPressed, input.sqrMagnitude > .01f, dt);
            slowedByOil = IsStandingOnOil();
            speed = OfficeRules.MovementSpeed(speed, slowedByOil);
            Vector3 move = Quaternion.Euler(0, yaw, 0) * new Vector3(input.x, 0, input.y) * speed;
            if(EndlessMechActive&&mechDashLeft>0)move=Vector3.zero;
            Vector3 beforeMove = player.position;
            var collision = controller.Move((move + Vector3.up * vertical) * dt);
            slowedByOil = IsStandingOnOil();
            if ((collision & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            AnimatePlayerRun(dt, player.position - beforeMove);
            TickWeaponSkills(dt);
            avatar.rotation = Quaternion.Euler(0, yaw + (whirlwindLeft > 0 ? whirlwindAngle : 0), 0);
            if(EndlessMechActive)endlessMechRoot.rotation=Quaternion.Euler(0,yaw,0);
            swing = Mathf.MoveTowards(swing, 0, dt * 5);
            UpdateWeaponSwingPose();
            TickEndlessGunPose(dt);
            UpdateCamera();
            TickDecoy(dt);
            if (mouse != null && (weaponIndex == 4 || (weaponIndex==7 && orbitalReady) ? mouse.leftButton.wasPressedThisFrame : mouse.leftButton.isPressed) && Cursor.lockState == CursorLockMode.Locked && cooldown <= 0) Attack();
            if (ended || CombatLocked) return;
            if(endlessMode)
            {
                if(player.position.y < -8){controller.enabled=false;player.position=new Vector3(0,.2f,32);controller.enabled=true;vertical=0;Hurt(10);}
                return;
            }
            TickStoryObjectives(dt);
            SealFinalRoom();
            if (current.number < OfficeStoryFloors.Boss && current.unlocked && current.door.IsOpen && InsideElevator())
                StartCoroutine(RideElevator());
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
            if(finaleCamera) return;
            if (submissionPlaying || submissionEnded) { UpdateSubmissionCamera(); return; }
            if (endingPlaying) { UpdateEndingCamera(); return; }
            if (mushroomCamera)
            {
                view.fieldOfView = 56;
                view.transform.position = mushroomCenter + new Vector3(-18, 11, -25);
                view.transform.LookAt(mushroomCenter + Vector3.up * 9.5f);
                return;
            }
            if(endlessMode&&earthquakeLeft>0){UpdateEarthquakeCamera();return;}
            if(endlessMode&&ufoLeft>0){UpdateUFOCamera();return;}
            bool airView=endlessMode&&(droneLeft>0||jetLeft>0);
            view.fieldOfView = airView?80:72;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pivot = player.position + Vector3.up * (EndlessMechActive?4.4f:airView?(jetLeft>0?5:3):1.55f);
            Vector3 offset = rotation * (EndlessMechActive?new Vector3(1.4f,1,-14):airView?new Vector3(.65f,1,jetLeft>0?-20:-12):endlessMode?new Vector3(.65f,.6f,-8):new Vector3(.65f,.25f,-3.5f));
            float distance = offset.magnitude;
            // Player visuals have no colliders; ignore the player's controller in camera casts.
            foreach (var hit in Physics.SphereCastAll(pivot, .16f, offset.normalized, distance, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.gameObject != player.gameObject && !(endlessMode && hit.collider.GetComponentInParent<OfficeDefender>()!=null)) distance = Mathf.Min(distance, Mathf.Max(.25f, hit.distance - .12f));
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
            if(endlessMode){EndlessPropDestroyed(kind,position);return;}
            var room = rooms.Find(r => r.number == roomNumber);
            room.destroyed++;
            score += 100;
            if (skillDamageDepth==0)
            {
                combo = comboLeft > 0 ? combo + 1 : 1; comboLeft=4;
                ChargeWhirlwind(); ChargeElements(); score+=Mathf.Min(combo,20)*15;
                if(combo%3==0) { Notify("콤보 "+combo+" · 스킬 충전!",2); audioBus.Play("combo",.45f); }
            }
            RefreshExit(room);
            float previousHealth = hp;
            hp = kind==OfficeRules.WaterCooler?Mathf.Min(StoryMaxHealth,hp+5):OfficeRules.HealthAfterBreak(hp, kind);
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
            if(endlessMode)damage=AbsorbEndlessDamage(damage);
            hp = Mathf.Max(0, hp - damage); invulnerability = 1; shake = 2;
            audioBus.Play("thud", .7f, .8f);
            if (hp <= 0) End(false);
        }

        private void Notify(string message, float duration) { notice = message; noticeLeft = duration; SetCompactNotice(message); }
        public void PlayPropSound(int material, bool broken, Vector3 position) { audioBus.Impact(material, broken, position); }
        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
        private void End(bool victory)
        {
            if(ended)return;
            if(endlessMode)SaveEndlessRecord();
            ended = true; won = victory; LockCursor(false);
            audioBus.Finish(victory);
            if(endlessMode){ResetAirSupport();ClearDoubleDamageVisual();PrepareEndlessRank();}
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && menuPage == MenuPage.Game && !ended && world != null) { paused = true; Time.timeScale = 0; LockCursor(false); audioBus.SetPaused(true); }
        }
        private void OnDestroy()
        {
            SaveMenuSettings();
            // menuFont is a shared project asset; do not destroy it.
            Time.timeScale = originalTimeScale;
            LockCursor(false);
            foreach (var material in materials.Values) if (material != null) Destroy(material);
            // defenderFont is the same shared project asset.
            if(endlessEnergyMaterial!=null)Destroy(endlessEnergyMaterial);
            foreach(var mat in endlessGlowMaterials.Values)if(mat!=null)Destroy(mat);endlessGlowMaterials.Clear();
        }

        private void Box(Rect rect, Color color)
        {
            GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
        }

        private void OnGUI()
        {
            if (menuPage != MenuPage.Game) { DrawFrontEnd(); return; }
            if (world == null) return;
            if(finaleCamera && !paused) {DrawStoryCinematicScreen();return;}
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
            if(storyRewardOpen&&!paused){DrawElevatorReward();GUI.matrix=previous;return;}
            if(endlessMode){DrawEndlessHUD();GUI.matrix=previous;return;}
            if (submissionPlaying || submissionEnded) { DrawSubmission(); GUI.matrix=previous; return; }
            if (endingPlaying || (ended && won)) { DrawEndingScreen(); GUI.matrix = previous; return; }
            DrawStoryTopStrip();
            DrawCompactNotice(); DrawStoryObjectives(); DrawStorySkipButton();
            GUI.Label(new Rect(627, 342, 30, 32), "+", titleStyle);
            Box(new Rect(22, 617, 320, 80), Navy);
            GUI.Label(new Rect(38, 627, 290, 28), "체력  " + Mathf.CeilToInt(hp)+" / "+StoryMaxHealth, labelStyle);
            Box(new Rect(38, 667, 285, 10), new Color(.2f, .25f, .3f));
            Box(new Rect(38, 667, 285 * Mathf.Clamp01(hp / StoryMaxHealth), 10), invulnerability > 0 ? Orange : Cyan);
            DrawStoryWeaponCard(); DrawSprintHUD();
            DrawExpansionOverlay();
            if (ended || paused)
            {
                Box(new Rect(0, 0, 1280, 720), new Color(.01f, .025f, .045f, .88f));
                GUI.Label(new Rect(430, 245, 550, 55), paused ? "잠깐 쉬어가세요" : won ? "대표를 쓰러뜨렸습니다!" : "오늘은 여기까지…", titleStyle);
                GUI.Label(new Rect(340, 325, 600, 40), paused ? "Esc를 누르면 계속합니다." : "점수 " + score.ToString("N0") + "  |  " + (won ? "대표 처치" : "체력 소진"), centeredStyle);
                GUI.Label(new Rect(340, 380, 600, 40), paused ? "계속하기 또는 시작 메뉴로 이동할 수 있습니다." : "재게임 버튼으로 첫 사무실부터 시작합니다.", centeredStyle);
                if (MenuButton(new Rect(510, 450, 260, 48), paused ? "계속하기" : "재게임"))
                {
                    if (paused) { paused = false; SyncStoryPause(); }
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
