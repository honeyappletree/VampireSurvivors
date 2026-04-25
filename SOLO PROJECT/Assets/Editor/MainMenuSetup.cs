// ============================================================
//  메인 메뉴 씬 자동 세팅 도구
//  메뉴: 뱀파이어 서바이벌 ▶ 메인 메뉴 세팅
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class MainMenuSetup
{
    // ── 경로 상수 ─────────────────────────────────────────────
    const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

    // ── 색상 상수 ─────────────────────────────────────────────
    static readonly Color ColBg         = new Color(0.102f, 0.102f, 0.102f, 1f);    // #1A1A1A 어두운 배경
    static readonly Color ColPanel      = new Color(0.145f, 0.145f, 0.145f, 0.97f); // #252525 팝업 패널
    static readonly Color ColBtn        = new Color(0.180f, 0.180f, 0.180f, 1f);    // #2E2E2E 버튼 배경
    static readonly Color ColBtnHover   = new Color(0.243f, 0.243f, 0.243f, 1f);    // #3E3E3E 호버
    static readonly Color ColBtnPress   = new Color(0.118f, 0.118f, 0.118f, 1f);    // #1E1E1E 클릭
    static readonly Color ColTitle      = new Color(0.753f, 0.224f, 0.169f, 1f);    // #C0392B 붉은 타이틀
    static readonly Color ColSliderFill = new Color(0.753f, 0.224f, 0.169f, 1f);    // #C0392B 슬라이더 필
    static readonly Color ColOverlay    = new Color(0f,     0f,     0f,     0.65f); // 설정 오버레이

    // =============================================================
    //  메인 진입점
    // =============================================================
    [MenuItem("뱀파이어 서바이벌/메인 메뉴 세팅", false, 20)]
    public static void CreateMainMenuScene()
    {
        bool ok = EditorUtility.DisplayDialog(
            "메인 메뉴 세팅",
            "Assets/Scenes/MainMenu.unity 씬을 자동으로 생성합니다.\n\n" +
            "• Canvas + 타이틀 텍스트\n" +
            "• 게임 시작 / 설정 / 종료 버튼\n" +
            "• BGM·SFX 볼륨 슬라이더 설정 패널\n" +
            "• MainMenuController 컴포넌트 자동 연결\n\n" +
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

            CreateTitleText(tf);
            var mainPanel = CreateMainPanel(tf);

            Button startBtn    = CreateStyledButton(mainPanel.transform, "StartButton",    "게임 시작", new Vector2(0,  90), new Vector2(280, 60));
            Button settingsBtn = CreateStyledButton(mainPanel.transform, "SettingsButton", "설정",     new Vector2(0,  10), new Vector2(280, 60));
            Button quitBtn     = CreateStyledButton(mainPanel.transform, "QuitButton",     "종료",     new Vector2(0, -70), new Vector2(280, 60));

            Step("설정 패널 생성 중...", 0.65f);
            var settingsPanel = CreateSettingsPanel(tf,
                out Slider bgmSlider, out Slider sfxSlider, out Button closeBtn);
            settingsPanel.SetActive(false);

            Step("MainMenuController 연결 중...", 0.80f);
            var controllerGo = new GameObject("MainMenuController");
            var controller   = controllerGo.AddComponent<MainMenuController>();

            // SerializedObject 로 private [SerializeField] 필드 연결
            var so = new SerializedObject(controller);
            so.FindProperty("startButton")          .objectReferenceValue = startBtn;
            so.FindProperty("settingsButton")       .objectReferenceValue = settingsBtn;
            so.FindProperty("quitButton")           .objectReferenceValue = quitBtn;
            so.FindProperty("settingsPanel")        .objectReferenceValue = settingsPanel;
            so.FindProperty("bgmSlider")            .objectReferenceValue = bgmSlider;
            so.FindProperty("sfxSlider")            .objectReferenceValue = sfxSlider;
            so.FindProperty("closeSettingsButton")  .objectReferenceValue = closeBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            Step("씬 저장 중...", 0.90f);
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(SCENE_PATH);

            EditorUtility.DisplayDialog("완료!",
                $"MainMenu.unity 씬이 생성되었습니다.\n경로: {SCENE_PATH}\n\n" +
                "Build Settings에도 자동 추가되었습니다.", "확인");

            Debug.Log($"[메인 메뉴 세팅] ✓ 완료 — {SCENE_PATH}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[메인 메뉴 세팅 오류] {e}");
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
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
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

    static void CreateTitleText(Transform parent)
    {
        var go = new GameObject("TitleText");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 1f);
        rt.anchorMax        = new Vector2(0.5f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -90);
        rt.sizeDelta        = new Vector2(900, 130);

        var tmp = ObjectFactory.AddComponent<TextMeshProUGUI>(go);
        tmp.text      = "뱀파이어 서바이벌";
        tmp.fontSize  = 76;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = ColTitle;
        tmp.fontStyle = FontStyles.Bold;
    }

    /// <summary>버튼 3개를 담는 세로 컨테이너</summary>
    static GameObject CreateMainPanel(Transform parent)
    {
        var go = new GameObject("MainPanel");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -30);
        rt.sizeDelta        = new Vector2(300, 240);

        return go;
    }

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

        var img = go.AddComponent<Image>();
        img.color = ColBtn;

        var btn = go.AddComponent<Button>();
        // targetGraphic을 Image로 명시 → ColorTint가 텍스트에 영향을 주지 않음
        btn.targetGraphic = img;
        var cb  = btn.colors;
        cb.normalColor      = ColBtn;
        cb.highlightedColor = ColBtnHover;
        cb.pressedColor     = ColBtnPress;
        cb.selectedColor    = ColBtn;
        cb.fadeDuration     = 0.08f;
        btn.colors          = cb;

        // ─ 텍스트 ─
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        var textRT        = textGo.AddComponent<RectTransform>();
        textRT.anchorMin  = Vector2.zero;
        textRT.anchorMax  = Vector2.one;
        textRT.offsetMin  = Vector2.zero;
        textRT.offsetMax  = Vector2.zero;

        var tmp       = ObjectFactory.AddComponent<TextMeshProUGUI>(textGo);
        tmp.text      = label;
        tmp.fontSize  = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;  // 명시적 흰색 — 어두운 배경에서 가시성 보장

        return btn;
    }

    // =============================================================
    //  설정 패널
    // =============================================================

    static GameObject CreateSettingsPanel(Transform parent,
        out Slider bgmSlider, out Slider sfxSlider, out Button closeBtn)
    {
        // ─ 어두운 반투명 오버레이 (전체 화면) ─
        var overlay = new GameObject("SettingsPanel");
        overlay.transform.SetParent(parent, false);

        var overlayRT       = overlay.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        var overlayImg  = overlay.AddComponent<Image>();
        overlayImg.color = ColOverlay;

        // 클릭 차단용 (오버레이 자체가 Raycast Target)
        overlayImg.raycastTarget = true;

        // ─ 팝업 박스 ─
        var popup = new GameObject("Popup");
        popup.transform.SetParent(overlay.transform, false);

        var popupRT         = popup.AddComponent<RectTransform>();
        popupRT.anchorMin   = new Vector2(0.5f, 0.5f);
        popupRT.anchorMax   = new Vector2(0.5f, 0.5f);
        popupRT.anchoredPosition = Vector2.zero;
        popupRT.sizeDelta   = new Vector2(500, 380);

        var popupImg  = popup.AddComponent<Image>();
        popupImg.color = ColPanel;

        // ─ 설정 타이틀 ─
        AddLabel(popup.transform, "SettingsTitle", "설정",
            new Vector2(0, 145), new Vector2(400, 50), 34, Color.white, FontStyles.Bold);

        // ─ BGM 라벨 + 슬라이더 ─
        AddLabel(popup.transform, "BGMLabel", "BGM 볼륨",
            new Vector2(-90, 55), new Vector2(140, 36), 22, Color.white);
        bgmSlider = AddSlider(popup.transform, "BGMSlider",
            new Vector2(70, 55), new Vector2(220, 28));

        // ─ SFX 라벨 + 슬라이더 ─
        AddLabel(popup.transform, "SFXLabel", "SFX 볼륨",
            new Vector2(-90, 0), new Vector2(140, 36), 22, Color.white);
        sfxSlider = AddSlider(popup.transform, "SFXSlider",
            new Vector2(70, 0), new Vector2(220, 28));

        // ─ 닫기 버튼 ─
        closeBtn = CreateStyledButton(popup.transform, "CloseButton", "닫기",
            new Vector2(0, -120), new Vector2(160, 50));

        return overlay;
    }

    static void AddLabel(Transform parent, string name, string text,
        Vector2 pos, Vector2 size, float fontSize, Color color,
        FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var tmp       = ObjectFactory.AddComponent<TextMeshProUGUI>(go);
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
        tmp.fontStyle = style;
    }

    // =============================================================
    //  슬라이더 빌더
    //  Unity UGUI Slider 의 필수 계층(Background / FillArea / HandleSlideArea)
    //  을 수동으로 구성하고 Slider.fillRect / handleRect 를 연결합니다.
    // =============================================================

    static Slider AddSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        // ─ 루트 ─
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;

        var slider      = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        // ─ Background ─
        var bgGo  = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bgRT       = bgGo.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f);
        bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg      = bgGo.AddComponent<Image>();
        bgImg.color    = new Color(0.28f, 0.28f, 0.28f, 1f);

        // ─ Fill Area ─
        var fillAreaGo  = new GameObject("Fill Area");
        fillAreaGo.transform.SetParent(go.transform, false);
        var fillAreaRT       = fillAreaGo.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5f,  0f);
        fillAreaRT.offsetMax = new Vector2(-15f, 0f);

        var fillGo  = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRT       = fillGo.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImg      = fillGo.AddComponent<Image>();
        fillImg.color    = ColSliderFill;

        // ─ Handle Slide Area ─
        var hsaGo  = new GameObject("Handle Slide Area");
        hsaGo.transform.SetParent(go.transform, false);
        var hsaRT       = hsaGo.AddComponent<RectTransform>();
        hsaRT.anchorMin = Vector2.zero;
        hsaRT.anchorMax = Vector2.one;
        hsaRT.offsetMin = new Vector2(10f, 0f);
        hsaRT.offsetMax = new Vector2(-10f, 0f);

        var handleGo  = new GameObject("Handle");
        handleGo.transform.SetParent(hsaGo.transform, false);
        var handleRT       = handleGo.AddComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0.5f, 0f);
        handleRT.anchorMax = new Vector2(0.5f, 1f);
        handleRT.sizeDelta = new Vector2(20f, 0f);
        var handleImg      = handleGo.AddComponent<Image>();
        handleImg.color    = Color.white;

        // ─ Slider 연결 ─
        slider.fillRect     = fillRT;
        slider.handleRect   = handleRT;
        slider.targetGraphic = handleImg;

        return slider;
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
        EditorUtility.DisplayProgressBar("메인 메뉴 세팅", msg, progress);
    }

}
