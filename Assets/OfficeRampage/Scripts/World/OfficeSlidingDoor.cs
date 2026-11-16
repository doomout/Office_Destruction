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
        public string openingText="엘리베이터 열리는 중";
        private bool opening;
        private float progress;
        public bool IsOpen { get { return progress >= 1; } }
        public void RemoveBarrier(){opening=true;progress=1;barrier.enabled=false;gameObject.SetActive(false);}
        public void Open()
        {
            if (opening) return;
            opening = true;
            foreach (var indicator in indicators) indicator.sharedMaterial = openMaterial;
            sign.text = openingText;
        }
        private void Update()
        {
            if (!opening || IsOpen || game == null || !game.RunActive) return;
            progress = Mathf.Min(1, progress + Time.deltaTime / 1.6f);
            float slide = Mathf.SmoothStep(0, 1.8f, progress);
            left.localPosition = new Vector3(-.825f - slide, 1.55f, 0);
            right.localPosition = new Vector3(.825f + slide, 1.55f, 0);
            if (IsOpen) { barrier.enabled = false; sign.text = ""; }
        }
    }
}
