using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace OfficeRampage
{
    public sealed class OfficeNameEditor : MonoBehaviour
    {
        public bool Active { get; private set; }
        public string Text { get; private set; }="";
        public string Composition { get; private set; }="";
        private Keyboard keyboard;
        private int cursor,anchor;
        private float repeatAt,scroll;
        private Key repeatKey;
        private char pendingSurrogate;
        private GUIStyle textStyle;
        public void Open(string value)
        {Close();Text=Limit(value??"");cursor=anchor=Text.Length;Active=true;scroll=0;BindKeyboard();}
        public void Close()
        {
            if(keyboard!=null){keyboard.onTextInput-=OnText;keyboard.onIMECompositionChange-=OnComposition;keyboard.SetIMEEnabled(false);}
            keyboard=null;Active=false;Composition="";repeatKey=Key.None;pendingSurrogate='\0';
        }
        private void BindKeyboard()
        {
            if(keyboard==Keyboard.current)return;
            if(keyboard!=null){keyboard.onTextInput-=OnText;keyboard.onIMECompositionChange-=OnComposition;keyboard.SetIMEEnabled(false);}
            keyboard=Keyboard.current;Composition="";
            if(keyboard!=null){keyboard.onTextInput+=OnText;keyboard.onIMECompositionChange+=OnComposition;keyboard.SetIMEEnabled(true);}
        }
        private void OnDisable(){Close();}
        private void OnApplicationFocus(bool focus)
        {if(Active&&keyboard!=null){keyboard.SetIMEEnabled(focus);if(!focus)Composition="";}}
        private void OnComposition(IMECompositionString value){if(Active)Composition=value.ToString();}
        private void OnText(char value)
        {
            if(!Active||char.IsControl(value))return;
            if(keyboard!=null&&keyboard.ctrlKey.isPressed&&!keyboard.altKey.isPressed)return;
            if(char.IsHighSurrogate(value)){pendingSurrogate=value;return;}
            if(char.IsLowSurrogate(value))
            {if(pendingSurrogate!='\0')Insert(new string(new[]{pendingSurrogate,value}));pendingSurrogate='\0';return;}
            pendingSurrogate='\0';Insert(value.ToString());
        }
        private static string Limit(string value)
        {if(value.Length<=24)return value;value=value.Substring(0,24);return char.IsHighSurrogate(value[23])?value.Substring(0,23):value;}
        private int Start=>Mathf.Min(cursor,anchor);
        private int End=>Mathf.Max(cursor,anchor);
        private void Insert(string value)
        {
            var clean=new System.Text.StringBuilder();foreach(char c in value)if(!char.IsControl(c))clean.Append(c);
            int start=Start,end=End,space=24-(Text.Length-(end-start));string insert=clean.ToString();
            if(insert.Length>space)insert=insert.Substring(0,space);
            if(insert.Length>0&&char.IsHighSurrogate(insert[insert.Length-1]))insert=insert.Substring(0,insert.Length-1);
            Text=Text.Substring(0,start)+insert+Text.Substring(end);cursor=anchor=start+insert.Length;
        }
        private int Previous(int at)=>at>1&&char.IsLowSurrogate(Text[at-1])?at-2:Mathf.Max(0,at-1);
        private int Next(int at)=>at<Text.Length&&char.IsHighSurrogate(Text[at])?Mathf.Min(Text.Length,at+2):Mathf.Min(Text.Length,at+1);
        private void Edit(Key key,bool shift)
        {
            if(key==Key.Backspace||key==Key.Delete)
            {if(cursor==anchor){if(key==Key.Backspace)anchor=Previous(cursor);else anchor=Next(cursor);}Insert("");return;}
            if(key==Key.Home)cursor=0;
            else if(key==Key.End)cursor=Text.Length;
            else if(key==Key.LeftArrow)cursor=!shift&&cursor!=anchor?Start:Previous(cursor);
            else if(key==Key.RightArrow)cursor=!shift&&cursor!=anchor?End:Next(cursor);
            if(!shift)anchor=cursor;
        }
        private void Update()
        {
            if(!Active)return;BindKeyboard();if(keyboard==null)return;
            if(Composition.Length>0){repeatKey=Key.None;return;}
            bool ctrl=keyboard.ctrlKey.isPressed||keyboard.leftMetaKey.isPressed||keyboard.rightMetaKey.isPressed;
            if(ctrl)
            {
                if(keyboard.aKey.wasPressedThisFrame){anchor=0;cursor=Text.Length;}
                if(keyboard.cKey.wasPressedThisFrame||keyboard.xKey.wasPressedThisFrame)
                {if(Start!=End)GUIUtility.systemCopyBuffer=Text.Substring(Start,End-Start);if(keyboard.xKey.wasPressedThisFrame)Insert("");}
                if(keyboard.vKey.wasPressedThisFrame)Insert(GUIUtility.systemCopyBuffer);
            }
            foreach(var key in editKeys)
                if(keyboard[key].wasPressedThisFrame){Edit(key,keyboard.shiftKey.isPressed);repeatKey=key;repeatAt=Time.unscaledTime+.45f;return;}
            if(repeatKey!=Key.None)
            {if(!keyboard[repeatKey].isPressed)repeatKey=Key.None;else if(Time.unscaledTime>=repeatAt){Edit(repeatKey,keyboard.shiftKey.isPressed);repeatAt=Time.unscaledTime+.055f;}}
        }
        private static readonly Key[] editKeys={Key.Backspace,Key.Delete,Key.LeftArrow,Key.RightArrow,Key.Home,Key.End};
        public void DrawField(Rect rect,GUIStyle style)
        {
            if(!Active)return;
            if(textStyle==null)textStyle=new GUIStyle(GUI.skin.label){richText=false,wordWrap=false,padding=new RectOffset(0,0,0,0),alignment=TextAnchor.MiddleLeft};
            textStyle.font=style.font;textStyle.fontSize=style.fontSize;textStyle.normal.textColor=style.normal.textColor;
            var inner=new Rect(rect.x+8,rect.y+3,rect.width-16,rect.height-6);
            float Width(string text)=>textStyle.CalcSize(new GUIContent(text)).x;
            var mouse=Mouse.current;
            if(Event.current.type==EventType.Repaint&&mouse!=null&&mouse.leftButton.wasPressedThisFrame&&rect.Contains(OfficeUIInput.Pointer)&&Composition.Length==0)
            {
                float x=OfficeUIInput.Pointer.x-inner.x+scroll;int at=0;
                while(at<Text.Length&&x>(Width(Text.Substring(0,at))+Width(Text.Substring(0,Next(at))))*.5f)at=Next(at);
                cursor=at;if(keyboard==null||!keyboard.shiftKey.isPressed)anchor=cursor;
            }
            string before=Text.Substring(0,cursor),display=before+Composition+Text.Substring(cursor);
            float caret=Width(before+Composition);scroll=Mathf.Clamp(scroll,Mathf.Max(0,caret-inner.width+3),Mathf.Max(0,caret));
            GUI.Box(rect,GUIContent.none,style);
            GUI.BeginGroup(inner);
            if(Start!=End&&Composition.Length==0)
            {var color=GUI.color;GUI.color=new Color(.1f,.5f,.7f,.55f);GUI.DrawTexture(new Rect(Width(Text.Substring(0,Start))-scroll,2,Width(Text.Substring(Start,End-Start)),inner.height-4),Texture2D.whiteTexture);GUI.color=color;}
            GUI.Label(new Rect(-scroll,0,Mathf.Max(inner.width,Width(display)+8),inner.height),display,textStyle);
            if(Composition.Length>0)GUI.DrawTexture(new Rect(Width(before)-scroll,inner.height-3,Width(Composition),1),Texture2D.whiteTexture);
            if(Time.unscaledTime%1<.6f)GUI.DrawTexture(new Rect(caret-scroll,3,1,inner.height-6),Texture2D.whiteTexture);
            GUI.EndGroup();
            var pos=GUI.matrix.MultiplyPoint3x4(new Vector3(inner.x+caret-scroll,inner.yMax,0));keyboard?.SetIMECursorPosition(pos);
        }
    }
}
