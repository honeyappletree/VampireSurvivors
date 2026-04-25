// ================================================================
// ArtImportAutoSetup.cs
// 경로: Assets/Editor/ArtImportAutoSetup.cs
//
// 메뉴: Tools → 죽음유예 → [Step 1] Art Import Full Setup
// ================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

public class ArtImportAutoSetup : EditorWindow
{
    // ── 에셋 정의 ─────────────────────────────────────────────
    struct SpriteInfo
    {
        public string assetPath;
        public int    ppu;
        public float  pivotY;
        public bool   isTile;
        public string characterId;  // MonsterDataSO key
    }

    static readonly SpriteInfo[] SPRITES = new SpriteInfo[]
    {
        new SpriteInfo { assetPath="Assets/Art/Characters/Player/player_default.png",
                         ppu=885,  pivotY=0f, isTile=false, characterId="player" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Enemies/enemy_zombie_green.png",
                         ppu=768,  pivotY=0f, isTile=false, characterId="zombie_green" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Enemies/enemy_skeleton_white.png",
                         ppu=768,  pivotY=0f, isTile=false, characterId="skeleton_white" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Enemies/enemy_hound_skeleton.png",
                         ppu=576,  pivotY=0f, isTile=false, characterId="hound_skeleton" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Enemies/enemy_zombie_red.png",
                         ppu=640,  pivotY=0f, isTile=false, characterId="zombie_red" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Enemies/enemy_ghost_purple.png",
                         ppu=736,  pivotY=0.5f,isTile=false,characterId="ghost_purple" },

        new SpriteInfo { assetPath="Assets/Art/Characters/Boss/boss_commander_immortal.png",
                         ppu=523,  pivotY=0f, isTile=false, characterId="boss_commander" },

        new SpriteInfo { assetPath="Assets/Art/Backgrounds/Tiles/bg_ground_07.png",
                         ppu=100,  pivotY=0.5f,isTile=true, characterId="" },

        new SpriteInfo { assetPath="Assets/Art/Backgrounds/Tiles/bg_ground_08.png",
                         ppu=100,  pivotY=0.5f,isTile=true, characterId="" },
    };

    // ── 메뉴 진입점 ──────────────────────────────────────────
    [MenuItem("Tools/죽음유예/[Step 1] Art Import Full Setup")]
    public static void RunFullSetup()
    {
        int ok = 0, skip = 0;

        foreach (var info in SPRITES)
        {
            if (!File.Exists(info.assetPath))
            {
                Debug.LogWarning($"[Art Setup] 파일 없음: {info.assetPath}");
                skip++;
                continue;
            }

            var importer = AssetImporter.GetAtPath(info.assetPath) as TextureImporter;
            if (importer == null) { skip++; continue; }

            // ── 기본 스프라이트 설정 ──
            importer.textureType          = TextureImporterType.Sprite;
            importer.spriteImportMode     = SpriteImportMode.Single;
            importer.spritePixelsPerUnit  = info.ppu;
            importer.spritePivot          = new Vector2(0.5f, info.pivotY);
            importer.alphaSource          = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency  = true;
            importer.mipmapEnabled        = false;
            importer.sRGBTexture          = true;

            if (info.isTile)
            {
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode   = TextureWrapMode.Repeat;
            }
            else
            {
                importer.filterMode = FilterMode.Point;
                importer.wrapMode   = TextureWrapMode.Clamp;
            }

            // ── 압축 없음 (테스트 단계) ──
            var platform = new TextureImporterPlatformSettings
            {
                name               = "DefaultTexturePlatform",
                maxTextureSize     = 2048,
                textureCompression = TextureImporterCompression.Uncompressed,
                overridden         = false
            };
            importer.SetPlatformTextureSettings(platform);

            importer.SaveAndReimport();
            Debug.Log($"[Art Setup] ✅ {Path.GetFileName(info.assetPath)} | PPU={info.ppu}");
            ok++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"스프라이트 임포트 설정 완료\n성공: {ok}  /  스킵: {skip}\n\n" +
            "다음 단계: [Step 2] Create Monster ScriptableObjects", "확인");
    }

