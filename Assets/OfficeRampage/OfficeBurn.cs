using UnityEngine;
namespace OfficeRampage
{
    // Repeated hits refresh duration, but never stack DPS or postpone the next tick.
    public sealed class OfficeBurn : MonoBehaviour
    {
        public OfficeRampageGame game;
        private float remaining, tick = 1;
        private Transform flame;
        private Material material;
        public void Refresh() { remaining = 5; }
        private void Update() { Advance(Time.deltaTime); }
        private void Advance(float dt)
        {
            if (game == null) { Destroy(this); return; }
            if (!game.RunActive || game.CombatLocked) return;
            var enemy = GetComponent<OfficeDefender>();
            var prop = GetComponent<OfficeProp>();
            int room = enemy != null ? enemy.room : prop != null ? prop.room : -1;
            if (!game.IsCurrentRoom(room) || (enemy != null && !enemy.Alive)) { Destroy(this); return; }
            if (flame == null) BuildFlame();
            float elapsed = Mathf.Min(remaining, dt);
            remaining -= elapsed; tick -= elapsed;
            while (tick <= .00001f)
            {
                tick += 1;
                if (enemy != null) enemy.Hit(1);
                else if (prop != null) prop.Hit(1, Vector3.up);
                if (!game.RunActive || game.CombatLocked || !gameObject.activeInHierarchy) break;
            }
            if (flame != null) flame.localRotation = Quaternion.Euler(0, Time.time * 150, 0);
            if (remaining <= 0) Destroy(this);
        }
        private void BuildFlame()
        {
            flame = new GameObject("Shotgun burn flames").transform;
            flame.SetParent(transform, false); flame.localPosition = Vector3.up;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(1, .25f, .015f);
            for (int i = 0; i < 5; i++)
            {
                var part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var col = part.GetComponent<Collider>(); col.enabled = false; Destroy(col);
                part.transform.SetParent(flame, false);
                float angle = i * Mathf.PI * 2 / 5;
                part.transform.localPosition = new Vector3(Mathf.Sin(angle) * .3f, 0, Mathf.Cos(angle) * .3f);
                part.transform.localScale = new Vector3(.12f, .5f, .12f);
                part.GetComponent<Renderer>().sharedMaterial = material;
            }
        }
        private void OnDestroy()
        {
            if (flame != null) Destroy(flame.gameObject);
            if (material != null) Destroy(material);
        }
    }
}
