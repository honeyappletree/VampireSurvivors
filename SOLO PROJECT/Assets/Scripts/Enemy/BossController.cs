using System.Collections;
using UnityEngine;

/// <summary>
/// 불멸의 사령관 - 보스 AI
///
/// 패턴 1 — 돌진: 플레이어 방향 고속 직선 돌진 → 0.5초 정지
///   기본 주기 5초 / HP 50% 이하 시 2.5초
///
/// 패턴 2 — 죽음의 포효: 선딜레이 1.5초 → 반경 4 unit 원형 충격파 (데미지 40)
///   주기 8초 / 포효 중에는 돌진 차단 (isRoaring 플래그)
///
/// 처치 시: 생명유지시간 +30초 → 레벨업 카드 표시 → 3초 사망 연출 → ResultScreen
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BossController : MonoBehaviour
{
    [Header("몬스터 데이터 SO")]
    [SerializeField] private MonsterDataSO monsterData;

    [Header("기본 스탯 (SO 미연결 시 직접 입력)")]
    [SerializeField] private float maxHP              = 1000f;
    [SerializeField] private float moveSpeed          = 1.0f;
    [SerializeField] private float contactDamagePerSec = 25f;
    [SerializeField] private float timeReward         = 30f;

    [Header("패턴 1 — 돌진")]
    [SerializeField] private float dashSpeed          = 12f;    // 돌진 속도
    [SerializeField] private float dashDuration       = 0.4f;   // 돌진 지속 시간
    [SerializeField] private float dashCooldown       = 0.5f;   // 돌진 후 정지 시간
    [SerializeField] private float dashInterval       = 5f;     // 돌진 발동 주기 (Phase 1)
    [SerializeField] private float dashIntervalPhase2 = 2.5f;   // HP 50% 이하 주기

    [Header("패턴 2 — 포효")]
    [SerializeField] private float roarInterval       = 8f;     // 포효 발동 주기
    [SerializeField] private float roarWindup         = 1.5f;   // 선딜레이
    [SerializeField] private float roarRadius         = 4f;     // 충격파 반경
    [SerializeField] private float roarDamage         = 40f;    // 포효 데미지

    private float currentHP;
    private Rigidbody2D rb;
    private Transform player;
    private bool isRoaring  = false;
    private bool isDead     = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
        rb.bodyType       = RigidbodyType2D.Dynamic;

        GetComponent<CircleCollider2D>().isTrigger = true;

        if (monsterData != null)
            ApplyMonsterData(monsterData);

        currentHP = maxHP;
    }

    void Start()
    {
        if (PlayerStats.Instance != null)
            player = PlayerStats.Instance.transform;

        StartCoroutine(DashLoop());
        StartCoroutine(RoarLoop());
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        if (isRoaring)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null)
        {
            if (PlayerStats.Instance != null)
                player = PlayerStats.Instance.transform;
            return;
        }

        // 평상시 저속 추적
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    // ── 피해 처리 ────────────────────────────────────────────────

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHP -= damage;
        if (currentHP <= 0f)
            StartCoroutine(DeathRoutine());
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (isDead) return;
        if (col.CompareTag("Player"))
            PlayerStats.Instance?.TakeDamage(contactDamagePerSec * Time.fixedDeltaTime);
    }

    // ── 패턴 1: 돌진 루프 ────────────────────────────────────────

    IEnumerator DashLoop()
    {
        while (!isDead)
        {
            float interval = (currentHP / maxHP <= 0.5f) ? dashIntervalPhase2 : dashInterval;
            yield return new WaitForSeconds(interval);

            if (isDead) yield break;
            if (isRoaring) continue;
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) continue;
            if (player == null) continue;

            yield return StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        Vector2 dashDir = ((Vector2)player.position - rb.position).normalized;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown);
    }

    // ── 패턴 2: 포효 루프 ────────────────────────────────────────

    IEnumerator RoarLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(roarInterval);

            if (isDead) yield break;
            if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) continue;

            yield return StartCoroutine(RoarRoutine());
        }
    }

    IEnumerator RoarRoutine()
    {
        isRoaring = true;
        rb.linearVelocity = Vector2.zero;

        // 선딜레이
        yield return new WaitForSeconds(roarWindup);

        if (isDead) { isRoaring = false; yield break; }

        // 원형 충격파
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, roarRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
                PlayerStats.Instance?.TakeDamage(roarDamage);
        }

        Debug.Log($"[BossController] 포효! 반경 {roarRadius}m 충격파 데미지 {roarDamage}");

        isRoaring = false;
    }

    // ── 사망 연출 ─────────────────────────────────────────────────

    IEnumerator DeathRoutine()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        // 처치 보상: 생명유지시간 회복
        PlayerStats.Instance?.AddKill();
        PlayerStats.Instance?.AddTimeReward(timeReward);

        // 레벨업 카드 즉시 표시
        GameManager.Instance?.TriggerLevelUp();

        // 사망 연출 3초 (timeScale 무관)
        yield return new WaitForSecondsRealtime(3f);

        // ResultScreen으로 전환
        GameManager.Instance?.TriggerGameOver();

        Destroy(gameObject);
    }

    // ── 내부 유틸 ─────────────────────────────────────────────────

    void ApplyMonsterData(MonsterDataSO data)
    {
        maxHP               = data.MaxHP;
        moveSpeed           = data.MoveSpeed;
        contactDamagePerSec = data.AttackDamage;
        timeReward          = data.TimeReward;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 포효 범위
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, roarRadius);
    }
#endif
}
