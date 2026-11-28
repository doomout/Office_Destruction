using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

namespace OfficeRampage
{
    // Use a real focused text control for native IME; Web uses the browser's native input.
    public sealed class OfficeNameEditor : MonoBehaviour
    {
        public bool Active { get; private set; }
        public string Text { get; private set; } = "";
        public string Composition { get; private set; } = "";
        private UIDocument document;
        private PanelSettings panel;
        private TextField field;
        private Keyboard keyboard;
        private bool needsFocus;
        private GUIStyle displayStyle;
        private float textScroll;
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool previousCapture;
        [DllImport("__Internal")] private static extern void OfficeNameOpen(string receiver,string value);
        [DllImport("__Internal")] private static extern void OfficeNameRect(float x,float y,float w,float h,float sw,float sh,float fontSize);
        [DllImport("__Internal")] private static extern void OfficeNameClose();
#endif
        public void Open(string value)
        {
            Close(); Text=Limit(value??""); Active=true; Composition="";textScroll=0;
#if UNITY_WEBGL && !UNITY_EDITOR
            previousCapture=WebGLInput.captureAllKeyboardInput; WebGLInput.captureAllKeyboardInput=false;
            OfficeNameOpen(gameObject.name,Text);
#else
            EnsureField(); field.SetValueWithoutNotify(Text); document.rootVisualElement.style.display=DisplayStyle.Flex;
            keyboard=Keyboard.current;
            if(keyboard!=null)keyboard.onIMECompositionChange+=OnComposition;
            needsFocus=true;
#endif
        }
        public void Close()
        {
            if(!Active)return;
            Active=false;
#if UNITY_WEBGL && !UNITY_EDITOR
            OfficeNameClose(); WebGLInput.captureAllKeyboardInput=previousCapture;
#else
            if(keyboard!=null)keyboard.onIMECompositionChange-=OnComposition;
            if(field!=null)field.Blur();
            if(document!=null)document.rootVisualElement.style.display=DisplayStyle.None;
            keyboard?.SetIMEEnabled(false);keyboard=null;
#endif
            Composition="";
        }
        private static string Limit(string value)
        {
            value=value.Replace("\r","").Replace("\n","");
            if(value.Length<=24)return value;
            value=value.Substring(0,24);return char.IsHighSurrogate(value[23])?value.Substring(0,23):value;
        }
        private void EnsureField()
        {
            if(document!=null)return;
            panel=ScriptableObject.CreateInstance<PanelSettings>();
            panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.sortingOrder=1000;
            panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("OfficeFonts/NameInputTheme");
            var host=new GameObject("Name input panel");host.transform.SetParent(transform,false);
            document=host.AddComponent<UIDocument>();document.panelSettings=panel;
            var root=document.rootVisualElement;root.pickingMode=PickingMode.Ignore;
            field=new TextField{maxLength=24,isDelayed=false,multiline=false};
            field.style.position=Position.Absolute;
            // IMGUI is drawn above runtime panels. Keep the focused native editor for input,
            // and mirror its text/caret in the existing IMGUI layer below.
            field.style.opacity=0;
            field.style.marginLeft=field.style.marginRight=field.style.marginTop=field.style.marginBottom=0;
            field.style.unityFontDefinition=FontDefinition.FromFont(OfficeGameFont.Shared);
            field.RegisterValueChangedCallback(e=>{if(Active)Text=Limit(e.newValue);});
            root.Add(field);
            var input=field.Q(className:TextField.inputUssClassName);
            input.style.backgroundColor=new Color(.10f,.17f,.22f);input.style.color=Color.white;
            input.style.paddingLeft=input.style.paddingRight=8;
            input.style.paddingTop=input.style.paddingBottom=0;
            input.style.unityTextAlign=TextAnchor.MiddleLeft;
        }
        private void OnComposition(IMECompositionString value){if(Active)Composition=value.ToString();}
        [Serializable] private class BrowserValue { public string text; public bool composing; }
        // Called by the WebGL plugin. Preserve these names for SendMessage/IL2CPP.
        [UnityEngine.Scripting.Preserve] public void OnBrowserName(string json)
        {
            if(!Active)return;
            var value=JsonUtility.FromJson<BrowserValue>(json);
            Composition=value.composing?"…":"";
            if(!value.composing)Text=Limit(value.text??"");
        }
        private void OnDisable(){Close();}
        private void OnDestroy(){Close();if(document!=null)Destroy(document.gameObject);if(panel!=null)Destroy(panel);}
        public void DrawField(Rect rect,GUIStyle style)
        {
            if(!Active||Event.current.type!=EventType.Repaint)return;
            Vector3 top=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.x,rect.y,0));
            Vector3 bottom=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMax,rect.yMax,0));
            float fontSize=style.fontSize*Mathf.Abs(GUI.matrix.m11);
#if UNITY_WEBGL && !UNITY_EDITOR
            OfficeNameRect(top.x,top.y,bottom.x-top.x,bottom.y-top.y,Screen.width,Screen.height,fontSize);
#else
            field.style.left=top.x;field.style.top=top.y;field.style.width=bottom.x-top.x;
            field.style.height=Mathf.Max(bottom.y-top.y,fontSize*1.65f);field.style.fontSize=fontSize;
            if(needsFocus){needsFocus=false;field.schedule.Execute(()=>{if(Active){field.Focus();field.SelectAll();}});}
            DrawNativeField(rect,style);
#endif
        }
        // TextField owns IME preview and committed text. Never append a second IME stream.
        private string NativeDisplayText => field != null ? field.text : Text;
        private void DrawNativeField(Rect rect,GUIStyle source)
        {
            if(displayStyle==null)displayStyle=new GUIStyle(GUI.skin.label){richText=false,wordWrap=false,padding=new RectOffset(0,0,0,0),alignment=TextAnchor.MiddleLeft,clipping=TextClipping.Overflow};
            displayStyle.font=source.font;displayStyle.fontSize=source.fontSize;displayStyle.normal.textColor=Color.white;
            GUI.Box(rect,GUIContent.none,source);
            string shown=NativeDisplayText;
            int cursor=Mathf.Clamp(field.cursorIndex,0,shown.Length),anchor=Mathf.Clamp(field.selectIndex,0,shown.Length);
            string before=shown.Substring(0,cursor);
            float Width(string value)=>displayStyle.CalcSize(new GUIContent(value)).x;
            var inner=new Rect(rect.x+8,rect.y,rect.width-16,rect.height);
            float caret=Width(before);
            textScroll=Mathf.Clamp(textScroll,Mathf.Max(0,caret-inner.width+3),Mathf.Max(0,caret));
            GUI.BeginGroup(inner);
            if(cursor!=anchor&&Composition.Length==0)
            {
                int start=Mathf.Min(cursor,anchor),end=Mathf.Max(cursor,anchor);
                Color saved=GUI.color;GUI.color=new Color(.15f,.6f,.8f,.65f);
                GUI.DrawTexture(new Rect(Width(shown.Substring(0,start))-textScroll,3,Width(shown.Substring(start,end-start)),inner.height-6),Texture2D.whiteTexture);GUI.color=saved;
            }
            GUI.Label(new Rect(-textScroll,0,Mathf.Max(inner.width,Width(shown)+8),inner.height),shown,displayStyle);
            if(Time.unscaledTime%1<.6f)GUI.DrawTexture(new Rect(caret-textScroll,5,1,inner.height-10),Texture2D.whiteTexture);
            GUI.EndGroup();
        }
    }
}