    // ── [Step 2] MonsterDataSO 생성 ──────────────────────────
    [MenuItem("Tools/죽음유예/[Step 2] Create Monster ScriptableObjects")]
    public static void CreateMonsterSOs()
    {
        string soDir = "Assets/Resources/MonsterData";

        // Resources 폴더 없으면 먼저 생성
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            Debug.Log("[SO] Assets/Resources 폴더 생성");
        }
        // MonsterData 폴더 없으면 생성
        if (!AssetDatabase.IsValidFolder(soDir))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "MonsterData");
            Debug.Log($"[SO] 폴더 생성: {soDir}");
        }
        AssetDatabase.Refresh();

        // MonsterDataSO 타입 찾기
        var soType = FindMonsterDataSOType();
        if (soType == null)
        {
            // SO 타입 없으면 ScriptableObject 기본형으로 생성 (스프라이트만 연결)
            Debug.LogWarning("[SO] MonsterDataSO 클래스를 찾을 수 없어 기본 ScriptableObject로 생성합니다.");
            soType = typeof(ScriptableObject);
        }

        // 적 에셋 → SO 매핑
        var mapping = new Dictionary<string, (string path, string displayName, int hp, float spd, int atk, float recover)>
        {
            ["zombie_green"]   = ("Assets/Art/Characters/Enemies/enemy_zombie_green.png",   "녹색 좀비",   60, 1.5f,  8, 3f),
            ["skeleton_white"] = ("Assets/Art/Characters/Enemies/enemy_skeleton_white.png", "흰색 해골",   40, 1.8f,  6, 3f),
            ["hound_skeleton"] = ("Assets/Art/Characters/Enemies/enemy_hound_skeleton.png", "흰색 해골개", 30, 3.5f,  5, 3f),
            ["zombie_red"]     = ("Assets/Art/Characters/Enemies/enemy_zombie_red.png",     "붉은 좀비",  200, 1.0f, 15, 5f),
            ["ghost_purple"]   = ("Assets/Art/Characters/Enemies/enemy_ghost_purple.png",   "보라 유령",   50, 1.2f, 12, 3f),
            ["boss_commander"] = ("Assets/Art/Characters/Boss/boss_commander_immortal.png", "불멸의 사령관",1000,1.0f,25,0f),
        };

        int created = 0;
        foreach (var (id, data) in mapping)
        {
            string soPath = $"{soDir}/{id}_data.asset";

            // 기존 SO가 없으면 생성
            var so = AssetDatabase.LoadAssetAtPath(soPath, soType) as ScriptableObject;
            if (so == null)
            {
                so = ScriptableObject.CreateInstance(soType);
                AssetDatabase.CreateAsset(so, soPath);
                Debug.Log($"[SO] 생성: {soPath}");
            }

            // SerializedObject로 필드 세팅
            var serialized = new SerializedObject(so);

            SetIfExists(serialized, "monsterName",    data.displayName);
            SetIfExists(serialized, "maxHP",          data.hp);
            SetIfExists(serialized, "moveSpeed",      data.spd);
            SetIfExists(serialized, "attackDamage",   data.atk);
            SetIfExists(serialized, "lifeTimeRestore",data.recover);

            // 스프라이트 연결
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(data.path);
            if (sprite != null)
                SetSpriteIfExists(serialized, "idleSprite", sprite);

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(so);
            created++;
            Debug.Log($"[SO] ✅ {data.displayName} SO 설정 완료 (HP={data.hp}, 회복={data.recover}초)");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"MonsterDataSO {created}개 생성/업데이트 완료\n\n" +
            "다음 단계: [Step 3] Create Animator Controllers", "확인");
    }

    // ── [Step 3] Animator Controller 생성 ───────────────────
    [MenuItem("Tools/죽음유예/[Step 3] Create Animator Controllers")]
    public static void CreateAnimatorControllers()
    {
        string animDir = "Assets/Art/Animations";
        if (!AssetDatabase.IsValidFolder(animDir))
            AssetDatabase.CreateFolder("Assets/Art", "Animations");

        var characters = new (string id, string spritePath)[]
        {
            ("Player",          "Assets/Art/Characters/Player/player_default.png"),
            ("ZombieGreen",     "Assets/Art/Characters/Enemies/enemy_zombie_green.png"),
            ("SkeletonWhite",   "Assets/Art/Characters/Enemies/enemy_skeleton_white.png"),
            ("HoundSkeleton",   "Assets/Art/Characters/Enemies/enemy_hound_skeleton.png"),
            ("ZombieRed",       "Assets/Art/Characters/Enemies/enemy_zombie_red.png"),
            ("GhostPurple",     "Assets/Art/Characters/Enemies/enemy_ghost_purple.png"),
            ("BossCommander",   "Assets/Art/Characters/Boss/boss_commander_immortal.png"),
        };

        int created = 0;
        foreach (var (id, spritePath) in characters)
        {
            string controllerPath = $"{animDir}/{id}_Controller.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                Debug.Log($"[Anim] 이미 존재: {id}_Controller");
                continue;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var rootSM     = controller.layers[0].stateMachine;
            var sprite     = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            // ── 파라미터 추가 ──
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDead",   AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack",   AnimatorControllerParameterType.Trigger);

            // ── 스테이트 생성 (현재는 단일 스프라이트 = 같은 스프라이트 사용) ──
            var clip_idle   = CreateSingleFrameClip(sprite, $"{id}_Idle",   true);
            var clip_walk   = CreateSingleFrameClip(sprite, $"{id}_Walk",   true);
            var clip_attack = CreateSingleFrameClip(sprite, $"{id}_Attack", false);
            var clip_death  = CreateSingleFrameClip(sprite, $"{id}_Death",  false);

            // 클립 저장
            foreach (var clip in new[] { clip_idle, clip_walk, clip_attack, clip_death })
                AssetDatabase.AddObjectToAsset(clip, controllerPath);

            var st_idle   = rootSM.AddState("Idle",   new Vector3(-200, 0));
            var st_walk   = rootSM.AddState("Walk",   new Vector3(-200, 80));
            var st_attack = rootSM.AddState("Attack", new Vector3(-200, 160));
            var st_death  = rootSM.AddState("Death",  new Vector3(-200, 240));

            st_idle.motion   = clip_idle;
            st_walk.motion   = clip_walk;
            st_attack.motion = clip_attack;
            st_death.motion  = clip_death;

            rootSM.defaultState = st_idle;

            // ── 트랜지션 ──
            // Idle ↔ Walk
            AddBoolTransition(rootSM, st_idle,   st_walk,   "IsMoving", true,  0f);
            AddBoolTransition(rootSM, st_walk,   st_idle,   "IsMoving", false, 0f);
            // any → Death
            var anyDeath = rootSM.AddAnyStateTransition(st_death);
            anyDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
            anyDeath.hasExitTime        = false;
            anyDeath.duration           = 0f;
            anyDeath.canTransitionToSelf = false;
            // Idle → Attack
            AddTriggerTransition(rootSM, st_idle, st_attack, "Attack", 0f);
            // Attack → Idle
            var t_ai = st_attack.AddTransition(st_idle);
            t_ai.hasExitTime = true; t_ai.exitTime = 1f; t_ai.duration = 0f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            created++;
            Debug.Log($"[Anim] ✅ {id}_Controller 생성 완료");
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료",
            $"Animator Controller {created}개 생성 완료\n\n" +
            "⚠️ 현재 애니메이션은 단일 프레임입니다.\n" +
            "Aseprite 스프라이트 시트 완성 후 클립을 교체하세요.", "확인");
    }

    // ── 헬퍼: 단일 프레임 AnimationClip ─────────────────────
    static AnimationClip CreateSingleFrameClip(Sprite sprite, string clipName, bool loop)
    {
        var clip = new AnimationClip { name = clipName };

        if (sprite != null)
        {
            var binding = new UnityEditor.EditorCurveBinding
            {
                type         = typeof(SpriteRenderer),
                path         = "",
                propertyName = "m_Sprite"
            };
            var kf = new UnityEditor.ObjectReferenceKeyframe[]
            {
                new UnityEditor.ObjectReferenceKeyframe { time=0f, value=sprite }
            };
            UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip, binding, kf);
        }

        if (loop)
        {
            var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            UnityEditor.AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        clip.frameRate = 12f;
        return clip;
    }

    // ── 헬퍼: Bool 트랜지션 ──────────────────────────────────
    static void AddBoolTransition(AnimatorStateMachine sm,
        AnimatorState from, AnimatorState to, string param, bool value, float duration)
    {
        var t = from.AddTransition(to);
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
        t.hasExitTime = false;
        t.duration    = duration;
    }

    // ── 헬퍼: Trigger 트랜지션 ──────────────────────────────
    static void AddTriggerTransition(AnimatorStateMachine sm,
        AnimatorState from, AnimatorState to, string param, float duration)
    {
        var t = from.AddTransition(to);
        t.AddCondition(AnimatorConditionMode.If, 0, param);
        t.hasExitTime = false;
        t.duration    = duration;
    }

    // ── 헬퍼: SerializedProperty 세팅 ───────────────────────
    static void SetIfExists(SerializedObject so, string propName, object value)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) return;
        try
        {
            switch (value)
            {
                case string s:
                    prop.stringValue = s;
                    break;
                case int i:
                    if (prop.propertyType == SerializedPropertyType.Integer)
                        prop.intValue = i;
                    else if (prop.propertyType == SerializedPropertyType.Float)
                        prop.floatValue = (float)i;
                    break;
                case float f:
                    if (prop.propertyType == SerializedPropertyType.Float)
                        prop.floatValue = f;
                    else if (prop.propertyType == SerializedPropertyType.Integer)
                        prop.intValue = (int)f;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SO] {propName} 세팅 스킵: {e.Message}");
        }
    }

    static void SetSpriteIfExists(SerializedObject so, string propName, Sprite sprite)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.objectReferenceValue = sprite;
    }

    // ── 임포터 가져오기 ──────────────────────────────────────
    static TextureImporter GetImporter(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            Debug.LogWarning($"[Setup] 파일 없음: {assetPath}");
        return importer;
    }

    // ── MonsterDataSO 타입 검색 ──────────────────────────────
    static System.Type FindMonsterDataSOType()
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("MonsterDataSO");
            if (t != null) return t;
        }
        return null;
    }
    // ── 데코 오브젝트 임포트 설정 ────────────────────────────
    static void SetupDecoSprites()
    {
        var decos = new (string path, int ppu)[]
        {
            ("Assets/Art/Backgrounds/Decorations/deco_banner_01.png", 300),
            ("Assets/Art/Backgrounds/Decorations/deco_banner_02.png", 300),
            ("Assets/Art/Backgrounds/Decorations/deco_rock_01.png",   250),
            ("Assets/Art/Backgrounds/Decorations/deco_rock_02.png",   250),
            ("Assets/Art/Backgrounds/Decorations/deco_shield_01.png", 350),
            ("Assets/Art/Backgrounds/Decorations/deco_sword_01.png",  350),
            ("Assets/Art/Backgrounds/Decorations/deco_sword_02.png",  350),
        };

        foreach (var (path, ppu) in decos)
        {
            var importer = GetImporter(path);
            if (importer == null) continue;

            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.spritePivot         = new Vector2(0.5f, 0f);
            importer.filterMode          = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.alphaSource         = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled       = false;
            importer.wrapMode            = TextureWrapMode.Clamp;

            var settings = new TextureImporterPlatformSettings
            {
                name               = "DefaultTexturePlatform",
                maxTextureSize     = 2048,
                textureCompression = TextureImporterCompression.Uncompressed,
                overridden         = false
            };
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
            Debug.Log($"[Deco] ✅ {System.IO.Path.GetFileName(path)} | PPU={ppu}");
        }
    }
}
#endif
