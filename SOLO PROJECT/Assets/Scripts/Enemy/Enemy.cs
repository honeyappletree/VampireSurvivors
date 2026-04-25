using System.Collections;
using UnityEngine;

/// <summary>
/// 언데드 적 AI
/// - 플레이어를 추적하여 접촉 데미지를 줌
/// - 처치 시 플레이어 생명유지시간 회복 (초과분은 ExpOverflowConverter가 경험치 변환)
///
/// GDD v2: HP와 생명유지시간이 통합 운용됨.
/// 몬스터의 내구력도 maxLifeTime/currentLifeTime 단일 수치로 관리한다.
///
/// MonsterDataSO가 연결된 경우 해당 수치를 우선 적용한다.
/// 연결되지 않은 경우 Inspector 직접 입력값을 그대로 사용한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Enemy : MonoBehaviour
{
    [Header("몬스터 데이터 SO (연결 시 아래 수치 자동 덮어씀)")]
    [SerializeField] private MonsterDataSO monsterData;

    [Header("전투 스탯 (GDD v2: lifeTime = 내구력)")]
    public float maxLifeTime          = 50f;
    public float moveSpeed            = 2f;
    public float contactDamagePerSec  = 20f;

    [Header("처치 보상 (초과분은 ExpOverflowConverter → 경험치 자동 변환)")]
    public float timeReward = 5f;

    [Header("드롭 설정")]
    public GameObject xpOrbPrefab;
    public GameObject blackManaPrefab;
    public float dropHealAmount = 3f;

    private float           currentLifeTime;
    private Rigidbody2D     rb;
    private Transform       player;
    private bool            initialized = false;
    private Animator        _anim;
    private SpriteRenderer  _sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
        rb.bodyType       = RigidbodyType2D.Dynamic;

        GetComponent<CircleCollider2D>().isTrigger = true;

        _anim = GetComponentInChildren<Animator>();
        _sr   = GetComponentInChildren<SpriteRenderer>();

        if (monsterData != null)
            ApplyMonsterData(monsterData);

        currentLifeTime = maxLifeTime;
    }

    void Start()
    {
        if (!initialized)
            currentLifeTime = maxLifeTime;

        if (PlayerStats.Instance != null)
            player = PlayerStats.Instance.transform;
    }

    /// <summary>스포너에서 웨이브 강도에 따라 스탯 초기화</summary>
    public void Init(int wave)
    {
        float waveMultiplier = 1f + wave * 0.15f;
        maxLifeTime     *= waveMultiplier;
        currentLifeTime  = maxLifeTime;
        moveSpeed       += wave * 0.1f;
        initialized      = true;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
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

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // 이동 방향에 따라 스프라이트 좌우 반전
        if (_sr != null)
        {
            if (dir.x < 0f)       _sr.flipX = true;
            else if (dir.x > 0f)  _sr.flipX = false;
        }
    }

    /// <summary>발사체에 의한 피해 처리 (몬스터 생명유지시간 감소)</summary>
    public void TakeDamage(float damage)
    {
        currentLifeTime -= damage;
        if (currentLifeTime <= 0f)
            Die();
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            PlayerStats.Instance?.TakeDamage(contactDamagePerSec * Time.fixedDeltaTime);
    }

    void Die()
    {
        PlayerStats.Instance?.AddKill();
        PlayerStats.Instance?.AddTimeReward(timeReward);

        if (xpOrbPrefab != null)
            Object.Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);

        if (blackManaPrefab != null)
        {
            var orb = Object.Instantiate(blackManaPrefab, transform.position, Quaternion.identity);
            orb.GetComponent<BlackManaOrb>()?.Init(dropHealAmount);
        }

        if (_anim != null)
            _anim.SetBool("isDead", true);

        StartCoroutine(DestroyAfterDeath(1.5f));
    }

    IEnumerator DestroyAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void ApplyMonsterData(MonsterDataSO data)
    {
        maxLifeTime         = data.MaxHP;
        moveSpeed           = data.MoveSpeed;
        contactDamagePerSec = data.AttackDamage;
        timeReward          = data.TimeReward;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>()?.radius ?? 0.5f);
    }
#endif
}
