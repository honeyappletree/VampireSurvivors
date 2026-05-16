using UnityEngine;

/// <summary>
/// MVP 무기 3 : Collapse Edge
/// - 발사 시점의 가장 가까운 적을 타겟으로 설정하고 추적
/// - 미사일 발사체(MissileProjectile)는 동일 파일에 정의
///
/// 레벨업 수치
///   Lv.1 : 데미지 30, 속도 6
///   Lv.2 : 데미지 42, 속도 7
///   Lv.3 : 데미지 80, 속도 8.5
/// </summary>
public class CollapseEdge : WeaponBase
{
    [Header("유도 미사일 설정")]
    [SerializeField] private float missileSpeed   = 6f;
    [SerializeField] private float turnSpeed      = 3f;   // 방향 전환 보간 속도
    [SerializeField] private float colliderRadius = 0.2f;

    [Header("레벨별 수치")]
    [SerializeField] private float[] damageLevels = { 20f, 30f, 40f }; //레벨업 수치별 데미지
    [SerializeField] private float[] speedLevels  = { 6f, 7f, 8.5f }; //레벨업 수치별 속도

    public override string WeaponName => "유도 미사일";

    // ── 라이프사이클 ──────────────────────────────────────────

    void Awake()
    {
        baseDamage   = damageLevels[0];
        fireInterval = 1.5f;
    }

    // ── WeaponBase 구현 ───────────────────────────────────────

    protected override void Fire()
    {
        Enemy nearest = FindNearestEnemy();
        if (nearest == null) return;

        var go = new GameObject("MissileProjectile");
        go.transform.position = transform.position;

        var missile = go.AddComponent<MissileProjectile>();
        missile.Init(nearest.transform, baseDamage, missileSpeed, turnSpeed, colliderRadius);
    }

    protected override void OnLevelUp()
    {
        int idx      = Mathf.Clamp(weaponLevel - 1, 0, damageLevels.Length - 1);
        baseDamage   = damageLevels[idx];
        missileSpeed = speedLevels[idx];
    }
}

// ─────────────────────────────────────────────────────────────

/// <summary>
/// 유도 미사일의 발사체 컴포넌트.
/// - FixedUpdate에서 타겟 방향으로 속도를 Lerp하여 부드럽게 추적
/// - 타겟이 소멸해도 마지막 속도 방향으로 계속 직진 후 Lifetime 경과 시 소멸
/// </summary>
public class MissileProjectile : MonoBehaviour
{
    private Transform   target;
    private float       damage;
    private float       speed;
    private float       turnSpeed;
    private Rigidbody2D rb;

    private const float Lifetime = 8f;

    public void Init(Transform target, float damage, float speed, float turnSpeed, float radius)
    {
        this.target    = target;
        this.damage    = damage;
        this.speed     = speed;
        this.turnSpeed = turnSpeed;

        rb              = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType     = RigidbodyType2D.Kinematic;

        // 초기 속도: 타겟 방향
        if (target != null)
            rb.linearVelocity =
                ((Vector2)target.position - (Vector2)transform.position).normalized * speed;

        var col       = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = radius;

        Destroy(gameObject, Lifetime);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 타겟이 살아있으면 방향 보정
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - rb.position).normalized;
            rb.linearVelocity =
                Vector2.Lerp(rb.linearVelocity, dir * speed, turnSpeed * Time.fixedDeltaTime);
        }

        // 이동 방향으로 스프라이트 회전
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();
        if (enemy == null) return;
        enemy.TakeDamage(damage);
        Destroy(gameObject);
    }
}
