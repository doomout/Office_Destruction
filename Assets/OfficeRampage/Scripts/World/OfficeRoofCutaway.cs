using UnityEngine;
namespace OfficeRampage
{
    // Wide cinematic cameras look down through the roof; normal interior cameras see the ceiling and windows.
    public sealed class OfficeRoofCutaway : MonoBehaviour
    {
        public float height;
        public Renderer[] renderers;
        private void LateUpdate()
        {
            if(Camera.main==null) return;
            bool show=Camera.main.transform.position.y<height;
            foreach(var renderer in renderers) if(renderer!=null)renderer.enabled=show;
        }
    }
}
