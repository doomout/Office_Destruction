using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeNameEditor : MonoBehaviour
    {
        public bool Active { get; private set; }
        public string Text { get; private set; } = "";
        public string Composition { get { return Active ? Input.compositionString : ""; } }
        private bool focusPending;
        public void Open(string value)
        { Text=value??""; Active=true; focusPending=true; Input.imeCompositionMode=IMECompositionMode.On; }
        public void Close()
        { Active=false; focusPending=false; Input.imeCompositionMode=IMECompositionMode.Auto; }
        private void OnDisable() { Close(); }
        public void DrawField(Rect rect, GUIStyle style)
        {
            Input.imeCompositionMode=IMECompositionMode.On;
            GUI.SetNextControlName("CompanyNameIME");
            Text=GUI.TextField(rect,Text,24,style);
            if(focusPending && Event.current.type==EventType.Repaint)
            { GUI.FocusControl("CompanyNameIME"); focusPending=false; }
        }
    }
}
