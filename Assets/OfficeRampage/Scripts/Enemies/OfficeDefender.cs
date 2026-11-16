using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender : MonoBehaviour
    {
        public OfficeRampageGame game;
        [System.NonSerialized] public OfficeNavigation navigation;
        public int room;
        public float roomStart, roomEnd, roomWidth;
        public float patrolStart=-1,patrolEnd=-1;
        public bool dog, boss, guard, female;
        public int companyRank = -1;
        public int securityWeapon; // 0 baton, 3 pistol, 4 long-range shotgun
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool Dormant { get; private set; }
        public bool Enraged { get; private set; }
        public bool PhaseTwo { get; private set; }
        public bool Alive { get { return Health > 0; } }
        public Vector3 AimPoint { get { return transform.position + Vector3.up * (dog ? .48f : PhaseTwo ? 3.5f : 1.15f); } }
        public Transform visual, warning, flames;
        public TextMesh label;
        public string displayName;
        private CharacterController body;
        private readonly System.Random random = new System.Random();
        private List<OfficeLayout.Point> path;
        private int waypoint, attackNumber, pendingAttack = -1;
        private float chooseTime, attackClock, windup, recovery, rising;
        private Vector3 lockedDirection;
        public float RangeScale { get { return Enraged ? 1.5f : 1; } }
        public void Initialize(float health, bool seated)
        {
            Health = MaxHealth = health; Dormant = seated;
            body = gameObject.AddComponent<CharacterController>();
            body.radius = .3f; body.height = dog ? .9f : boss ? 2.3f : 1.85f;
            body.center = Vector3.up * body.height * .5f;
            body.stepOffset = .15f; body.skinWidth = .04f;
            attackClock = .8f + (float)random.NextDouble() * 2;
            UpdateLabel(); if (seated) game.SetBossPose(this, 0);
        }
        public void Wake()
        {
            if (!boss || !Dormant || !Alive) return;
            Dormant = false; rising = 1; UpdateLabel();
        }
        public void Hit(float amount)
        {
            if (!Alive || Dormant || !game.RunActive || game.CombatLocked || !game.IsCurrentRoom(room)) return;
            Health = boss ? OfficeCombatRules.DamageResult(Health, amount, MaxHealth, PhaseTwo) : Mathf.Max(0, Health - amount);
            if(boss && PhaseTwo && !game.OrbitalRequested) Health=Mathf.Max(MaxHealth*.2f,Health);
            game.DefenderImpact(this);
            if (boss && !Enraged && OfficeCombatRules.Rage(Health, MaxHealth))
            { Enraged = true; if (flames != null) flames.gameObject.SetActive(true); game.BossRaged(); }
            if (boss && !PhaseTwo && OfficeCombatRules.Gift(Health, MaxHealth)) game.RequestSecretWeapon();
            if(boss && PhaseTwo && Health<=MaxHealth*.2f) game.RequestOrbitalWeapon();
            UpdateLabel();
            if (!Alive)
            {
                body.enabled = false; if (warning != null) warning.gameObject.SetActive(false);
                if (boss) { game.DefenderDefeated(this); return; }
                gameObject.SetActive(false); game.DefenderDefeated(this); Destroy(gameObject);
            }
        }
        public void EnterPhaseTwo(OfficeNavigation newNavigation, float width, float end)
        {
            PhaseTwo = true; Health = OfficeCombatRules.PhaseTwoHealth(Health, MaxHealth); MaxHealth = Health;
            navigation = newNavigation; roomWidth = width; roomEnd = end; patrolStart=patrolEnd=-1;
            path = null; pendingAttack = -1; recovery = 0; attackClock = 2;
            if (warning != null) warning.gameObject.SetActive(false);
            displayName += " / 거대 로봇 · 2페이즈";
            body.height=6.6f;body.radius=1.05f;body.center=Vector3.up*3.3f;
            if(label!=null) label.transform.localPosition=Vector3.up*7;
            UpdateLabel();
        }
        public void Teleport(Vector3 position)
        { body.enabled = false; transform.position = position; body.enabled = true; path = null; }
        private void UpdateLabel()
        {
            if (label != null) label.text = displayName + "  " + Mathf.CeilToInt(Health) + " / " + MaxHealth + (Dormant ? "\n호위 보호 중" : Enraged ? "\n분노 +50%" : "");
        }
        private void Update()
        {
            if (label != null && Camera.main != null) label.transform.rotation = Camera.main.transform.rotation;
            if (!game.RunActive || game.CombatLocked || !game.IsCurrentRoom(room) || !Alive || Dormant) return;
            float dt = Time.deltaTime;
            bool frozen=TickFrozen(dt), stunned=TickStun(dt);
            if(frozen || stunned) return;
            if (rising > 0) { rising = Mathf.Max(0, rising - dt); game.SetBossPose(this, 1 - rising); return; }
            if (TickDistraction(dt)) return;
            Vector3 toward = game.PlayerPosition - transform.position; toward.y = 0;
            if(PhaseTwo) { TickRobot(dt,toward); return; }
            if (recovery > 0) { recovery -= dt; game.AnimateDefenderAttack(this, 1 - recovery / .35f); return; }
            if (pendingAttack >= 0)
            {
                windup -= dt; game.AnimateDefenderAttack(this, Mathf.Clamp01(1 - windup / .8f));
                if (windup <= 0)
                {
                    if (warning != null) warning.gameObject.SetActive(false);
                    game.ReleaseDefenderAttack(this, pendingAttack, lockedDirection);
                    pendingAttack = -1; recovery = .35f; attackClock = boss ? 1.1f : dog ? .9f : 1.8f + (float)random.NextDouble();
                }
                return;
            }
            game.AnimateDefenderAttack(this, 0); attackClock -= dt;
            int kind = guard ? securityWeapon : dog ? 0 : (boss || companyRank == OfficeCompanySettings.RepresentativeRank) ? attackNumber % 3 : companyRank <= 2 ? 1 : companyRank <= 5 ? 2 : 1 + attackNumber % 2;
            float range = (kind == 3 ? 12f : kind == 4 ? 22f : kind == 0 ? 2.4f : kind == 1 ? 6f : 16f) * RangeScale;
            if (attackClock <= 0 && toward.magnitude <= range && game.ClearDefenseSight(AimPoint, game.PlayerPosition + Vector3.up, transform, null))
            {
                pendingAttack = kind; attackNumber++; windup = kind == 2 ? .9f : .8f;
                lockedDirection = toward.sqrMagnitude > .01f ? toward.normalized : transform.forward;
                transform.rotation = Quaternion.LookRotation(lockedDirection);
                game.ShowDefenderWarning(this, kind, range, lockedDirection); return;
            }
            Patrol(dt, toward);
        }
        private void Patrol(float dt, Vector3 toward)
        {
            float speed = (dog ? 6f : boss ? 3.6f : guard ? 2.8f : 2.4f) * (Enraged ? 1.5f : 1);
            if(guard && securityWeapon>=3 && toward.magnitude<(securityWeapon==3?9:17) && game.ClearDefenseSight(AimPoint,game.PlayerPosition+Vector3.up,transform,null)) return;
            if (toward.magnitude > 1.3f && game.ClearDefenseSight(AimPoint, game.PlayerPosition + Vector3.up, transform, null))
            { Move(toward.normalized * speed, dt); return; }
            chooseTime -= dt;
            if (path == null || waypoint >= path.Count || chooseTime <= 0)
            {
                var target = toward.magnitude > 2 ? new OfficeLayout.Point(game.PlayerPosition.x, game.PlayerPosition.z - roomStart) : navigation.RandomPoint(random);
                path = navigation.Path(transform.position.x, transform.position.z - roomStart, target.x, target.z);
                waypoint = 0; chooseTime = 1.5f + (float)random.NextDouble();
            }
            if (waypoint >= path.Count) return;
            Vector3 delta = new Vector3(path[waypoint].x, transform.position.y, roomStart + path[waypoint].z) - transform.position;
            if (delta.magnitude < .15f) { waypoint++; return; }
            Move(delta.normalized * Mathf.Min(speed, delta.magnitude / Mathf.Max(dt, .001f)), dt);
        }
        private void Move(Vector3 velocity, float dt)
        {
            Vector3 next = transform.position + velocity * dt;
            next.x = Mathf.Clamp(next.x, -roomWidth / 2 + (PhaseTwo?1.3f:.5f), roomWidth / 2 - (PhaseTwo?1.3f:.5f));
            next.z = Mathf.Clamp(next.z, patrolStart>=0?patrolStart:roomStart + .7f, patrolEnd>=0?patrolEnd:roomEnd - .7f);
            body.Move(next - transform.position + Vector3.down * 3 * dt);
            if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), dt * 12);
        }
    }
}
