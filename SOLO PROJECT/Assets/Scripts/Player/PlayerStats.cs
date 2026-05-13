using UnityEngine;
using System;

/// <summary>
/// 플레이어의 모든 스탯과 죽음 유예 시스템, 경험치/레벨 시스템을 관리
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    // ── 죽음 유예 시스템 ──────────────────────────────────────
    [Header("생명 유지 시간 (죽음 유예)")]
    public float maxLifeTime = 60f;             // 최대 생명유지시간 (초)
    public float maxLifeTimeCap = 90f;          // 생명유지시간 절대 상한선 (초)
    public float lifeTimeDecreaseRate = 0.333f;  // 3초당 1 감소
    public float timeRewardPerKill = 5f;        // 언데드 처치 시 회복량
    [HideInInspector] public float currentLifeTime;

    // ── 피격 무적 ─────────────────────────────────────────────
    [Header("피격 무적")]
    public float invincibleDuration = 0.5f;     // 피격 후 무적 시간
    private float invincibleTimer;

    // ── 레벨 / 경험치 ─────────────────────────────────────────
    [Header("레벨 / 경험치")]
    public float xpToNextLevel = 50f;
    public float xpScaleFactor = 1.4f;          // 레벨당 필요 경험치 배수
    public static int level = 1;
    [HideInInspector] public float currentXP;

    // ── 이동 / 공격 스탯 ──────────────────────────────────────
    [Header("이동 스탯")]
    public float moveSpeed = 5f;

    [Header("공격 스탯")]
    public float attackDamage = 20f;
    public float attackFireRate = 1f;           // 초당 발사 횟수
    public float projectileSpeed = 8f;

    // ── 처치 수 ───────────────────────────────────────────────
    public static int killCount;

    // ── 이벤트 ────────────────────────────────────────────────
    public event Action<int> OnLevelUp;

    public float CurrentLifeTime => currentLifeTime;
    public float MaxLifeTime     => maxLifeTime;

    HitFlashEffect _hitFlash;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentLifeTime = maxLifeTime;
        _hitFlash = GetComponent<HitFlashEffect>();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // 생명 유지 시간 지속 감소
        currentLifeTime -= lifeTimeDecreaseRate * Time.deltaTime;
        Debug.Log("player의 currentTime: " + currentLifeTime);
        if (currentLifeTime <= 0f)
        {
            currentLifeTime = 0f;
            GameManager.Instance.TriggerGameOver();
        }

        // 무적 타이머
        if (invincibleTimer > 0f)
            invincibleTimer -= Time.deltaTime;
    }

    // ── 공개 메서드 ───────────────────────────────────────────

    /// <summary>적 처치 카운트 증가</summary>
    public void AddKill() => killCount++;

    /// <summary>
    /// 언데드 처치 시 생명유지시간 회복 (maxLifeTimeCap 초과 불가).
    /// 초과분이 발생하면 ExpOverflowConverter를 통해 경험치로 변환된다.
    /// </summary>
    public void AddTimeReward(float amount)
    {
        float overflow = (currentLifeTime + amount) - maxLifeTimeCap;
        currentLifeTime = Mathf.Min(currentLifeTime + amount, maxLifeTimeCap);

        if (overflow > 0f)
            ExpOverflowConverter.Instance?.Convert(overflow);
    }

    /// <summary>피격 시 생명유지시간 감소 (무적 시간 포함, HP 통합 운용)</summary>
    public void TakeDamage(float damage)
    {
        if (invincibleTimer > 0f) return;
        currentLifeTime -= damage;
        Debug.Log("플레이어가 입은 데미지: " + damage);
        invincibleTimer = invincibleDuration;
        if (_hitFlash != null) _hitFlash.TriggerFlash();

        if (currentLifeTime <= 0f)
        {
            currentLifeTime = 0f;
            Debug.Log("GameOver!");
            GameManager.Instance.TriggerGameOver();
        }
    }

    /// <summary>경험치 획득 및 레벨업 처리</summary>
    public void AddXP(float amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            xpToNextLevel *= xpScaleFactor;
            level++;
            OnLevelUp?.Invoke(level);
            GameManager.Instance.TriggerLevelUp();
        }
        if(level >= 3)
        {
            
        }
    }

    // ── 레벨업 능력 업그레이드 메서드 ─────────────────────────

    public void UpgradeMoveSpeed(float amount)      => moveSpeed += amount;
    public void UpgradeAttackDamage(float amount)   => attackDamage += amount;
    public void UpgradeFireRate(float amount)        => attackFireRate = Mathf.Clamp(attackFireRate + amount, 0.1f, 10f);
    public void UpgradeMaxLifeTime(float amount)
    {
        maxLifeTime = Mathf.Min(maxLifeTime + amount, maxLifeTimeCap);
        currentLifeTime = Mathf.Min(currentLifeTime + amount, maxLifeTimeCap);
    }
    public void UpgradeTimeReward(float amount)     => timeRewardPerKill += amount;
    public void UpgradeProjectileSpeed(float amount) => projectileSpeed += amount;

    /// <summary>블랙 마나 오브 수집 시 생명유지시간 회복 (maxLifeTimeCap 초과분은 경험치 변환)</summary>
    public void HealLifeTime(float amount)
    {
        float overflow = (currentLifeTime + amount) - maxLifeTimeCap;
        currentLifeTime = Mathf.Min(currentLifeTime + amount, maxLifeTimeCap);

        if (overflow > 0f)
            ExpOverflowConverter.Instance?.Convert(overflow);
    }
}
