using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    // IMGUI is retained for drawing only. Runtime interactions come from Input System.
    public static class OfficeUIInput
    {
        private static int activeControl,releaseFrame=-1,scrollFrame=-1;
        private static Vector2 offset;
        private static bool pointerAllowed=true;
        public static Vector2 Pointer
        {
            get
            {
                if(Mouse.current==null)return new Vector2(-10000,-10000);
                var p=Mouse.current.position.ReadValue();
                return (Vector2)GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(p.x,Screen.height-p.y,0))-offset;
            }
        }
        public static bool Button(Rect rect,string text,GUIStyle style)
        {
            int id=GUIUtility.GetControlID(FocusType.Passive,rect);bool hover=pointerAllowed&&rect.Contains(Pointer);
            if(Event.current.type!=EventType.Repaint)return false;
            var mouse=Mouse.current;
            style.Draw(rect,new GUIContent(text),hover,activeControl==id,false,false);
            if(mouse==null||!GUI.enabled)return false;
            if(mouse.leftButton.wasPressedThisFrame&&hover)activeControl=id;
            if(mouse.leftButton.wasReleasedThisFrame&&activeControl==id)
            {activeControl=0;if(releaseFrame==Time.frameCount)return false;releaseFrame=Time.frameCount;if(hover)GUI.changed=true;return hover;}
            return false;
        }
        public static float Slider(Rect rect,float value,float min,float max,bool vertical=false)
        {
            int id=GUIUtility.GetControlID(FocusType.Passive,rect);var mouse=Mouse.current;
            if(Event.current.type!=EventType.Repaint||!GUI.enabled||mouse==null)return value;
            if(pointerAllowed&&rect.Contains(Pointer)&&mouse.leftButton.wasPressedThisFrame)activeControl=id;
            if(activeControl!=id)return value;
            if(!mouse.leftButton.isPressed){activeControl=0;return value;}
            float t=vertical?(Pointer.y-rect.y)/rect.height:(Pointer.x-rect.x-10)/Mathf.Max(1,rect.width-20);
            float updated=Mathf.Lerp(min,max,Mathf.Clamp01(t));if(updated!=value)GUI.changed=true;return updated;
        }
        public static Vector2 BeginScrollView(Rect rect,Vector2 scroll,Rect content)
        {
            float max=Mathf.Max(0,content.height-rect.height);bool hover=rect.Contains(Pointer);
            if(Event.current.type==EventType.Repaint&&GUI.enabled&&hover&&Mouse.current!=null&&scrollFrame!=Time.frameCount)
            {scroll.y-=Mouse.current.scroll.ReadValue().y*.4f;scrollFrame=Time.frameCount;}
            if(max>0)
            {
                var track=new Rect(rect.xMax-15,rect.y,15,rect.height);
                scroll.y=Slider(track,scroll.y,0,max,true);
                GUI.Box(track,GUIContent.none);
                float h=Mathf.Max(24,rect.height*rect.height/content.height);
                GUI.Box(new Rect(track.x,track.y+(track.height-h)*Mathf.Clamp01(scroll.y/max),15,h),GUIContent.none);
            }
            scroll.y=Mathf.Clamp(scroll.y,0,max);scroll.x=0;
            pointerAllowed=hover&&Pointer.x<rect.xMax-16;
            offset=rect.position-scroll;
            GUI.BeginGroup(rect);GUI.BeginGroup(new Rect(-scroll.x,-scroll.y,content.width,content.height));
            return scroll;
        }
        public static void EndScrollView(){GUI.EndGroup();GUI.EndGroup();offset=Vector2.zero;pointerAllowed=true;}
    }
}
