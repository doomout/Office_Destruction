using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeHealingPotion : MonoBehaviour
    {
        public OfficeRampageGame game;
        public int room;
        public Transform visual;
        private bool collected;
        private float bob;
        private void Update()
        {
            if (collected || game == null || !game.RunActive || game.CombatLocked || !game.IsCurrentRoom(room)) return;
            bob += Time.deltaTime;
            visual.localPosition = Vector3.up * (.65f + Mathf.Sin(bob * 3) * .12f);
            visual.Rotate(Vector3.up, 65 * Time.deltaTime, Space.Self);
            Vector3 delta = game.PlayerPosition - transform.position;
            if (new Vector2(delta.x, delta.z).sqrMagnitude > 1.21f || Mathf.Abs(delta.y) > 1.5f) return;
            if (!game.TryCollectHealingPotion(room)) return;
            collected = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
