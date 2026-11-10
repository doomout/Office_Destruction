using UnityEngine;

namespace OfficeRampage
{
    public sealed class OfficeProp : MonoBehaviour
    {
        public OfficeRampageGame game;
        public int room;
        public float health = 50f;
        public Color debrisColor;
        private bool broken;

        public void Hit(float damage, Vector3 direction)
        {
            if (broken) return;
            health -= damage;
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
