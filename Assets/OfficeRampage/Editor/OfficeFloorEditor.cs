using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using OfficeRampage;
public static class OfficeFloorEditor
{
    [MenuItem("Tools/Office Rampage/Floors/1층 로비 편집")]public static void One()=>Open(1);
    [MenuItem("Tools/Office Rampage/Floors/2층 사무실 편집")]public static void Two()=>Open(2);
    [MenuItem("Tools/Office Rampage/Floors/3층 회의실 편집")]public static void Three()=>Open(3);
    [MenuItem("Tools/Office Rampage/Floors/4층 임원 집무실 편집")]public static void Four()=>Open(4);
    [MenuItem("Tools/Office Rampage/Floors/5층 이사회실 편집")]public static void Five()=>Open(5);
    [MenuItem("Tools/Office Rampage/Floors/6층 대표층 편집")]public static void Six()=>Open(6);
    static void Open(int floor)
    {
        if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("층 편집","Play를 종료한 뒤 편집해 주세요.","확인");return;}
        var stage=PrefabStageUtility.OpenPrefab("Assets/OfficeRampage/Resources/OfficeFloors/Floor"+floor+".prefab");
        if(stage==null)return;
        var root=stage.prefabContentsRoot;Selection.activeGameObject=root;
        var shell=root.transform.Find("Office architecture");if(shell!=null)SceneVisibilityManager.instance.Hide(shell.gameObject,true);
        SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(0,0,root.GetComponent<OfficeFloorPrefab>().Length/2),new Vector3(30,5,48)),false);
    }
    [MenuItem("Tools/Office Rampage/Floors/천장·외벽 표시 전환")]
    static void Roof()
    {
        var stage=PrefabStageUtility.GetCurrentPrefabStage();if(stage==null)return;
        var shell=stage.prefabContentsRoot.transform.Find("Office architecture");if(shell==null)return;
        if(SceneVisibilityManager.instance.IsHidden(shell.gameObject))SceneVisibilityManager.instance.Show(shell.gameObject,true);else SceneVisibilityManager.instance.Hide(shell.gameObject,true);
    }
    public static string Validate(OfficeFloorPrefab floor)
    {
        if(floor.floorSurface==null||floor.playerSpawn==null)return "바닥 또는 플레이어 시작점 참조가 없습니다.";
        if(floor.enemySpawns==null||floor.enemySpawns.Any(p=>p==null))return "직원 시작점 참조가 누락되었습니다. 시작점 개수를 유지하세요.";
        if(floor.elevator==null && (floor.dogDoor==null||floor.guardDoor==null||floor.bossSpawn==null||floor.secretarySpawn==null||floor.secretaryDoor==null||floor.guardSpawns.Length!=5||floor.guardSpawns.Any(p=>p==null)))return "대표층 문/대표/비서/보디가드 시작점 참조를 확인하세요.";
        var data=floor.ReadLayout();var nav=new OfficeNavigation(data.CreateLayout());
        var start=floor.Local(floor.playerSpawn);
        foreach(var target in floor.enemySpawns.Concat(floor.guardSpawns??new Transform[0]))
        {var p=floor.Local(target);var path=nav.Path(start.x,start.z,p.x,p.z);var end=path.Last();if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(end.x,end.z))>.85f)return target.name+": 가구·벽과 겹치거나 길이 막혔습니다.";}
        return "배치 검사 통과. 저장 후 새 게임에서 충돌·문·전투를 확인하세요.";
    }
}
[CustomEditor(typeof(OfficeFloorPrefab))]
public sealed class OfficeFloorPrefabInspector:Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Hierarchy에서 가구·파티션·시작점을 선택하고 W/E/R로 이동·회전·크기를 조정한 뒤 Ctrl+S로 저장하세요. 이 프리팹을 실제 게임이 사용합니다. 시작점 개수와 문 참조는 유지하세요.",MessageType.Info);
        if(GUILayout.Button("배치 / 길찾기 검사"))EditorUtility.DisplayDialog("층 배치 검사",OfficeFloorEditor.Validate((OfficeFloorPrefab)target),"확인");
        DrawDefaultInspector();
    }
}
