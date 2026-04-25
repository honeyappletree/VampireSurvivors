using UnityEngine;

/// <summary>
/// 모든 무기의 공통 추상 기반 클래스.
/// 발동 주기(fireInterval), 데미지(baseDamage), 레벨(1~3)을 공통 관리하며
/// 구체적인 공격 로직과 레벨업 강화는 각 무기 클래스에서 구현한다.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("무기 기본 수치")]
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float fireInterval;          // 발동 주기 (초)
    [SerializeField, Range(1, 3)] protected int weaponLevel = 1;

    protected const int MaxLevel = 3;
    protected float fireTimer;

    // ── 프로퍼티 ─────────────────────────────────────────────

    public int  WeaponLevel => weaponLevel;
    public bool IsMaxLevel  => weaponLevel >= MaxLevel;

    /// <summary>Inspector·SlotManager에서 표시할 무기 이름</summary>
    public abstract string WeaponName { get; }

    // ── 라이프사이클 ──────────────────────────────────────────

    protected virtual void Update()
    {
        if (!IsPlaying) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    // ── 추상 메서드 ───────────────────────────────────────────

    /// <summary>발동 주기마다 호출되는 공격 로직</summary>
    protected abstract void Fire();

    /// <summary>레벨업 시 수치 강화 로직 (weaponLevel 증가 후 호출됨)</summary>
    protected abstract void OnLevelUp();

    // ── 공개 메서드 ───────────────────────────────────────────

    /// <summary>
    /// 레벨 1 증가 및 수치 강화. 이미 최고 레벨이면 무시.
    /// WeaponSlotManager에서 호출한다.
    /// </summary>
    public void LevelUp()
    {
        if (IsMaxLevel)
        {
            Debug.Log($"[{WeaponName}] 이미 최고 레벨(Lv.{MaxLevel})입니다.");
            return;
        }
        weaponLevel++;
        OnLevelUp();
        Debug.Log($"[{WeaponName}] Lv.{weaponLevel} 달성");
    }

    // ── 유틸리티 ─────────────────────────────────────────────

    protected bool IsPlaying =>
        GameManager.Instance != null &&
        GameManager.Instance.CurrentState == GameManager.GameState.Playing;

    /// <summary>씬 내 가장 가까운 Enemy 반환 (없으면 null)</summary>
    protected Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy  nearest  = null;
        float  minDist  = float.MaxValue;

        foreach (Enemy e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist) { minDist = d; nearest = e; }
        }
        return nearest;
    }
}
