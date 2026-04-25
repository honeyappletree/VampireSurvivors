using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// HUD 및 게임 UI 전반을 관리하는 매니저
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── 생명 유지 시간 UI ──────────────────────────────────────
    [Header("생명 유지 시간")]
    public Slider lifeTimeSlider;
    public TextMeshProUGUI lifeTimeText;

    // ── 체력 UI ────────────────────────────────────────────────
    [Header("체력")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    // ── 레벨 / 경험치 UI ──────────────────────────────────────
    [Header("레벨 / 경험치")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;

    // ── 웨이브 알림 ────────────────────────────────────────────
    [Header("웨이브 알림")]
    public TextMeshProUGUI waveNotificationText;

    // ── 게임 오버 패널 ─────────────────────────────────────────
    [Header("게임 오버")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverLevelText;
    public Button restartButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 초기 상태 설정
        gameOverPanel?.SetActive(false);
        waveNotificationText?.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
    }

    void Update()
    {
        if (PlayerStats.Instance == null) return;
        if (GameManager.Instance?.CurrentState == GameManager.GameState.GameOver) return;

        UpdateLifeTimeUI();
        UpdateHPUI();
        UpdateXPUI();
    }

    // ── 생명 유지 시간 바 단계별 색상 ────────────────────────────
    // 120초 이상: 파란색 / 60~120초: 초록색 / 30~60초: 주황색 / 30초 미만: 빨간색
    private static readonly Color LifeColorBlue   = new Color(0x4A / 255f, 0x90 / 255f, 0xD9 / 255f); // #4A90D9
    private static readonly Color LifeColorGreen  = new Color(0x27 / 255f, 0xAE / 255f, 0x60 / 255f); // #27AE60
    private static readonly Color LifeColorOrange = new Color(0xE6 / 255f, 0x7E / 255f, 0x22 / 255f); // #E67E22
    private static readonly Color LifeColorRed    = new Color(0xC0 / 255f, 0x39 / 255f, 0x2B / 255f); // #C0392B

    /// <summary>
    /// 현재 생명유지시간에 따라 구간별 인접 색상을 Lerp하여 반환
    /// 구간 경계에서 부드럽게 전환됨
    /// </summary>
    Color GetLifeTimeColor(float lifeTime)
    {
        if (lifeTime >= 120f)
            return LifeColorBlue;
        if (lifeTime >= 60f)
            return Color.Lerp(LifeColorGreen, LifeColorBlue, (lifeTime - 60f) / 60f);
        if (lifeTime >= 30f)
            return Color.Lerp(LifeColorOrange, LifeColorGreen, (lifeTime - 30f) / 30f);
        return Color.Lerp(LifeColorRed, LifeColorOrange, lifeTime / 30f);
    }

    void UpdateLifeTimeUI()
    {
        float current = PlayerStats.Instance.currentLifeTime;

        if (lifeTimeSlider != null)
        {
            lifeTimeSlider.value = current / PlayerStats.Instance.maxLifeTime;

            Image fill = lifeTimeSlider.fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = GetLifeTimeColor(current);
        }

        if (lifeTimeText != null)
            lifeTimeText.text = $"생명유지: {Mathf.CeilToInt(current)}초";
    }

    void UpdateHPUI()
    {
        // GDD v2: HP는 생명유지시간과 통합 운용 → 동일한 값으로 표시
        float current = PlayerStats.Instance.currentLifeTime;
        float max     = PlayerStats.Instance.maxLifeTime;

        if (hpSlider != null)
            hpSlider.value = max > 0f ? current / max : 0f;

        if (hpText != null)
            hpText.text = $"생존: {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}초";
    }

    void UpdateXPUI()
    {
        if (levelText != null)
            levelText.text = $"레벨 {PlayerStats.Instance.level}";

        if (xpSlider != null)
            xpSlider.value = PlayerStats.Instance.currentXP / PlayerStats.Instance.xpToNextLevel;
    }

    /// <summary>웨이브 알림 텍스트를 잠시 표시</summary>
    public void ShowWaveNotification(int wave)
    {
        if (waveNotificationText == null) return;
        StopAllCoroutines();
        StartCoroutine(WaveNotificationRoutine(wave));
    }

    IEnumerator WaveNotificationRoutine(int wave)
    {
        waveNotificationText.text = wave == 0 ? "!! 불멸의 사령관 등장 !!" : $"-- 웨이브 {wave} --";
        waveNotificationText.gameObject.SetActive(true);

        // 서서히 페이드
        CanvasGroup cg = waveNotificationText.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            yield return new WaitForSeconds(1.5f);
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                cg.alpha = 1f - elapsed / 0.5f;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        waveNotificationText.gameObject.SetActive(false);
    }

    /// <summary>게임 오버 패널 표시</summary>
    public void ShowGameOver()
    {
        gameOverPanel?.SetActive(true);

        if (gameOverLevelText != null && PlayerStats.Instance != null)
            gameOverLevelText.text = $"Lv.{PlayerStats.Instance.level} 도달\n생존 시간이 끝났습니다.";
    }
}
