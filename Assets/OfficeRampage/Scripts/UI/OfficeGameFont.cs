using UnityEngine;
namespace OfficeRampage
{
    // A Resources asset references the original TTF, so Unity includes its font data in players.
    public sealed class OfficeGameFont : ScriptableObject
    {
        public Font font;
        private static Font shared;
        private static bool attempted;
        public static Font Shared
        {
            get
            {
                if(shared!=null||attempted)return shared;
                attempted=true;
                var asset=Resources.Load<OfficeGameFont>("OfficeFonts/GameFont");
                if(asset==null||asset.font==null)
                {
                    Debug.LogError("[OfficeFont] GameFont 리소스 또는 Font 참조가 없습니다. Resources/OfficeFonts/GameFont.asset과 Assets/Fonts의 TTF 참조를 확인하세요. OS 폰트로 대체하지 않습니다.");
                    return null;
                }
                shared=asset.font;
                if(shared.material==null)
                    Debug.LogError("[OfficeFont] 폰트 재질을 불러오지 못했습니다: "+shared.name+". TTF Import Settings와 대상 플랫폼 재임포트를 확인하세요.");
                const string sample="한글사무실파괴ABCxyz0123456789";
                shared.RequestCharactersInTexture(sample,24,FontStyle.Normal);
                foreach(char c in sample)
                    if(!shared.HasCharacter(c))
                    {Debug.LogError("[OfficeFont] 글리프 누락: U+"+((int)c).ToString("X4")+" / "+shared.name+". 한글 지원 TTF와 Include Font Data 설정을 확인하세요.");break;}
                return shared;
            }
        }
        public static void Apply(TextMesh text)
        {
            var loaded=Shared;if(text==null||loaded==null)return;
            text.font=loaded;
            var renderer=text.GetComponent<MeshRenderer>();
            if(renderer!=null)renderer.sharedMaterial=loaded.material;
        }
    }
}
