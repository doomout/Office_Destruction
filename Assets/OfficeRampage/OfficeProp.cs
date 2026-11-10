using UnityEngine;

namespace OfficeRampage
{
    public sealed class OfficeProp : MonoBehaviour
    {
        public OfficeRampageGame game;
        public int room;
        public float health = 50f;
        public Color debrisColor;
        public int soundMaterial;
        private bool broken;
        private float nextImpactSound;

        public void Hit(float damage, Vector3 direction)
        {
            if (broken) return;
            health -= damage;
            if (health <= 0 || Time.time >= nextImpactSound)
            {
                game.PlayPropSound(soundMaterial, health <= 0, transform.position + Vector3.up);
                nextImpactSound = Time.time + .09f;
            }
            game.Impact(transform.position + Vector3.up, debrisColor, direction, health <= 0 ? 7 : 2);
            if (health > 0) return;
            broken = true;
            game.PropDestroyed(room);
            // Disable colliders immediately so a second pellet cannot score this object twice.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
