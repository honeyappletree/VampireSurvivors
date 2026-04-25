// ================================================================
// SceneAutoSetup.cs
// 경로: Assets/Editor/SceneAutoSetup.cs
//
// Tools/죽음유예/[Step 4] Scene Auto Setup 을 실행하면
// BackgroundManager, DecorationManager, Player 스프라이트,
// MonsterData idleSprite 를 자동으로 씬에 연결한다.
// ================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SceneAutoSetup
{
    // ── 아트 경로 상수 ──────────────────────────────────────────

    private const string BG_TILE_PATH   = "Assets/Art/Backgrounds/Tiles/";
    private const string DECO_PATH      = "Assets/Art/Backgrounds/Decorations/";
    private const string PLAYER_SPRITE  = "Assets/Art/Characters/Player/player_default.png";
    private const string ENEMY_PATH     = "Assets/Art/Characters/Enemies/";
    private const string BOSS_PATH      = "Assets/Art/Characters/Boss/";
    private const string MONSTER_DATA   = "Assets/Resources/MonsterData/";

    // ── 메인 메뉴 엔트리 ─────────────────────────────────────────

    [MenuItem("Tools/죽음유예/[Step 4] Scene Auto Setup")]
    private static void RunSetup()
    {
        int warnings = 0;

        warnings += SetupBackgroundManager();
        warnings += SetupDecorationManager();
        warnings += SetupPlayerSprite();
        warnings += SetupMonsterDataSprites();

        string result = warnings == 0
            ? "모든 작업이 경고 없이 완료되었습니다."
            : $"{warnings}개 경고가 있습니다. Console 창을 확인하세요.";

        EditorUtility.DisplayDialog(
            "[Step 4] Scene Auto Setup 완료",
            result,
            "확인"
        );

        Debug.Log($"[SceneAutoSetup] ✔ 전체 완료 — {result}");
    }

    // ================================================================
    // 작업 1 : BackgroundManager + BackgroundTileRenderer
    // ================================================================

    private static int SetupBackgroundManager()
    {
        const string GO_NAME = "BackgroundManager";
        int w = 0;

        // GameObject 확보
        GameObject go = GameObject.Find(GO_NAME);
        if (go == null)
        {
            go = new GameObject(GO_NAME);
            Debug.Log($"[SceneAutoSetup] 작업 1 — '{GO_NAME}' GameObject 생성");
        }
        else
        {
            Debug.Log($"[SceneAutoSetup] 작업 1 — '{GO_NAME}' 기존 GameObject 사용");
        }

        // 컴포넌트 확보
        BackgroundTileRenderer comp = go.GetComponent<BackgroundTileRenderer>();
        if (comp == null)
        {
            comp = go.AddComponent<BackgroundTileRenderer>();
            Debug.Log("[SceneAutoSetup] 작업 1 — BackgroundTileRenderer 컴포넌트 추가");
        }

        // tileSprites 배열 채우기 (bg_ground_01 ~ bg_ground_08)
        var sprites = new List<Sprite>();
        for (int i = 1; i <= 8; i++)
        {
            string path = $"{BG_TILE_PATH}bg_ground_{i:D2}.png";
            Sprite  sp  = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null)
            {
                sprites.Add(sp);
            }
            else
            {
                Debug.LogWarning($"[SceneAutoSetup] 작업 1 — 스프라이트 없음: {path}");
                w++;
            }
        }

        // SerializedObject 를 통해 private 배열 직접 할당
        SerializedObject   so   = new SerializedObject(comp);
        SerializedProperty prop = so.FindProperty("tileSprites");
        if (prop != null)
        {
            prop.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[SceneAutoSetup] 작업 1 — tileSprites {sprites.Count}개 연결 완료");
        }
        else
        {
            Debug.LogWarning("[SceneAutoSetup] 작업 1 — BackgroundTileRenderer.tileSprites 프로퍼티를 찾지 못했습니다.");
            w++;
        }

        EditorUtility.SetDirty(go);
        return w;
    }

    // ================================================================
    // 작업 2 : DecorationManager + DecorationPlacer (3 그룹)
    // ================================================================

    private static int SetupDecorationManager()
    {
        const string GO_NAME = "DecorationManager";
        int w = 0;

        // GameObject 확보
        GameObject go = GameObject.Find(GO_NAME);
        if (go == null)
        {
            go = new GameObject(GO_NAME);
            Debug.Log($"[SceneAutoSetup] 작업 2 — '{GO_NAME}' GameObject 생성");
        }
        else
        {
            Debug.Log($"[SceneAutoSetup] 작업 2 — '{GO_NAME}' 기존 GameObject 사용");
        }

        // 컴포넌트 확보
        DecorationPlacer comp = go.GetComponent<DecorationPlacer>();
        if (comp == null)
        {
            comp = go.AddComponent<DecorationPlacer>();
            Debug.Log("[SceneAutoSetup] 작업 2 — DecorationPlacer 컴포넌트 추가");
        }

        // 그룹 정의
        var groupDefs = new (string name, string[] files, int count)[]
        {
            ("Rocks",   new[] { "deco_rock_01.png",   "deco_rock_02.png" },                          8),
            ("Banners", new[] { "deco_banner_01.png", "deco_banner_02.png" },                        4),
            ("Props",   new[] { "deco_shield_01.png", "deco_sword_01.png", "deco_sword_02.png" },    5),
        };

        SerializedObject   so        = new SerializedObject(comp);
        SerializedProperty groupsProp = so.FindProperty("groups");

        if (groupsProp == null)
        {
            Debug.LogWarning("[SceneAutoSetup] 작업 2 — DecorationPlacer.groups 프로퍼티를 찾지 못했습니다.");
            return w + 1;
        }

        groupsProp.arraySize = groupDefs.Length;

        for (int g = 0; g < groupDefs.Length; g++)
        {
            var (gName, files, count) = groupDefs[g];
            SerializedProperty elem = groupsProp.GetArrayElementAtIndex(g);

            elem.FindPropertyRelative("groupName").stringValue = gName;
            elem.FindPropertyRelative("count").intValue        = count;

            SerializedProperty spritesProp = elem.FindPropertyRelative("sprites");
            spritesProp.arraySize = files.Length;

            for (int s = 0; s < files.Length; s++)
            {
                string path = $"{DECO_PATH}{files[s]}";
                Sprite sp   = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null)
                {
                    spritesProp.GetArrayElementAtIndex(s).objectReferenceValue = sp;
                }
                else
                {
                    Debug.LogWarning($"[SceneAutoSetup] 작업 2 — 스프라이트 없음: {path}");
                    w++;
                }
            }

            Debug.Log($"[SceneAutoSetup] 작업 2 — 그룹 [{g}] '{gName}' 설정 완료 (count={count}, sprites={files.Length})");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(go);
        Debug.Log("[SceneAutoSetup] 작업 2 — DecorationPlacer 3개 그룹 연결 완료");
        return w;
    }

    // ================================================================
    // 작업 3 : Player 스프라이트 설정
    // ================================================================

    private static int SetupPlayerSprite()
    {
        int w = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SceneAutoSetup] 작업 3 — 'Player' 태그 GameObject를 씬에서 찾지 못했습니다. 건너뜁니다.");
            return w + 1;
        }

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("[SceneAutoSetup] 작업 3 — Player에 SpriteRenderer가 없습니다. 건너뜁니다.");
            return w + 1;
        }

        Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(PLAYER_SPRITE);
        if (sp == null)
        {
            Debug.LogWarning($"[SceneAutoSetup] 작업 3 — 스프라이트 없음: {PLAYER_SPRITE}");
            return w + 1;
        }

        SerializedObject   so   = new SerializedObject(sr);
        SerializedProperty prop = so.FindProperty("m_Sprite");
        prop.objectReferenceValue = sp;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);

        Debug.Log($"[SceneAutoSetup] 작업 3 — Player SpriteRenderer 스프라이트 설정 완료 ({sp.name})");
        return w;
    }

    // ================================================================
    // 작업 4 : MonsterData idleSprite 연결
    // ================================================================

    private static int SetupMonsterDataSprites()
    {
        // asset 파일명(확장자 제외) → 스프라이트 파일 경로
        var mapping = new Dictionary<string, string>
        {
            { "zombie_green_data",    $"{ENEMY_PATH}enemy_zombie_green.png"         },
            { "skeleton_white_data",  $"{ENEMY_PATH}enemy_skeleton_white.png"       },
            { "hound_skeleton_data",  $"{ENEMY_PATH}enemy_hound_skeleton.png"       },
            { "zombie_red_data",      $"{ENEMY_PATH}enemy_zombie_red.png"           },
            { "ghost_purple_data",    $"{ENEMY_PATH}enemy_ghost_purple.png"         },
            { "boss_commander_data",  $"{BOSS_PATH}boss_commander_immortal.png"     },
        };

        int w = 0;

        foreach (var kv in mapping)
        {
            string assetPath  = $"{MONSTER_DATA}{kv.Key}.asset";
            string spritePath = kv.Value;

            MonsterDataSO data = AssetDatabase.LoadAssetAtPath<MonsterDataSO>(assetPath);
            if (data == null)
            {
                Debug.LogWarning($"[SceneAutoSetup] 작업 4 — asset 없음: {assetPath}");
                w++;
                continue;
            }

            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp == null)
            {
                Debug.LogWarning($"[SceneAutoSetup] 작업 4 — 스프라이트 없음: {spritePath}");
                w++;
                continue;
            }

            SerializedObject   so   = new SerializedObject(data);
            SerializedProperty prop = so.FindProperty("idleSprite");
            if (prop == null)
            {
                Debug.LogWarning($"[SceneAutoSetup] 작업 4 — MonsterDataSO.idleSprite 프로퍼티를 찾지 못했습니다. ({kv.Key})");
                w++;
                continue;
            }

            prop.objectReferenceValue = sp;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);

            Debug.Log($"[SceneAutoSetup] 작업 4 — {kv.Key} ← {sp.name} 연결 완료");
        }

        // 변경된 에셋 저장
        AssetDatabase.SaveAssets();
        Debug.Log("[SceneAutoSetup] 작업 4 — MonsterData idleSprite 전체 연결 완료");
        return w;
    }
}
