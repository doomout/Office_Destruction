using UnityEngine;

namespace CodexStarter
{
    public sealed class StarterCamera : MonoBehaviour
    {
        public Transform target;
        private readonly Vector3 offset = new Vector3(0f, 7f, -9f);

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }
}
