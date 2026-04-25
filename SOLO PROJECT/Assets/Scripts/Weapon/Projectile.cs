using UnityEngine;

/// <summary>
/// 플레이어의 자동 공격 발사체
/// - 일직선으로 이동하며 적에 닿으면 데미지를 주고 소멸
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float damage;
    private const float Lifetime = 6f; // 발사 후 자동 소멸 시간 (초)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // 발사체는 Kinematic으로 설정 (물리 힘을 받지 않음)
        rb.bodyType = RigidbodyType2D.Kinematic;

        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    /// <summary>생성 직후 방향과 데미지를 초기화</summary>
    public void Init(Vector2 direction, float damage, float speed)
    {
        this.damage = damage;
        rb.linearVelocity = direction * speed;

        // 발사 방향으로 스프라이트 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, Lifetime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy enemy = col.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
