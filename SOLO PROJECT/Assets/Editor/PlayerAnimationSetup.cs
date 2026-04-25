using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Tools → Player → Setup Animations 메뉴 실행 시:
///   Assets/Animations/ 에 Player_Idle / Player_Walk / Player_Death 클립과
///   Player_Controller.controller 를 생성하고, Player.prefab 의 Animator 에 연결한다.
///
/// 전제 조건:
///   - 스프라이트 시트가 이미 Sprite Mode = Multiple 로 임포트되어 있어야 한다.
///   - player_default_full_sheet.png 가 없을 경우 player_sprite_sheet.png 를 사용한다.
///     (Death 슬라이스 14~15 는 full_sheet 추가 후 재실행 필요)
/// </summary>
public static class PlayerAnimationSetup
{
    const string FULL_SHEET_PATH  = "Assets/Sprites/Characters/Player/player_default_full_sheet.png";
    const string SHEET_PATH       = "Assets/Sprites/Characters/Player/player_sprite_sheet.png";
    const string ANIM_FOLDER      = "Assets/Animations";
    const string CONTROLLER_PATH  = "Assets/Animations/Player_Controller.controller";
    const string PREFAB_PATH      = "Assets/Prefabs/Player.prefab";

    [MenuItem("Tools/Player/Setup Animations")]
    public static void Setup()
    {
        // ── 1. 스프라이트 시트 결정 ───────────────────────────────────
        string sheetPath = System.IO.File.Exists(
            System.IO.Path.Combine(Application.dataPath, "..", FULL_SHEET_PATH))
            ? FULL_SHEET_PATH : SHEET_PATH;

        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(s =>
            {
                var parts = s.name.Split('_');
                return int.TryParse(parts[parts.Length - 1], out int n) ? n : 0;
            })
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"[PlayerAnimationSetup] 스프라이트를 찾을 수 없습니다: {sheetPath}\n" +
                           "Sprite Mode = Multiple 로 임포트한 뒤 재실행하세요.");
            return;
        }

        Debug.Log($"[PlayerAnimationSetup] 스프라이트 시트: {sheetPath} ({sprites.Length}장)");

        // ── 2. Animations 폴더 보장 ───────────────────────────────────
        if (!AssetDatabase.IsValidFolder(ANIM_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Animations");

        // ── 3. 애니메이션 클립 생성 ───────────────────────────────────
        // Idle : 슬라이스 0~3  , 8 FPS, 루프 ON
        // Walk : 슬라이스 6~11 , 10 FPS, 루프 ON
        // Death: 슬라이스 12~15, 8 FPS, 루프 OFF (시트에 없는 인덱스는 마지막 프레임으로 대체)
        var idle  = CreateClip(sprites, "Player_Idle",   0,  3,  8f,  loop: true);
        var walk  = CreateClip(sprites, "Player_Walk",   6, 11, 10f,  loop: true);
        var death = CreateClip(sprites, "Player_Death", 12, 15,  8f,  loop: false);

        // ── 4. AnimatorController 생성 ────────────────────────────────
        var controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

        controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isDead",   AnimatorControllerParameterType.Bool);

        var rootSM = controller.layers[0].stateMachine;

        var stateIdle  = rootSM.AddState("Idle");
        var stateWalk  = rootSM.AddState("Walk");
        var stateDeath = rootSM.AddState("Death");

        stateIdle.motion  = idle;
        stateWalk.motion  = walk;
        stateDeath.motion = death;
        rootSM.defaultState = stateIdle;

        // Idle → Walk : isMoving = true, 즉시 전환
        var t1 = stateIdle.AddTransition(stateWalk);
        t1.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
        t1.hasExitTime = false;
        t1.duration    = 0f;

        // Walk → Idle : isMoving = false, 즉시 전환
        var t2 = stateWalk.AddTransition(stateIdle);
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
        t2.hasExitTime = false;
        t2.duration    = 0f;

        // Any → Death : isDead = true, 즉시 전환 (자기 자신 제외)
        var t3 = rootSM.AddAnyStateTransition(stateDeath);
        t3.AddCondition(AnimatorConditionMode.If, 0, "isDead");
        t3.hasExitTime         = false;
        t3.duration            = 0f;
        t3.canTransitionToSelf = false;

        EditorUtility.SetDirty(controller);

        // ── 5. Player.prefab Animator 에 컨트롤러 연결 ───────────────
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        if (prefab != null)
        {
            var anim = prefab.GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogWarning("[PlayerAnimationSetup] Player.prefab 에 Animator 컴포넌트가 없습니다. " +
                                 "수동으로 추가한 뒤 재실행하세요.");
            }
            else
            {
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(prefab);
                Debug.Log("[PlayerAnimationSetup] Player.prefab Animator 에 Player_Controller 연결 완료");
            }
        }
        else
        {
            Debug.LogWarning($"[PlayerAnimationSetup] {PREFAB_PATH} 를 찾을 수 없습니다.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[PlayerAnimationSetup] 완료\n" +
                  $"  Idle  : 슬라이스 0~3,   8 FPS, 루프 ON\n" +
                  $"  Walk  : 슬라이스 6~11,  10 FPS, 루프 ON\n" +
                  $"  Death : 슬라이스 12~15,  8 FPS, 루프 OFF\n" +
                  $"  컨트롤러: {CONTROLLER_PATH}");
    }

    // ── 헬퍼: 애니메이션 클립 생성 ────────────────────────────────────
    static AnimationClip CreateClip(Sprite[] sprites, string clipName,
                                    int fromIdx, int toIdx, float fps, bool loop)
    {
        var clip = new AnimationClip { name = clipName, frameRate = fps };

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        int clampedTo = Mathf.Min(toIdx, sprites.Length - 1);
        int count     = clampedTo - fromIdx + 1;

        if (count <= 0)
        {
            Debug.LogWarning($"[PlayerAnimationSetup] {clipName}: 유효한 슬라이스 없음 (from={fromIdx}, sprites={sprites.Length})");
        }
        else
        {
            if (toIdx > sprites.Length - 1)
                Debug.LogWarning($"[PlayerAnimationSetup] {clipName}: 슬라이스 {sprites.Length}~{toIdx} 미존재 → " +
                                 "마지막 프레임({sprites[clampedTo].name})으로 대체. full_sheet 추가 후 재실행 필요.");

            var frames = new ObjectReferenceKeyframe[count];
            float interval = 1f / fps;

            for (int i = 0; i < count; i++)
            {
                frames[i] = new ObjectReferenceKeyframe
                {
                    time  = i * interval,
                    value = sprites[fromIdx + i]
                };
            }

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        }

        string path = $"{ANIM_FOLDER}/{clipName}.anim";
        AssetDatabase.DeleteAsset(path);          // 이미 있으면 덮어쓰기
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }
}
