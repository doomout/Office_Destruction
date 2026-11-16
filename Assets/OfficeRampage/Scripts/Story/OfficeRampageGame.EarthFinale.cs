using System.Collections;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private IEnumerator SplitEarth(Transform space)
        {
            var globe=space.Find("Earth");Vector3 center=globe.position;
            yield return CameraFlight(center+new Vector3(0,80,760),center,1.2f);
            globe.GetComponent<Renderer>().enabled=false;
            var rim=globe.Find("Blue atmospheric rim");if(rim!=null)rim.gameObject.SetActive(false);
            var split=new GameObject("Earth split finale").AddComponent<OfficeEarthSplit>();split.transform.SetParent(space,false);split.transform.position=center;
            split.Initialize(globe.GetComponent<Renderer>().sharedMaterial);audioBus.Play("explosion",1);
            float t=0;while(t<4.5f){if(RunActive){t+=Time.deltaTime;split.SetProgress(t/4.5f);}yield return null;}
            yield return SceneWait(1.3f);
        }
    }
}
