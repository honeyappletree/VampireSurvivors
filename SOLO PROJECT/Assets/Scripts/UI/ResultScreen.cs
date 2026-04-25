using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 종료 후 결과를 표시하는 UI 스크린
/// GameManager.LastResult에서 결과 데이터를 읽어 표시한다.
/// </summary>
public class ResultScreen : MonoBehaviour
{
    // ── 결과 수치 텍스트 ───────────────────────────────────────
    [Header("결과 수치 텍스트")]
    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI maxLevelText;
    [SerializeField] private TextMeshProUGUI currencyText;

    // ── 버튼 ──────────────────────────────────────────────────
    [Header("버튼")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    // ── 씬 이름 ───────────────────────────────────────────────
    [Header("씬 이름")]
    [SerializeField] private string gameSceneName     = "Game";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Start()
    {
        GameManager.GameResultData result = GameManager.LastResult;

        if (survivalTimeText != null)
            survivalTimeText.text = $"생존 시간  {FormatTime(result.survivalTime)}";

        if (killCountText != null)
            killCountText.text = $"처치 수  {result.killCount}";

        if (maxLevelText != null)
            maxLevelText.text = $"최고 레벨  Lv.{result.maxLevel}";

        if (currencyText != null)
            currencyText.text = $"획득 재화  {result.currency}";

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    /// <summary>초 단위 시간을 "X분 XX초" 또는 "XX초" 형식으로 변환</summary>
    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return m > 0 ? $"{m}분 {s:00}초" : $"{s}초";
    }

    private void OnRestart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
