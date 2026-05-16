using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MVP 무기 2 : Poison Cloud
/// - 가장 가까운 적 방향으로 직선 발사, 최대 N적을 관통
/// - 기존 AutoAttack + Projectile 구조와 동일한 패턴으로 구현
/// - 발사체(PiercingProjectile)는 동일 파일에 정의
///
/// 레벨업 수치
///   Lv.1 : 데미지 8, 관통 3적
///   Lv.2 : 데미지 12, 관통 4적
///   Lv.3 : 데미지 18, 관통 5적
/// </summary>
public class PoisonCloud : WeaponBase
{
    [Header("관통 볼트 설정")]
    [SerializeField] private float projectileSpeed  = 10f;
    [SerializeField] private int   maxPierceCount   = 3;
    [SerializeField] private float colliderRadius   = 0.2f;

    [Header("레벨별 수치")]
    [SerializeField] private float[] damageLevels = { 8f, 12f, 18f }; //레벨업 수치별 데미지
    [SerializeField] private int[]   pierceLevels = { 3, 4, 5 }; //레벨업 수치별 관통 수

    public override string WeaponName => "Poison Cloud";

    // ── 라이프사이클 ──────────────────────────────────────────

    void Awake()
    {
        baseDamage   = damageLevels[0];
        fireInterval = 1.0f;
    }

    // ── WeaponBase 구현 ───────────────────────────────────────

    protected override void Fire()
    {
        Enemy nearest = FindNearestEnemy();
        if (nearest == null) return;

        Vector2 dir = ((Vector2)nearest.transform.position - (Vector2)transform.position).normalized;

        var go = new GameObject("PiercingProjectile");
        go.transform.position = transform.position;

        var proj = go.AddComponent<PiercingProjectile>();
        proj.Init(dir, baseDamage, projectileSpeed, maxPierceCount, colliderRadius);
    }

    protected override void OnLevelUp()
    {
        int idx        = Mathf.Clamp(weaponLevel - 1, 0, damageLevels.Length - 1);
        baseDamage     = damageLevels[idx];
        maxPierceCount = pierceLevels[idx];
    }
}

// ─────────────────────────────────────────────────────────────

/// <summary>
/// 관통 볼트의 발사체 컴포넌트.
/// - 일직선으로 이동하며 최대 remainPierceCount만큼 적을 관통
/// - 같은 적에게 중복 피해를 방지하기 위해 hitEnemies 집합으로 관리
/// </summary>
public class PiercingProjectile : MonoBehaviour
{
    private float       damage;
    private int         remainPierceCount;
    private Rigidbody2D rb;
    private HashSet<Enemy> hitEnemies = new();

    private const float Lifetime = 6f;

    public void Init(Vector2 dir, float damage, float speed, int pierceCount, float radius)
    {
        this.damage            = damage;
        this.remainPierceCount = pierceCount;

        rb              = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.linearVelocity = dir * speed;

        var col       = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = radius;

        // 발사 방향으로 스프라이트 회전 (Projectile.cs 와 동일 패턴)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, Lifetime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();
        if (enemy == null || hitEnemies.Contains(enemy)) return;

        hitEnemies.Add(enemy);
        enemy.TakeDamage(damage);

        remainPierceCount--;
        if (remainPierceCount <= 0)
            Destroy(gameObject);
    }
}
