using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace OfficeRampage
{
    // Use committed Unicode characters and OS IME composition from the project's actual Input System.
    // IMGUI text fields do not reliably receive that composition with the new-input-only backend.
    public sealed class OfficeNameEditor : MonoBehaviour
    {
        public bool Active { get; private set; }
        public string Text { get; private set; }="";
        public string Composition { get; private set; }="";
        public int Caret { get; private set; }
        public bool SelectAll { get; private set; }
        private Keyboard keyboard;
        private int compositionFrame=-100;
        public void Open(string text)
        {
            Close(); Text=text??""; Caret=Text.Length; Active=true; SelectAll=false;
            Attach();
        }
        private void Attach()
        {
            if(keyboard==Keyboard.current) return;
            Detach(); keyboard=Keyboard.current;
            if(keyboard==null) return;
            keyboard.onTextInput+=AcceptCharacter; keyboard.onIMECompositionChange+=OnComposition;
            keyboard.SetIMEEnabled(true);
        }
        private void Detach()
        {
            if(keyboard==null) return;
            keyboard.onTextInput-=AcceptCharacter; keyboard.onIMECompositionChange-=OnComposition;
            if(keyboard.added) keyboard.SetIMEEnabled(false); keyboard=null;
        }
        public void Close() { Active=false; Composition=""; SelectAll=false; Detach(); }
        private void OnDisable() { Close(); }
        private void OnDestroy() { Detach(); }
        private void OnApplicationFocus(bool focus) { if(Active && focus && keyboard!=null) keyboard.SetIMEEnabled(true); }
        private void OnComposition(IMECompositionString value) { SetComposition(value.ToString()); }
        public void SetComposition(string value)
        { if(!Active) return; Composition=value??""; compositionFrame=Time.frameCount; }
        public void AcceptCharacter(char character)
        {
            if(!Active || char.IsControl(character)) return;
            Insert(character.ToString());
        }
        public void Insert(string value)
        {
            if(!Active) return;
            if(SelectAll) { Text=""; Caret=0; SelectAll=false; }
            foreach(char c in value)
            {
                if(char.IsControl(c) || Text.Length>=24) continue;
                Text=Text.Insert(Caret,c.ToString()); Caret++;
            }
        }
        public void Backspace()
        {
            if(!Active || Composition.Length>0) return;
            if(SelectAll) { Text=""; Caret=0; SelectAll=false; }
            else if(Caret>0) { Text=Text.Remove(Caret-1,1); Caret--; }
        }
        public string DisplayText { get { return SelectAll?(Composition.Length>0?"〈"+Composition+"〉":"["+Text+"]"):Text.Insert(Caret,Composition.Length>0?"〈"+Composition+"〉":"│"); } }
        public void PositionCandidateWindow(Vector2 position) { if(Active && keyboard!=null) keyboard.SetIMECursorPosition(position); }
        private void Update()
        {
            if(!Active) return; Attach(); if(keyboard==null) return;
            if(Composition.Length>0 || compositionFrame==Time.frameCount) return;
            bool ctrl=keyboard.ctrlKey.isPressed;
            if(ctrl && keyboard.aKey.wasPressedThisFrame) SelectAll=true;
            if(ctrl && keyboard.cKey.wasPressedThisFrame && SelectAll) GUIUtility.systemCopyBuffer=Text;
            if(ctrl && keyboard.vKey.wasPressedThisFrame) Insert(GUIUtility.systemCopyBuffer);
            if(keyboard.backspaceKey.wasPressedThisFrame) Backspace();
            if(keyboard.deleteKey.wasPressedThisFrame) { if(SelectAll) Backspace(); else if(Caret<Text.Length) Text=Text.Remove(Caret,1); }
            if(keyboard.leftArrowKey.wasPressedThisFrame) { Caret=Mathf.Max(0,Caret-1); SelectAll=false; }
            if(keyboard.rightArrowKey.wasPressedThisFrame) { Caret=Mathf.Min(Text.Length,Caret+1); SelectAll=false; }
            if(keyboard.homeKey.wasPressedThisFrame) { Caret=0; SelectAll=false; }
            if(keyboard.endKey.wasPressedThisFrame) { Caret=Text.Length; SelectAll=false; }
        }
    }
}
