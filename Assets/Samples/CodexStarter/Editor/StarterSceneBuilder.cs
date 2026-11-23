using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodexStarter
{
    public static class StarterSceneBuilder
    {
        [MenuItem("Tools/Codex Starter/Create Practice Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(24f, 1f, 24f);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1.1f, -3f);
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;
            player.AddComponent<StarterPlayer>();

            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "JumpObstacle";
            obstacle.transform.position = new Vector3(0f, 0.4f, 2f);
            obstacle.transform.localScale = new Vector3(4f, 0.8f, 1f);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 7f, -9f);
            cameraObject.transform.LookAt(player.transform.position + Vector3.up * 0.5f);
            cameraObject.AddComponent<StarterCamera>().target = player.transform;
            var lightObject = new GameObject("Directional Light", typeof(Light));
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);

            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Samples/CodexStarter/Practice.unity");
            if (!EditorSceneManager.SaveScene(scene, path))
                throw new System.IO.IOException("Could not save practice scene: " + path);
            Selection.activeGameObject = player;
            Debug.Log("Codex Starter ready: " + path + ". Press Play, click Game, use WASD and Space.");
        }
    }
}
