using System.Collections.Generic;
using UnityEngine;

namespace OfficeRampage
{
    // Self-contained procedural VFX: no downloaded textures, models or audio.
    public sealed class OfficeBlastEffect : MonoBehaviour
    {
        private sealed class Puff
        {
            public Transform transform;
            public Renderer renderer;
            public Vector3 origin, velocity, size;
            public Color hot, cool;
            public float delay, duration, grow, gravity, glow;
        }
        private readonly List<Puff> puffs = new List<Puff>();
        private OfficeRampageGame game;
        private Material material, ringMaterial;
        private MaterialPropertyBlock properties;
        private LineRenderer ring;
        private Light flashLight;
        private float age, lifetime, ringRadius, ringDuration;
        public bool Nuclear { get; private set; }
        public float Age { get { return age; } }

        public void Initialize(OfficeRampageGame owner, bool nuclear)
        {
            game = owner; Nuclear = nuclear; lifetime = nuclear ? 8 : 3.2f;
            properties = new MaterialPropertyBlock();
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Smoothness", .05f); material.EnableKeyword("_EMISSION");
            ringMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ringMaterial.SetColor("_BaseColor", new Color(1f, .62f, .16f));
            var lightObject = new GameObject("Blast illumination"); lightObject.transform.SetParent(transform, false);
            flashLight = lightObject.AddComponent<Light>(); flashLight.type = LightType.Point;
            flashLight.color = new Color(1f, .55f, .15f); flashLight.range = nuclear ? 45 : 12;
            flashLight.shadows = LightShadows.None;
            ring = new GameObject("Expanding shockwave").AddComponent<LineRenderer>();
            ring.transform.SetParent(transform, false); ring.useWorldSpace = false;
            ring.loop = true; ring.positionCount = 80; ring.sharedMaterial = ringMaterial;
            ringRadius = nuclear ? 28 : 5.5f; ringDuration = nuclear ? 1.4f : .65f;
            if (nuclear) BuildMushroom(); else BuildRocket();
            RenderAge();
        }
        private void Add(Vector3 at, Vector3 size, Vector3 velocity, float delay, float duration, float grow, Color hot, Color cool, float glow = 0, float gravity = 0)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = glow > 0 ? "Rolling fire" : "Smoke billow";
            obj.transform.SetParent(transform, false);
            var col = obj.GetComponent<Collider>(); col.enabled = false; Destroy(col);
            var renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            puffs.Add(new Puff { transform = obj.transform, renderer = renderer, origin = at, size = size,
                velocity = velocity, delay = delay, duration = duration, grow = grow, hot = hot, cool = cool, glow = glow, gravity = gravity });
        }
        private void BuildRocket()
        {
            Color yellow = new Color(1, .85f, .25f), orange = new Color(1, .22f, .015f);
            Add(Vector3.zero, Vector3.one * 3.1f, Vector3.zero, 0, .38f, .12f, Color.white, orange, 5);
            for (int i = 0; i < 9; i++)
            {
                Vector3 dir = Random.onUnitSphere;
                Add(dir * .25f, Vector3.one * Random.Range(1.1f, 2f), dir * 2, .03f, .65f, .2f, yellow, orange, 2);
            }
            for (int i = 0; i < 14; i++)
            {
                Vector3 dir = Random.onUnitSphere; dir.y = Mathf.Abs(dir.y);
                Add(dir * .6f, Vector3.one * Random.Range(1.3f, 2.3f), dir * 1.5f + Vector3.up * .7f, .14f + i * .018f, 2.5f, .8f,
                    new Color(.13f, .1f, .08f), new Color(.42f, .39f, .35f));
            }
            for (int i = 0; i < 22; i++)
            {
                Vector3 velocity = Random.onUnitSphere * Random.Range(6f, 12f); velocity.y = Mathf.Abs(velocity.y);
                Add(Vector3.zero, new Vector3(.08f, .19f, .08f), velocity, 0, .75f, .03f, yellow, orange, 3, 10);
            }
        }
        private void BuildMushroom()
        {
            Color fire = new Color(1f, .38f, .025f), smoke = new Color(.25f, .22f, .19f), ash = new Color(.48f, .43f, .36f);
            Add(Vector3.up, new Vector3(10, 8, 10), Vector3.up * 2, 0, 1.5f, .28f, new Color(1, .9f, .6f), fire, 5);
            // Narrow rising column: stacked turbulent lobes, deliberately narrower than the cap.
            for (int layer = 0; layer < 9; layer++)
                for (int i = 0; i < 3; i++)
                {
                    float a = (i * 120 + layer * 47) * Mathf.Deg2Rad;
                    Vector3 at = new Vector3(Mathf.Cos(a) * .9f, 1 + layer * 1.6f, Mathf.Sin(a) * .9f);
                    Add(at, new Vector3(3.7f, 3.5f, 3.7f), Vector3.up * .18f, .12f + layer * .11f, 7, 1.1f, layer < 4 ? fire : smoke, ash, layer < 4 ? .8f : 0);
                }
            // Wide toroidal cap with a rounded crown: readable mushroom silhouette from any side.
            for (int i = 0; i < 28; i++)
            {
                float a = i * Mathf.PI * 2 / 28;
                float radius = i % 2 == 0 ? 6.4f : 4.5f;
                Vector3 at = new Vector3(Mathf.Cos(a) * radius, 14.5f + (i % 3) * .55f, Mathf.Sin(a) * radius);
                Add(at, new Vector3(5.8f, 4.8f, 5.8f), new Vector3(Mathf.Cos(a) * .17f, .2f, Mathf.Sin(a) * .17f), 1.05f + (i % 4) * .06f, 7, 1.4f, i % 4 == 0 ? fire : smoke, ash, i % 4 == 0 ? .5f : 0);
            }
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2 / 7;
                Add(new Vector3(Mathf.Cos(a) * 3, 17, Mathf.Sin(a) * 3), new Vector3(6, 4, 6), Vector3.up * .15f, 1.2f, 7, 1.5f, smoke, ash);
            }
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2 / 24;
                Vector3 radial = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Add(radial * 2 + Vector3.up * .25f, new Vector3(4, 1.7f, 4), radial * 2.4f, .3f, 6, 1.2f, new Color(.51f, .4f, .26f), new Color(.65f, .54f, .38f));
            }
        }
        private void Update()
        {
            if (game == null) { Destroy(gameObject); return; }
            if (!game.RunActive) return;
            age += Time.deltaTime; RenderAge();
            if (age > lifetime) Destroy(gameObject);
        }
        private void RenderAge()
        {
            foreach (var puff in puffs)
            {
                float elapsed = age - puff.delay;
                bool visible = elapsed >= 0 && elapsed < puff.duration;
                puff.renderer.enabled = visible; if (!visible) continue;
                float growth = Mathf.SmoothStep(.05f, 1, Mathf.Clamp01(elapsed / puff.grow));
                float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(puff.duration * .78f, puff.duration, elapsed));
                puff.transform.localScale = puff.size * growth * fade;
                puff.transform.localPosition = puff.origin + puff.velocity * elapsed + Vector3.down * (.5f * puff.gravity * elapsed * elapsed);
                Color color = Color.Lerp(puff.hot, puff.cool, Mathf.Clamp01(elapsed / (puff.duration * .55f)));
                properties.SetColor("_BaseColor", color);
                properties.SetColor("_EmissionColor", color * puff.glow * Mathf.Max(0, 1 - elapsed / 1.4f));
                puff.renderer.SetPropertyBlock(properties);
            }
            flashLight.intensity = (Nuclear ? 9 : 5) * Mathf.Exp(-age * (Nuclear ? 2 : 7));
            ring.enabled = age < ringDuration;
            if (ring.enabled)
            {
                float t = age / ringDuration;
                ring.widthMultiplier = Mathf.Lerp(Nuclear ? .5f : .2f, .015f, t);
                for (int i = 0; i < 80; i++)
                {
                    float a = i * Mathf.PI * 2 / 80;
                    ring.SetPosition(i, new Vector3(Mathf.Cos(a) * ringRadius * t, .09f, Mathf.Sin(a) * ringRadius * t));
                }
            }
        }
        private void OnDestroy()
        { if (material != null) Destroy(material); if (ringMaterial != null) Destroy(ringMaterial); }
    }
}
