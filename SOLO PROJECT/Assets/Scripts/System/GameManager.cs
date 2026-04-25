using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태를 관리하는 싱글톤 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, LevelUp, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    // ── 씬 이름 설정 ──────────────────────────────────────────
    [Header("씬 이름")]
    [SerializeField] private string resultSceneName   = "ResultScreen";
    [SerializeField] private string gameSceneName     = "Game";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // ── 결과 데이터 ────────────────────────────────────────────
    /// <summary>씬 전환 후 ResultScreen이 읽어가는 게임 결과 데이터</summary>
    public struct GameResultData
    {
        public float survivalTime; // 실제 생존 경과 시간(초)
        public int   killCount;    // 처치 수
        public int   maxLevel;     // 달성 최고 레벨
        public int   currency;     // 획득 재화 (생존 시간(초) 수치 그대로)
    }
    public static GameResultData LastResult { get; private set; }

    private float survivalTimer; // Playing 상태일 때만 누적

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (CurrentState == GameState.Playing)
            survivalTimer += Time.deltaTime;
    }

    /// <summary>레벨업 화면으로 전환 (시간 정지)</summary>
    public void TriggerLevelUp()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.LevelUp;
        Time.timeScale = 0f;
        LevelUpPanel.Instance?.Show();
    }

    /// <summary>레벨업 선택 후 게임 재개</summary>
    public void ResumePlaying()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    /// <summary>생명 유지 시간 소진 시 결과 데이터를 저장하고 결과 씬으로 전환</summary>
    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;

        // 결과 데이터 수집
        PlayerStats ps = PlayerStats.Instance;
        LastResult = new GameResultData
        {
            survivalTime = survivalTimer,
            killCount    = ps != null ? ps.killCount : 0,
            maxLevel     = ps != null ? ps.level     : 1,
            currency     = Mathf.FloorToInt(survivalTimer)
        };

        Time.timeScale = 1f;
        SceneManager.LoadScene(resultSceneName);
    }

    /// <summary>게임 씬 재시작</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>메인 메뉴 씬 로드</summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
