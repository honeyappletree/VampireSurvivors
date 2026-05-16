using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MVP 무기 1 : Dark Bolt
/// - 플레이어 주위를 공전하며 닿는 적에게 주기적으로 피해를 줌
/// - 실제 충돌 판정은 OrbHitbox(동일 파일)가 담당
/// - 같은 적에게 fireInterval(0.5초)마다 한 번만 피해를 줌
///
/// 레벨업 수치
///   Lv.1 : 오브 1개, 데미지 20
///   Lv.2 : 오브 2개, 데미지 30
///   Lv.3 : 오브 3개, 데미지 40
/// </summary>
public class DarkBolt : WeaponBase
{
    [Header("회전 오브 설정")]
    [SerializeField] private float orbitRadius       = 1.5f;  // 공전 반지름
    [SerializeField] private float rotateSpeed       = 90f;   // 공전 속도 (도/초)
    [SerializeField] private float orbColliderRadius = 0.3f;  // 오브 충돌 반지름

    [Header("레벨별 수치")]
    public static float[] damageLevels   = { 20f, 30f, 40f }; //레벨업 수치별 데미지
    [SerializeField] private int[]   orbCountLevels = { 1, 2, 3 }; //레벨업 수치별 오브 개수

    private float currentAngle;
    private List<OrbHitbox> orbObjects = new();

    // 적별 히트 쿨다운 (같은 적에게 중복 피해 방지)
    private Dictionary<Enemy, float> hitCooldowns = new();

    public override string WeaponName => "Dark Bolt";

    // ── 라이프사이클 ──────────────────────────────────────────

    void Awake()
    {
        baseDamage   = damageLevels[0];
        fireInterval = 0.5f;
    }

    void Start() => SpawnOrbs(orbCountLevels[0]);

    // WeaponBase.Update 대신 직접 처리 (타이머 기반 Fire() 미사용)
    protected override void Update()
    {
        if (!IsPlaying) return;
        RotateOrbs();
        TickHitCooldowns();
    }

    // ── WeaponBase 구현 ───────────────────────────────────────

    // 피해는 OrbHitbox.OnTriggerEnter2D + hitCooldown으로 처리하므로 비워 둠
    protected override void Fire() { }

    protected override void OnLevelUp()
    {
        int idx    = Mathf.Clamp(weaponLevel - 1, 0, damageLevels.Length - 1);
        baseDamage = damageLevels[idx];
        Debug.Log(baseDamage);
        SpawnOrbs(orbCountLevels[idx]);
    }

    // ── 내부 로직 ─────────────────────────────────────────────

    void RotateOrbs()
    {
        currentAngle += rotateSpeed * Time.deltaTime;
        int count = orbObjects.Count;

        for (int i = 0; i < count; i++)
        {
            if (orbObjects[i] == null) continue;
            float offset = i * (360f / count);
            float rad    = (currentAngle + offset) * Mathf.Deg2Rad;
            orbObjects[i].transform.localPosition = new Vector2(
                Mathf.Cos(rad) * orbitRadius,
                Mathf.Sin(rad) * orbitRadius);
        }
    }

    void TickHitCooldowns()
    {
        var keys = new List<Enemy>(hitCooldowns.Keys);
        foreach (Enemy e in keys)
        {
            hitCooldowns[e] -= Time.deltaTime;
            if (hitCooldowns[e] <= 0f) hitCooldowns.Remove(e);
        }
    }

    /// <summary>OrbHitbox가 적과 충돌했을 때 호출</summary>
    public void OnOrbHit(Enemy enemy)
    {
        if (hitCooldowns.ContainsKey(enemy)) return;
        enemy.TakeDamage(baseDamage);
        hitCooldowns[enemy] = fireInterval;
    }

    void SpawnOrbs(int count)
    {
        // 기존 오브 전부 제거
        foreach (OrbHitbox o in orbObjects)
            if (o != null) Destroy(o.gameObject);
        orbObjects.Clear();

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Orb_{i}");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector2.right * orbitRadius;

            var hitbox = go.AddComponent<OrbHitbox>();
            hitbox.Init(this, orbColliderRadius);
            orbObjects.Add(hitbox);
        }
    }
}

// ─────────────────────────────────────────────────────────────

/// <summary>
/// 회전 오브 하나의 충돌 판정을 담당하는 컴포넌트.
/// RotatingOrb.SpawnOrbs()에서 코드로 생성되며 Inspector 노출 불필요.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class OrbHitbox : MonoBehaviour
{
    private DarkBolt owner;

    public void Init(DarkBolt owner, float radius)
    {
        this.owner = owner;

        var col       = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = radius;

        // 트리거 이벤트 수신을 위한 Kinematic Rigidbody2D
        var rb         = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType    = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();
        if (enemy != null) owner?.OnOrbHit(enemy);
    }
}
