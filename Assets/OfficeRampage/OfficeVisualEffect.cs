using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeVisualEffect : MonoBehaviour
    {
        public OfficeRampageGame game;
        public float life = 1, growth;
        public Vector3 velocity, spin;
        private void Update()
        {
            if (game == null) { Destroy(gameObject); return; }
            if (!game.RunActive) return;
            float dt = Time.deltaTime; life -= dt;
            transform.position += velocity * dt;
            transform.Rotate(spin * dt, Space.Self);
            transform.localScale += Vector3.one * growth * dt;
            if (life <= 0) Destroy(gameObject);
        }
    }
}
