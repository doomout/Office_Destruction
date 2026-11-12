using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeRocketTrail : MonoBehaviour
    {
        public OfficeRampageGame game;
        private Material material;
        private Transform flame;
        private TrailRenderer trail;
        private float age;
        private void Start()
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(1, .5f, .035f);
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.sharedMaterial = material; trail.time = .22f; trail.minVertexDistance = .08f;
            trail.startWidth = .45f; trail.endWidth = .02f;
            trail.startColor = Color.yellow; trail.endColor = new Color(1, .15f, 0, 0);
            trail.numCapVertices = 3;
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var col = obj.GetComponent<Collider>(); col.enabled = false; Destroy(col);
            flame = obj.transform; flame.name = "Rocket exhaust flame";
            flame.SetParent(transform, false);
            // Parent projectile is .2 x .2 x .7; compensate so exhaust is readable in world space.
            flame.localPosition = new Vector3(0, 0, -1.7f);
            flame.localScale = new Vector3(2, 2, 3.4f);
            obj.GetComponent<Renderer>().sharedMaterial = material;
        }
        private void Update()
        {
            if (game == null || !game.RunActive || game.CombatLocked) return;
            age += Time.deltaTime;
            if (flame != null) flame.localScale = new Vector3(2, 2, 3.4f + Mathf.Sin(age * 65) * .6f);
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
