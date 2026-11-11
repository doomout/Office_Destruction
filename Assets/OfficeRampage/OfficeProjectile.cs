using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeProjectile : MonoBehaviour
    {
        public OfficeRampageGame game;
        public Transform owner;
        public Vector3 direction;
        public float speed, range, damage, radius = .12f;
        public bool friendly, explosive;
        private float distance;
        private void Update()
        {
            if (game == null) { Destroy(gameObject); return; }
            if (!game.RunActive || game.CombatLocked) return;
            float step = Mathf.Min(speed * Time.deltaTime, range - distance);
            RaycastHit closest = default; bool found = false; float nearest = float.MaxValue;
            foreach (var hit in Physics.SphereCastAll(transform.position, radius, direction, step, ~0, QueryTriggerInteraction.Ignore))
            {
                if (owner != null && (hit.transform == owner || hit.transform.IsChildOf(owner))) continue;
                if (!friendly && hit.collider.GetComponentInParent<OfficeDefender>() != null) continue;
                if (hit.distance < nearest) { nearest = hit.distance; closest = hit; found = true; }
            }
            if (found)
            {
                if (explosive) game.ResolvePlayerBlast(closest.point - direction * .2f, 4.5f, damage);
                else if (friendly)
                {
                    var prop = closest.collider.GetComponentInParent<OfficeProp>(); if (prop != null) prop.Hit(damage, direction);
                    var enemy = closest.collider.GetComponentInParent<OfficeDefender>(); if (enemy != null) enemy.Hit(damage);
                }
                else if (game.IsPlayerCollider(closest.collider)) game.DefenderContact(damage);
                Destroy(gameObject); return;
            }
            transform.position += direction * step; distance += step;
            if (!explosive) transform.Rotate(direction, 540 * Time.deltaTime, Space.World);
            if (distance >= range)
            { if (explosive) game.ResolvePlayerBlast(transform.position, 4.5f, damage); Destroy(gameObject); }
        }
    }
}
