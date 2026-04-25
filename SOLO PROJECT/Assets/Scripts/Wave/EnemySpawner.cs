using UnityEngine;

/// <summary>
/// 웨이브 기반 적 스폰 시스템
/// - 일정 시간마다 대규모 웨이브 발생
/// - 웨이브 사이에도 지속적으로 소규모 스폰
/// - 웨이브 진행에 따라 스폰량과 적 스탯 증가
/// - 게임 시작 10분(bossSpawnTime) 후 보스 소환 및 일반 스폰 중단
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("적 프리팹")]
    public GameObject enemyPrefab;

    [Header("보스 설정")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private float bossSpawnTime = 600f; // 10분 후 보스 소환

    [Header("웨이브 설정")]
    public float waveInterval = 20f;      // 웨이브 발생 간격 (초)
    public int baseWaveCount = 8;         // 첫 웨이브 스폰 수
    public int waveCountIncrement = 4;    // 웨이브당 추가 스폰 수

    [Header("지속 스폰 설정")]
    public float continuousInterval = 3f; // 지속 스폰 간격 (초)
    public int baseContinuousCount = 2;   // 기본 지속 스폰 수

    [Header("스폰 범위")]
    public float spawnRadius = 13f;       // 플레이어 기준 스폰 반경
    public float spawnRadiusVariance = 2f;

    private int currentWave = 0;
    private float waveTimer;
    private float continuousTimer;
    private float gameTimer;
    private bool bossSpawned = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefab이 설정되지 않았습니다!");
            return;
        }

        // 게임 시작 시 첫 웨이브 즉시 스폰
        SpawnWave();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        gameTimer += Time.deltaTime;

        // 보스 소환 조건: 10분 경과 & 아직 미소환
        if (!bossSpawned && gameTimer >= bossSpawnTime)
        {
            SpawnBoss();
            return; // 보스 소환 후 일반 스폰 중단 (enabled = false 처리됨)
        }

        if (enemyPrefab == null) return;

        // 웨이브 타이머
        waveTimer += Time.deltaTime;
        if (waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            SpawnWave();
        }

        // 지속 스폰 타이머
        continuousTimer += Time.deltaTime;
        if (continuousTimer >= continuousInterval)
        {
            continuousTimer = 0f;
            int count = baseContinuousCount + currentWave;
            SpawnEnemies(count, currentWave);
        }
    }

    void SpawnBoss()
    {
        bossSpawned = true;

        if (bossPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] bossPrefab이 설정되지 않았습니다!");
            return;
        }

        if (PlayerStats.Instance == null) return;
        Vector2 playerPos = PlayerStats.Instance.transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 spawnPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;

        Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        UIManager.Instance?.ShowWaveNotification(0); // 0 = 보스 등장 알림 (ShowWaveNotification 확장 전까지 임시)
        Debug.Log("[EnemySpawner] 불멸의 사령관 소환 — 일반 스폰 중단");

        // 일반 웨이브 스폰 중단
        this.enabled = false;
    }

    void SpawnWave()
    {
        currentWave++;
        int count = baseWaveCount + (currentWave - 1) * waveCountIncrement;
        SpawnEnemies(count, currentWave);

        UIManager.Instance?.ShowWaveNotification(currentWave);
        Debug.Log($"[웨이브 {currentWave}] 적 {count}마리 스폰");
    }

    void SpawnEnemies(int count, int wave)
    {
        if (PlayerStats.Instance == null) return;
        Vector2 playerPos = PlayerStats.Instance.transform.position;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = spawnRadius + Random.Range(0f, spawnRadiusVariance);
            Vector2 spawnPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            enemy?.Init(wave);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (PlayerStats.Instance == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(PlayerStats.Instance.transform.position, spawnRadius);
    }
#endif
}
