using System.Collections.Generic;
using UnityEngine;

namespace OfficeRampage
{
    public sealed class OfficeDefender : MonoBehaviour
    {
        public OfficeRampageGame game;
        public OfficeNavigation navigation;
        public int room;
        public float roomStart, roomEnd, roomWidth;
        public bool dog, boss, guard;
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool Dormant { get; private set; }
        public bool Alive { get { return Health > 0; } }
        public Vector3 AimPoint { get { return transform.position + Vector3.up * (dog ? .48f : 1.15f); } }
        public Transform visual, warning, flames;
        public TextMesh label;
        public string displayName;
        private CharacterController body;
        private readonly System.Random random = new System.Random(System.Guid.NewGuid().GetHashCode());
        private List<OfficeLayout.Point> path;
        private int waypoint, phase;
        private float chooseTime, phaseTime = 2, rising;
        private Vector3 chargeDirection;

        public void Initialize(float health, bool seated)
        {
            Health = MaxHealth = health; Dormant = seated;
            body = gameObject.AddComponent<CharacterController>();
            body.radius = .3f; body.height = dog ? .9f : boss ? 2.3f : 1.85f;
            body.center = Vector3.up * body.height * .5f;
            body.stepOffset = .15f;
            body.skinWidth = .04f;
            UpdateLabel();
            if (seated) game.SetBossPose(this, 0);
        }

        public void Wake()
        {
            if (!boss || !Dormant || !Alive) return;
            Dormant = false; rising = 1;
            if (flames != null) flames.gameObject.SetActive(true);
            UpdateLabel();
        }

        public void Hit(float amount)
        {
            if (!Alive || Dormant || !game.RunActive || !game.IsCurrentRoom(room)) return;
            Health = Mathf.Max(0, Health - amount);
            game.DefenderImpact(this);
            UpdateLabel();
            if (!Alive)
            {
                body.enabled = false;
                gameObject.SetActive(false);
                if (warning != null) warning.gameObject.SetActive(false);
                game.DefenderDefeated(this);
                Destroy(gameObject);
            }
        }

        private void UpdateLabel()
        {
            if (label != null) label.text = displayName + "  " + Mathf.CeilToInt(Health) + " / " + MaxHealth + (Dormant ? "\n경비견 보호 중" : "");
        }

        private void Update()
        {
            if (!game.RunActive || !game.IsCurrentRoom(room) || !Alive) return;
            if (label != null && Camera.main != null) label.transform.rotation = Camera.main.transform.rotation;
            if (Dormant) return;
            float dt = Time.deltaTime;
            if (boss && flames != null)
            {
                for (int i = 0; i < flames.childCount; i++)
                {
                    var flame = flames.GetChild(i);
                    if (flame.GetComponent<Renderer>() == null) continue;
                    float flicker = 1 + Mathf.Sin(Time.time * 13 + i * 2) * .3f;
                    flame.localScale = new Vector3(.15f, .5f * flicker, .15f);
                }
            }
            if (rising > 0)
            {
                rising = Mathf.Max(0, rising - dt);
                game.SetBossPose(this, 1 - rising);
                return;
            }
            Vector3 toward = game.PlayerPosition - transform.position;
            toward.y = 0;
            if (boss)
            {
                phaseTime -= dt;
                if (phase == 0 && phaseTime <= 0)
                {
                    phase = 1; phaseTime = .85f;
                    chargeDirection = toward.sqrMagnitude > .01f ? toward.normalized : transform.forward;
                    warning.gameObject.SetActive(true);
                    warning.position = transform.position + chargeDirection * 4.5f + Vector3.up * .03f;
                    warning.rotation = Quaternion.LookRotation(chargeDirection);
                }
                else if (phase == 1 && phaseTime <= 0)
                { phase = 2; phaseTime = .6f; warning.gameObject.SetActive(false); }
                else if (phase == 2 && phaseTime <= 0)
                { phase = 3; phaseTime = 1.4f; }
                else if (phase == 3 && phaseTime <= 0)
                { phase = 0; phaseTime = 2.4f; chooseTime = 0; }
            }
            if (boss && phase == 2) Move(chargeDirection * 15, dt);
            else if (!boss || phase == 0) Patrol(dt, toward);
            else body.Move(Vector3.down * 3 * dt);
            if ((!boss || phase != 3) && toward.magnitude < (dog ? 1f : 1.15f) &&
                Mathf.Abs(game.PlayerPosition.y - transform.position.y) < (dog ? .75f : 1.3f) &&
                game.ClearDefenseSight(AimPoint, game.PlayerPosition + Vector3.up * .8f, transform, null))
                game.DefenderContact(boss && phase == 2 ? 25 : dog || boss ? 12 : 10);
        }

        private void Patrol(float dt, Vector3 toward)
        {
            // Close-range pursuit can use newly destroyed furniture space while the controller
            // still enforces real collisions. Guard keeps its random patrol behavior.
            if (!guard && toward.magnitude < 3 && toward.magnitude > .7f &&
                game.ClearDefenseSight(AimPoint, game.PlayerPosition + Vector3.up * .8f, transform, null))
            {
                Move(toward.normalized * (dog ? 7.2f : boss ? 3.6f : 2.4f), dt);
                return;
            }
            chooseTime -= dt;
            if (path == null || waypoint >= path.Count || chooseTime <= 0)
            {
                var target = navigation.RandomPoint(random);
                bool chase = boss || (!guard && toward.magnitude < 7 && (!dog || random.NextDouble() < .35));
                if (chase) target = new OfficeLayout.Point(game.PlayerPosition.x, game.PlayerPosition.z - roomStart);
                path = navigation.Path(transform.position.x, transform.position.z - roomStart, target.x, target.z);
                waypoint = 0;
                chooseTime = boss ? .9f : dog ? .8f + (float)random.NextDouble() : 2 + (float)random.NextDouble() * 3;
            }
            if (waypoint >= path.Count) return;
            Vector3 destination = new Vector3(path[waypoint].x, transform.position.y, roomStart + path[waypoint].z);
            Vector3 delta = destination - transform.position;
            if (delta.magnitude < .12f) { waypoint++; body.Move(Vector3.down * dt * 3); return; }
            float speed = dog ? 7.2f : boss ? 3.6f : guard ? 2 : 2.4f;
            Move(delta.normalized * Mathf.Min(speed, delta.magnitude / Mathf.Max(dt, .001f)), dt);
            if (visual != null && dog) visual.localPosition = Vector3.up * (Mathf.Abs(Mathf.Sin(Time.time * 15)) * .06f);
        }

        private void Move(Vector3 velocity, float dt)
        {
            // Controller collision, then room bounds: no teleporting through walls or into other offices.
            Vector3 next = transform.position + velocity * dt;
            next.x = Mathf.Clamp(next.x, -roomWidth / 2 + .5f, roomWidth / 2 - .5f);
            next.z = Mathf.Clamp(next.z, roomStart + .7f, roomEnd - .7f);
            body.Move(next - transform.position + Vector3.down * 3 * dt);
            if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), dt * 12);
        }
    }
}
