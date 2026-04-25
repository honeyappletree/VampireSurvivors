// ================================================================
// BackgroundSetup.cs
// 경로: Assets/Editor/BackgroundSetup.cs
// ================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class BackgroundSetup : EditorWindow
{
    [MenuItem("Tools/죽음유예/[Step 5] Build Background Tiles")]
    static void BuildBackgroundTiles()
    {
        // BackgroundManager 찾기 또는 생성
        var bg = Object.FindObjectOfType<BackgroundTileRenderer>();
        if (bg == null)
        {
            var go = new GameObject("BackgroundManager");
            bg = go.AddComponent<BackgroundTileRenderer>();
            Debug.Log("[BGSetup] BackgroundManager GameObject 생성");
        }

        // 타일 스프라이트 자동 연결 (bg_ground_base_01)
        string path = "Assets/Sprites/Background/Tiles/bg_ground_base_01.png";
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp == null)
        {
            EditorUtility.DisplayDialog("오류",
                "타일 스프라이트를 찾지 못했습니다.\n" +
                "Assets/Sprites/Background/Tiles/ 폴더에\n" +
                "bg_ground_base_01.png 가 있는지 확인하세요.", "확인");
            return;
        }

        Debug.Log($"[BGSetup] 타일 연결: {path}");
        bg.tileSprite = sp;

        // Scene 변경 표시
        // (타일은 런타임 Start/Update에서 자동 생성)
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("완료",
            "배경 타일 연결 완료\n" +
            "Ctrl+S 로 Scene을 저장하세요.", "확인");
    }
}
#endif
