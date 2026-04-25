using UnityEngine;

/// <summary>
/// 가장 가까운 적을 자동으로 탐지하여 발사체를 발사하는 자동 공격 시스템
/// </summary>
public class AutoAttack : MonoBehaviour
{
    [Header("프리팹 참조")]
    public GameObject projectilePrefab;

    [Header("발사 위치 (없으면 CharacterModel 중심)")]
    public Transform firePoint;

    Transform _firePoint;
    private float fireTimer;

    void Awake()
    {
        var model = transform.Find("CharacterModel");
        _firePoint = model != null ? model : transform;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
        if (PlayerStats.Instance == null || projectilePrefab == null)
            return;

        float fireInterval = 1f / PlayerStats.Instance.attackFireRate;
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            TryFire();
        }
    }

    void TryFire()
    {
        Enemy nearest = FindNearestEnemy();
        if (nearest == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : _firePoint.position;
        Vector2 dir = ((Vector3)nearest.transform.position - spawnPos).normalized;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Init(dir, PlayerStats.Instance.attackDamage, PlayerStats.Instance.projectileSpeed);
    }

    /// <summary>씬 내 모든 적 중 가장 가까운 적을 반환</summary>
    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy nearest = null;
        float minDist = float.MaxValue;

        foreach (Enemy e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }
}
