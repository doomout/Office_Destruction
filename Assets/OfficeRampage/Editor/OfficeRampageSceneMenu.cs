using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OfficeRampage.Editor
{
    public static class OfficeRampageSceneMenu
    {
        public const string ScenePath = "Assets/OfficeRampage/Scenes/OfficeRampage.unity";

        [MenuItem("Tools/Office Rampage/Open Game Scene")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Office Rampage: press Play, click Game. WASD / Mouse / LMB / Space / 1,2,3. Esc pauses.");
        }
    }
}
