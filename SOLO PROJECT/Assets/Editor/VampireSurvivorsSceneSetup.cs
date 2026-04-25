// ============================================================
//  뱀파이어 서바이벌 씬 자동 세팅 도구
//  메뉴: 뱀파이어 서바이벌 ▶ 씬 자동 세팅
// ============================================================
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class VampireSurvivorsSceneSetup
{
    // ── 경로 상수 ──────────────────────────────────────────────────
    const string PREFABS_DIR  = "Assets/Prefabs";
    const string TEXTURES_DIR = "Assets/Textures";

    // CreateSprites() 에서 채워지는 스프라이트 필드
    static Sprite sprPlayer, sprEnemy, sprProjectile, sprXPOrb, sprWhite;

    // =============================================================
    //  메인 진입점
    // =============================================================
    [MenuItem("뱀파이어 서바이벌/씬 자동 세팅 ▶", false, 0)]
    public static void SetupScene()
    {
        bool ok = EditorUtility.DisplayDialog(
            "씬 자동 세팅",
            "뱀파이어 서바이벌 프로토타입 씬을 자동으로 구성합니다.\n\n" +
            "• 프리팹 4종 생성 (Assets/Prefabs)\n" +
            "• 스프라이트 5종 생성 (Assets/Textures)\n" +
            "• 플레이어 / 카메라 / 스포너 배치\n" +
            "• HUD + 레벨업 패널 + 게임오버 패널 생성\n\n" +
            "기존 동명 오브젝트는 삭제 후 재생성됩니다.",
            "세팅 시작", "취소");
        if (!ok) return;

        try
        {
            Step("입력 시스템 설정 중...",    0.05f); SetupInputHandling();
            Step("태그 등록 중...",           0.10f); EnsureTag("Player");
            Step("폴더 생성 중...",           0.15f); EnsureFolders();
            Step("스프라이트 생성 중...",     0.20f); CreateSprites();

            Step("XP 오브 프리팹 생성 중...", 0.30f);
            var xpOrbPrefab      = CreateXPOrbPrefab();
            var projectilePrefab = CreateProjectilePrefab();
            var enemyPrefab      = CreateEnemyPrefab(xpOrbPrefab);
            var playerPrefab     = CreatePlayerPrefab(projectilePrefab);

            Step("씬 오브젝트 배치 중...",    0.60f);
            SetupSceneObjects(playerPrefab, enemyPrefab);

            Step("UI 생성 중...",             0.80f);
            SetupUI();

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            EditorUtility.DisplayDialog("완료!",
                "씬 자동 세팅이 완료되었습니다!\n\n▶ Play 버튼을 눌러 바로 테스트하세요.", "확인");

            Debug.Log("[씬 세팅] ✓ 완료 — Assets/Prefabs, Assets/Textures 확인");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[씬 세팅 오류] {e}");
            EditorUtility.DisplayDialog("오류 발생", $"세팅 중 오류:\n{e.Message}", "확인");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // =============================================================
    //  1. 입력 시스템 — Active Input Handling 을 "Both" 로 설정
    // =============================================================
    static void SetupInputHandling()
    {
        var so   = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop != null && prop.intValue != 2)
        {
            prop.intValue = 2; // 0=Legacy  1=New  2=Both
            so.ApplyModifiedProperties();
            Debug.Log("[씬 세팅] Active Input Handling → Both");
        }
    }

    // =============================================================
    //  2. 태그 등록
    // =============================================================
    static void EnsureTag(string tag)
    {
        var so   = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = so.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        so.ApplyModifiedProperties();
        Debug.Log($"[씬 세팅] 태그 등록: {tag}");
    }

    // =============================================================
    //  3. 폴더 생성
    // =============================================================
    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(PREFABS_DIR))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(TEXTURES_DIR))
            AssetDatabase.CreateFolder("Assets", "Textures");
    }

    // =============================================================
    //  4. 스프라이트 생성 (PNG → Import → Sprite)
    // =============================================================
    static void CreateSprites()
    {
        sprPlayer     = SaveSprite("spr_player",     64, new Color(0.40f, 0.80f, 1.00f), circle: true);
        sprEnemy      = SaveSprite("spr_enemy",      64, new Color(1.00f, 0.28f, 0.28f), circle: true);
        sprProjectile = SaveSprite("spr_projectile", 24, new Color(1.00f, 0.95f, 0.30f), circle: true);
        sprXPOrb      = SaveSprite("spr_xporb",      20, new Color(0.30f, 1.00f, 0.50f), circle: true);
        sprWhite      = SaveSprite("spr_white",       4, Color.white,                     circle: false);
    }

    static Sprite SaveSprite(string fileName, int size, Color color, bool circle)
    {
        string assetPath = $"{TEXTURES_DIR}/{fileName}.png";
        string absPath   = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", assetPath));

        // ── 텍스처 픽셀 생성 ──
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        if (circle)
        {
            float cx = (size - 1) * 0.5f, cy = (size - 1) * 0.5f;
            float r  = size * 0.5f - 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a = Mathf.Clamp01(r - d + 0.5f); // 부드러운 외곽선
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, a));
            }
        }
        else
        {
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
        }
        tex.Apply();

        // ── PNG 저장 → Import → Sprite 설정 ──
        File.WriteAllBytes(absPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(assetPath);

        if (AssetImporter.GetAtPath(assetPath) is TextureImporter imp)
        {
            imp.textureType         = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.alphaIsTransparency = true;
            imp.filterMode          = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    // =============================================================
    //  5. 프리팹 생성
    // =============================================================

    // ── 플레이어 ──
    static GameObject CreatePlayerPrefab(GameObject projectilePrefab)
    {
        string path = $"{PREFABS_DIR}/Player.prefab";
        DeleteAssetIfExists(path);

        var go = new GameObject("Player");
        go.tag = "Player";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprPlayer;
        sr.sortingOrder = 5;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale          = 0f;
        rb.freezeRotation        = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius    = 0.35f;
        col.isTrigger = false; // 물리 충돌 유지

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerStats>();

        var aa = go.AddComponent<AutoAttack>();
        aa.projectilePrefab = projectilePrefab;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── 적 ──
    static GameObject CreateEnemyPrefab(GameObject xpOrbPrefab)
    {
        string path = $"{PREFABS_DIR}/Enemy.prefab";
        DeleteAssetIfExists(path);

        var go = new GameObject("Enemy");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprEnemy;
        sr.sortingOrder = 3;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale          = 0f;
        rb.freezeRotation        = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius    = 0.42f;
        col.isTrigger = true;

        var enemy = go.AddComponent<Enemy>();
        enemy.xpOrbPrefab = xpOrbPrefab;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── 발사체 ──
    static GameObject CreateProjectilePrefab()
    {
        string path = $"{PREFABS_DIR}/Projectile.prefab";
        DeleteAssetIfExists(path);

        var go = new GameObject("Projectile");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprProjectile;
        sr.sortingOrder = 4;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType     = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius    = 0.12f;
        col.isTrigger = true;

        go.AddComponent<Projectile>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ── XP 오브 ──
    static GameObject CreateXPOrbPrefab()
    {
        string path = $"{PREFABS_DIR}/XPOrb.prefab";
        DeleteAssetIfExists(path);

        var go = new GameObject("XPOrb");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprXPOrb;
        sr.sortingOrder = 2;

        go.AddComponent<XPOrb>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // =============================================================
    //  6. 씬 오브젝트 배치
    // =============================================================
    static void SetupSceneObjects(GameObject playerPrefab, GameObject enemyPrefab)
    {
        // ── GameManager ──
        var gmGO = GetOrRecreateEmpty("GameManager");
        EnsureComp<GameManager>(gmGO);

        // ── 배경 (단색 어두운 사각형) ──
        DestroySceneObj("Background");
        var bgGO = new GameObject("Background");
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite       = sprWhite;
        bgSR.color        = new Color(0.08f, 0.08f, 0.12f);
        bgSR.sortingOrder = -100;
        bgGO.transform.localScale = new Vector3(200f, 200f, 1f);

        // ── Player (씬 인스턴스) ──
        DestroySceneObj("Player");
        var playerGO = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerGO.name = "Player";
        playerGO.transform.position = Vector3.zero;
        EditorUtility.SetDirty(playerGO);

        // ── EnemySpawner ──
        var spawnerGO = GetOrRecreateEmpty("EnemySpawner");
        var spawner   = EnsureComp<EnemySpawner>(spawnerGO);
        spawner.enemyPrefab = enemyPrefab;
        EditorUtility.SetDirty(spawnerGO);

        // ── Camera ──
        var camGO = Camera.main?.gameObject
            ?? GameObject.FindGameObjectWithTag("MainCamera");
        if (camGO != null)
        {
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            var cam = camGO.GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographic     = true;
                cam.orthographicSize = 7f;
                cam.backgroundColor  = new Color(0.08f, 0.08f, 0.12f);
            }

            var follow  = EnsureComp<CameraFollow>(camGO);
            follow.target       = playerGO.transform;
            follow.smoothSpeed  = 6f;
            EditorUtility.SetDirty(camGO);
        }

        Selection.activeGameObject = playerGO;
    }

    // =============================================================
    //  7. UI 생성
    // =============================================================
    static void SetupUI()
    {
        DestroySceneObj("Canvas");

        // ── Canvas ──
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // UIManager 컴포넌트
        var uiMgr = EnsureComp<UIManager>(canvasGO);

        // ──────────────────────────────────────────────────────────
        //  생명유지시간 슬라이더 (상단 전체 너비)
        // ──────────────────────────────────────────────────────────
        var lifeSlider = MakeSlider(canvasGO.transform, "LifeTimeSlider",
            new Vector2(0.005f, 0.940f), new Vector2(0.995f, 0.998f),
            fillColor: Color.cyan, bgAlpha: 0.55f);
        lifeSlider.value = 1f;
        uiMgr.lifeTimeSlider = lifeSlider;

        var lifeText = MakeText(canvasGO.transform, "LifeTimeText", "생명유지: 120초",
            new Vector2(0.01f, 0.885f), new Vector2(0.45f, 0.940f),
            fontSize: 26f, align: TextAlignmentOptions.Left);
        lifeText.color = Color.cyan;
        uiMgr.lifeTimeText = lifeText;

        // ──────────────────────────────────────────────────────────
        //  HP 슬라이더 (좌상단)
        // ──────────────────────────────────────────────────────────
        var hpSlider = MakeSlider(canvasGO.transform, "HPSlider",
            new Vector2(0.010f, 0.840f), new Vector2(0.280f, 0.885f),
            fillColor: new Color(0.25f, 0.92f, 0.36f), bgAlpha: 0.55f);
        hpSlider.value = 1f;
        uiMgr.hpSlider = hpSlider;

        var hpText = MakeText(canvasGO.transform, "HPText", "체력: 100/100",
            new Vector2(0.010f, 0.795f), new Vector2(0.280f, 0.840f),
            fontSize: 20f, align: TextAlignmentOptions.Left);
        hpText.color = new Color(0.25f, 0.92f, 0.36f);
        uiMgr.hpText = hpText;

        // ──────────────────────────────────────────────────────────
        //  레벨 텍스트 + XP 슬라이더 (우상단)
        // ──────────────────────────────────────────────────────────
        var levelText = MakeText(canvasGO.transform, "LevelText", "Lv.1",
            new Vector2(0.720f, 0.885f), new Vector2(0.998f, 0.998f),
            fontSize: 38f, align: TextAlignmentOptions.Right);
        levelText.fontStyle = FontStyles.Bold;
        levelText.color = new Color(1f, 0.92f, 0.3f);
        uiMgr.levelText = levelText;

        var xpSlider = MakeSlider(canvasGO.transform, "XPSlider",
            new Vector2(0.720f, 0.840f), new Vector2(0.998f, 0.885f),
            fillColor: new Color(0.9f, 0.85f, 0.2f), bgAlpha: 0.55f);
        xpSlider.value = 0f;
        uiMgr.xpSlider = xpSlider;

        // ──────────────────────────────────────────────────────────
        //  웨이브 알림 (화면 중앙, 초기 비활성)
        // ──────────────────────────────────────────────────────────
        var waveText = MakeText(canvasGO.transform, "WaveNotification", "── 웨이브 1 ──",
            new Vector2(0.25f, 0.54f), new Vector2(0.75f, 0.65f),
            fontSize: 52f, align: TextAlignmentOptions.Center);
        waveText.fontStyle = FontStyles.Bold;
        waveText.color     = new Color(1f, 0.75f, 0.2f);
        waveText.gameObject.AddComponent<CanvasGroup>().alpha = 1f;
        waveText.gameObject.SetActive(false); // UIManager.Start 에서 제어
        uiMgr.waveNotificationText = waveText;

        // ──────────────────────────────────────────────────────────
        //  게임 오버 패널 (중앙, 초기 비활성 — UIManager.Awake 에서 처리)
        // ──────────────────────────────────────────────────────────
        var goPanel = MakePanel(canvasGO.transform, "GameOverPanel",
            new Color(0f, 0f, 0f, 0.88f),
            new Vector2(0.20f, 0.20f), new Vector2(0.80f, 0.80f));

        var goTitle = MakeText(goPanel.transform, "Title", "게임 오버",
            new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.97f),
            fontSize: 72f, align: TextAlignmentOptions.Center);
        goTitle.fontStyle = FontStyles.Bold;
        goTitle.color     = new Color(1f, 0.25f, 0.25f);

        var goLevelText = MakeText(goPanel.transform, "ResultText",
            "Lv.1 도달\n생존 시간이 끝났습니다.",
            new Vector2(0.05f, 0.33f), new Vector2(0.95f, 0.65f),
            fontSize: 30f, align: TextAlignmentOptions.Center);
        goLevelText.color = new Color(0.85f, 0.85f, 0.85f);
        uiMgr.gameOverPanel    = goPanel;
        uiMgr.gameOverLevelText = goLevelText;

        var restartBtn = MakeButton(goPanel.transform, "RestartButton", "다시 시작",
            new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.28f),
            new Color(0.20f, 0.50f, 0.90f));
        uiMgr.restartButton = restartBtn;

        // ──────────────────────────────────────────────────────────
        //  레벨업 패널 (LevelUpPanel.Awake 가 게임 시작 시 자동으로 숨김)
        //  ※ 씬에서 Active 상태로 두어야 Awake → Instance 등록이 됩니다
        // ──────────────────────────────────────────────────────────
        var lvPanel = MakePanel(canvasGO.transform, "LevelUpPanel",
            new Color(0.04f, 0.04f, 0.14f, 0.97f),
            new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.90f));
        var lvComp = lvPanel.AddComponent<LevelUpPanel>();

        var lvHeader = MakeText(lvPanel.transform, "Header",
            "레벨 업!\n능력을 선택하세요",
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.99f),
            fontSize: 46f, align: TextAlignmentOptions.Center);
        lvHeader.fontStyle = FontStyles.Bold;
        lvHeader.color     = new Color(1f, 0.88f, 0.25f);
        lvComp.headerText  = lvHeader;

        // ── 능력 선택 버튼 3개 ──
        var optBtns   = new Button[3];
        var optTitles = new TextMeshProUGUI[3];
        var optDescs  = new TextMeshProUGUI[3];

        float[] xMin = { 0.02f, 0.36f, 0.69f };
        float[] xMax = { 0.34f, 0.67f, 0.98f };

        for (int i = 0; i < 3; i++)
        {
            // 버튼 패널
            var btnGO  = MakePanel(lvPanel.transform, $"OptionPanel{i}",
                new Color(0.14f, 0.14f, 0.32f, 1f),
                new Vector2(xMin[i], 0.04f), new Vector2(xMax[i], 0.74f));

            var img    = btnGO.GetComponent<Image>();
            var btn    = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor      = new Color(0.14f, 0.14f, 0.32f);
            colors.highlightedColor = new Color(0.24f, 0.24f, 0.52f);
            colors.pressedColor     = new Color(0.34f, 0.34f, 0.62f);
            btn.colors = colors;

            // 능력 이름
            var titleTMP = MakeText(btnGO.transform, "OptionTitle", "능력명",
                new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.95f),
                fontSize: 34f, align: TextAlignmentOptions.Center);
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color     = Color.white;

            // 능력 설명
            var descTMP = MakeText(btnGO.transform, "OptionDesc", "능력 설명",
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.62f),
                fontSize: 24f, align: TextAlignmentOptions.Center);
            descTMP.color           = new Color(0.78f, 0.84f, 1f);
            descTMP.enableWordWrapping = true;

            optBtns[i]   = btn;
            optTitles[i] = titleTMP;
            optDescs[i]  = descTMP;
        }

        lvComp.optionButtons = optBtns;
        lvComp.optionTitles  = optTitles;
        lvComp.optionDescs   = optDescs;

        // ── EventSystem (없을 때만 생성) ──
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EditorUtility.SetDirty(canvasGO);
    }

    // =============================================================
    //  UI 헬퍼 — Slider
    // =============================================================
    static Slider MakeSlider(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color fillColor, float bgAlpha = 0.6f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchors(rt, anchorMin, anchorMax);

        // 배경
        var bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, bgAlpha);
        Stretch(bg.GetComponent<RectTransform>());

        // Fill Area (앵커를 전체로 설정해 0 ~ max 사이를 Slider 가 조절)
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.offsetMin = Vector2.zero;
        faRT.offsetMax = Vector2.zero;

        // Fill
        var fill    = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRT  = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Slider 컴포넌트
        var slider          = go.AddComponent<Slider>();
        slider.fillRect     = fillRT;
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.value        = 1f;
        slider.interactable = false;
        slider.transition   = Selectable.Transition.None;

        return slider;
    }

    // =============================================================
    //  UI 헬퍼 — TextMeshProUGUI
    // =============================================================
    static TextMeshProUGUI MakeText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax,
        float fontSize, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchors(rt, anchorMin, anchorMax);

        var tmp               = ObjectFactory.AddComponent<TextMeshProUGUI>(go);
        tmp.text              = text;
        tmp.fontSize          = fontSize;
        tmp.alignment         = align;
        tmp.color             = Color.white;
        tmp.enableWordWrapping = true;
        tmp.overflowMode      = TextOverflowModes.Overflow;
        return tmp;
    }

    // =============================================================
    //  UI 헬퍼 — Button
    // =============================================================
    static Button MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color normalColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        SetAnchors(rt, anchorMin, anchorMax);

        var img   = go.AddComponent<Image>();
        img.color = normalColor;

        var btn   = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor      = normalColor;
        colors.highlightedColor = normalColor * 1.3f;
        colors.pressedColor     = normalColor * 0.75f;
        btn.colors = colors;

        MakeText(go.transform, "Label", label,
            new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f),
            fontSize: 28f, align: TextAlignmentOptions.Center);

        return btn;
    }

    // =============================================================
    //  UI 헬퍼 — Panel (Image)
    // =============================================================
    static GameObject MakePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        SetAnchors(rt, anchorMin, anchorMax);
        go.AddComponent<Image>().color = color;
        return go;
    }

    // =============================================================
    //  RectTransform 유틸리티
    // =============================================================
    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot     = new Vector2(0.5f, 0.5f);
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // =============================================================
    //  씬 / 에셋 유틸리티
    // =============================================================
    static void DeleteAssetIfExists(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);
    }

    static void DestroySceneObj(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null) Object.DestroyImmediate(obj);
    }

    static GameObject GetOrRecreateEmpty(string name)
    {
        DestroySceneObj(name);
        return new GameObject(name);
    }

    static T EnsureComp<T>(GameObject go) where T : Component
    {
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }

    static void Step(string msg, float progress) =>
        EditorUtility.DisplayProgressBar("뱀파이어 서바이벌 씬 세팅", msg, progress);
}
