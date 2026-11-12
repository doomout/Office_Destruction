using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeSlidingDoor : MonoBehaviour
    {
        public OfficeRampageGame game;
        public Transform left, right;
        public Collider barrier;
        public Renderer[] indicators;
        public Material openMaterial;
        public TextMesh sign;
        private bool opening;
        private float progress;
        public bool IsOpen { get { return progress >= 1; } }
        public void Open()
        {
            if (opening) return;
            opening = true;
            foreach (var indicator in indicators) indicator.sharedMaterial = openMaterial;
            sign.text = "문 여는 중";
        }
        private void Update()
        {
            if (!opening || IsOpen || game == null || !game.RunActive) return;
            progress = Mathf.Min(1, progress + Time.deltaTime / 1.6f);
            float slide = Mathf.SmoothStep(0, 1.8f, progress);
            left.localPosition = new Vector3(-.825f - slide, 1.55f, 0);
            right.localPosition = new Vector3(.825f + slide, 1.55f, 0);
            if (IsOpen) { barrier.enabled = false; sign.text = "출구 열림"; }
        }
    }
}
