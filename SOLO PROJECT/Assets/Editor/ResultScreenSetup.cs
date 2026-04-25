// ============================================================
//  결과 화면 씬 자동 세팅 도구
//  메뉴: 뱀파이어 서바이벌 ▶ 결과 화면 세팅
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class ResultScreenSetup
{
    // ── 경로 상수 ─────────────────────────────────────────────
    const string SCENE_PATH = "Assets/Scenes/ResultScreen.unity";

    // ── 색상 상수 ─────────────────────────────────────────────
    static readonly Color ColBg       = new Color(0.102f, 0.102f, 0.102f, 1f);    // #1A1A1A 어두운 배경
    static readonly Color ColPanel    = new Color(0.145f, 0.145f, 0.145f, 0.97f); // #252525 결과 패널
    static readonly Color ColBtn      = new Color(0.180f, 0.180f, 0.180f, 1f);    // #2E2E2E 버튼 배경
    static readonly Color ColBtnHover = new Color(0.243f, 0.243f, 0.243f, 1f);    // #3E3E3E 호버
    static readonly Color ColBtnPress = new Color(0.118f, 0.118f, 0.118f, 1f);    // #1E1E1E 클릭
    static readonly Color ColTitle    = new Color(0.753f, 0.224f, 0.169f, 1f);    // #C0392B 붉은 타이틀
    static readonly Color ColDivider  = new Color(0.3f,   0.3f,   0.3f,   1f);    // 구분선

    // =============================================================
    //  메인 진입점
    // =============================================================
    [MenuItem("뱀파이어 서바이벌/결과 화면 세팅", false, 25)]
    public static void CreateResultScene()
    {
        bool ok = EditorUtility.DisplayDialog(
            "결과 화면 세팅",
            "Assets/Scenes/ResultScreen.unity 씬을 자동으로 생성합니다.\n\n" +
            "• Canvas 생성\n" +
            "• 생존시간 / 처치수 / 최고레벨 / 획득재화 텍스트 4개\n" +
            "• 다시 시작 / 메인 메뉴로 버튼\n" +
            "• ResultScreen 컴포넌트 자동 연결\n\n" +
            "진행하면 현재 씬 저장 여부를 묻습니다.",
            "세팅 시작", "취소");
        if (!ok) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        try
        {
            Step("씬 생성 중...", 0.10f);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Step("카메라 / EventSystem 생성 중...", 0.20f);
            SetupCamera();
            SetupEventSystem();

            Step("Canvas 및 UI 생성 중...", 0.40f);
            var canvas = CreateCanvas();
            var tf     = canvas.transform;

            // 배경 어두운 패널 (전체 화면)
            CreateBackground(tf);

            // 타이틀
            CreateTitleText(tf);

            // 결과 수치 패널
            var statsPanel = CreateStatsPanel(tf);

            // 수치 텍스트 4개 생성
            var survivalTimeText = CreateStatText(statsPanel.transform, "SurvivalTimeText",
                "생존 시간  --분 --초", new Vector2(0,  90));
            var killCountText    = CreateStatText(statsPanel.transform, "KillCountText",
                "처치 수  --",          new Vector2(0,  30));
            var maxLevelText     = CreateStatText(statsPanel.transform, "MaxLevelText",
                "최고 레벨  Lv.--",     new Vector2(0, -30));
            var currencyText     = CreateStatText(statsPanel.transform, "CurrencyText",
                "획득 재화  --",        new Vector2(0, -90));

            // 구분선
            CreateDivider(tf, new Vector2(0, -130), new Vector2(440, 2));

            // 버튼 2개
            Step("버튼 생성 중...", 0.65f);
            var restartButton  = CreateStyledButton(tf, "RestartButton",  "다시 시작",
                new Vector2(-110, -215), new Vector2(200, 60));
            var mainMenuButton = CreateStyledButton(tf, "MainMenuButton", "메인 메뉴로",
                new Vector2( 110, -215), new Vector2(200, 60));

            // ResultScreen 컴포넌트 연결
            Step("ResultScreen 컴포넌트 연결 중...", 0.80f);
            var controllerGo  = new GameObject("ResultScreen");
            var resultScreen  = controllerGo.AddComponent<ResultScreen>();

            var so = new SerializedObject(resultScreen);
            so.FindProperty("survivalTimeText").objectReferenceValue = survivalTimeText;
            so.FindProperty("killCountText")   .objectReferenceValue = killCountText;
            so.FindProperty("maxLevelText")    .objectReferenceValue = maxLevelText;
            so.FindProperty("currencyText")    .objectReferenceValue = currencyText;
            so.FindProperty("restartButton")   .objectReferenceValue = restartButton;
            so.FindProperty("mainMenuButton")  .objectReferenceValue = mainMenuButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            Step("씬 저장 중...", 0.90f);
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(SCENE_PATH);

            EditorUtility.DisplayDialog("완료!",
                $"ResultScreen.unity 씬이 생성되었습니다.\n경로: {SCENE_PATH}\n\n" +
                "Build Settings에도 자동 추가되었습니다.", "확인");

            Debug.Log($"[결과 화면 세팅] ✓ 완료 — {SCENE_PATH}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[결과 화면 세팅 오류] {e}");
            EditorUtility.DisplayDialog("오류 발생", $"세팅 중 오류:\n{e.Message}", "확인");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // =============================================================
    //  카메라 / EventSystem
    // =============================================================

    static void SetupCamera()
    {
        var go  = new GameObject("Main Camera");
        go.tag  = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColBg;
        cam.orthographic    = true;
        cam.depth           = -1;
        go.AddComponent<AudioListener>();
    }

    static void SetupEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    // =============================================================
    //  Canvas
    // =============================================================

    static GameObject CreateCanvas()
    {
        var go     = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    // =============================================================
    //  UI 요소 빌더
    // =============================================================

    /// <summary>전체 화면 어두운 배경 이미지</summary>
    static void CreateBackground(Transform parent)
    {
        var go = new GameObject("Background");
        go.transform.SetParent(parent, false);

        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img   = go.AddComponent<Image>();
        img.color = ColBg;
        img.raycastTarget = false;
    }

    /// <summary>상단 타이틀 "결과"</summary>
    static void CreateTitleText(Transform parent)
    {
        var go = new GameObject("TitleText");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 1f);
        rt.anchorMax        = new Vector2(0.5f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -80);
        rt.sizeDelta        = new Vector2(600, 110);

        var tmp       = ObjectFactory.AddComponent<TextMeshProUGUI>(go);
        tmp.text      = "결  과";
        tmp.fontSize  = 72;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = ColTitle;          // 명시적 흰색(타이틀은 붉은 계열)
        tmp.fontStyle = FontStyles.Bold;
    }

    /// <summary>수치 텍스트 4개를 담는 패널</summary>
    static GameObject CreateStatsPanel(Transform parent)
    {
        var go = new GameObject("StatsPanel");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta        = new Vector2(480, 260);

        var img   = go.AddComponent<Image>();
        img.color = ColPanel;

        return go;
    }

    /// <summary>수치 한 줄 텍스트 (레이블 + 값 형식)</summary>
    static TextMeshProUGUI CreateStatText(Transform parent, string name,
        string defaultText, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(420, 46);

        var tmp       = ObjectFactory.AddComponent<TextMeshProUGUI>(go);
        tmp.text      = defaultText;
        tmp.fontSize  = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;   // 명시적 흰색
        return tmp;
    }

    /// <summary>구분선 (얇은 이미지)</summary>
    static void CreateDivider(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Divider");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var img   = go.AddComponent<Image>();
        img.color = ColDivider;
        img.raycastTarget = false;
    }

    /// <summary>스타일이 통일된 버튼 생성</summary>
    static Button CreateStyledButton(Transform parent, string name, string label,
                                     Vector2 pos, Vector2 size)
    {
        // ─ 루트 ─
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var img   = go.AddComponent<Image>();
        img.color = ColBtn;

        var btn           = go.AddComponent<Button>();
        btn.targetGraphic = img;   // ColorTint 가 텍스트에 영향을 주지 않도록 Image 지정
        var cb            = btn.colors;
        cb.normalColor      = ColBtn;
        cb.highlightedColor = ColBtnHover;
        cb.pressedColor     = ColBtnPress;
        cb.selectedColor    = ColBtn;
        cb.fadeDuration     = 0.08f;
        btn.colors          = cb;

        // ─ 텍스트 ─
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        var textRT       = textGo.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var tmp       = ObjectFactory.AddComponent<TextMeshProUGUI>(textGo);
        tmp.text      = label;
        tmp.fontSize  = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;   // 명시적 흰색 — 어두운 배경에서 가시성 보장

        return btn;
    }

    // =============================================================
    //  Build Settings 헬퍼
    // =============================================================

    static void AddSceneToBuildSettings(string path)
    {
        var current = EditorBuildSettings.scenes;
        foreach (var s in current)
            if (s.path == path) return; // 이미 등록됨

        var updated = new EditorBuildSettingsScene[current.Length + 1];
        System.Array.Copy(current, updated, current.Length);
        updated[current.Length] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = updated;
    }

    // =============================================================
    //  진행 바 헬퍼
    // =============================================================

    static void Step(string msg, float progress)
    {
        EditorUtility.DisplayProgressBar("결과 화면 세팅", msg, progress);
    }
}
