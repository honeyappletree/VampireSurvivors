using UnityEngine;

/// <summary>
/// 플레이어의 무기 슬롯(최대 3개)을 관리한다.
/// - LevelUpPanel에 무기 선택지를 등록 (Start에서 호출)
/// - 무기 획득: 빈 슬롯에 장착
/// - 무기 레벨업: 같은 타입을 재선택하면 LevelUp() 호출
/// - 슬롯 꽉 참: 경고 로그 출력 후 슬롯[0] 교체 (추후 교체 선택 UI로 대체)
///
/// [Inspector 연결 필수]
/// - rotatingOrbPrefab  : RotatingOrb 컴포넌트가 붙은 프리팹
/// - piercingBoltPrefab : PiercingBolt 컴포넌트가 붙은 프리팹
/// - homingMissilePrefab: HomingMissile 컴포넌트가 붙은 프리팹
/// </summary>
public class WeaponSlotManager : MonoBehaviour
{
    public static WeaponSlotManager Instance { get; private set; }

    [Header("무기 프리팹 (Inspector에서 연결)")]
    [SerializeField] private RotatingOrb   rotatingOrbPrefab;
    [SerializeField] private PiercingBolt  piercingBoltPrefab;
    [SerializeField] private HomingMissile homingMissilePrefab;

    [Header("슬롯 설정")]
    [SerializeField] private int maxSlots = 3;

    private WeaponBase[] slots;

    /// <summary>현재 장착된 무기 슬롯 배열 (읽기 전용)</summary>
    public WeaponBase[] Slots => slots;

    // ── 라이프사이클 ──────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        slots = new WeaponBase[maxSlots];
    }

    void Start()
    {
        // LevelUpPanel은 Awake에서 초기화 완료 → Start에서 안전하게 등록
        RegisterWeaponAbilities();
    }

    // ── LevelUpPanel 연동 ────────────────────────────────────

    void RegisterWeaponAbilities()
    {
        if (LevelUpPanel.Instance == null)
        {
            Debug.LogWarning("[WeaponSlotManager] LevelUpPanel.Instance가 없어 무기 등록을 건너뜁니다.");
            return;
        }

        LevelUpPanel.Instance.RegisterAbility(
            "회전 오브",
            "플레이어 주변을 공전하며 닿는 적에게 피해\n데미지 15 / 발동 0.5초",
            () => AcquireOrLevelUp(rotatingOrbPrefab)
        );
        LevelUpPanel.Instance.RegisterAbility(
            "관통 볼트",
            "적을 최대 3명 관통하는 직선 투사체 발사\n데미지 20 / 발동 1.0초",
            () => AcquireOrLevelUp(piercingBoltPrefab)
        );
        LevelUpPanel.Instance.RegisterAbility(
            "유도 미사일",
            "가장 가까운 적을 추적하는 미사일 발사\n데미지 25 / 발동 1.5초",
            () => AcquireOrLevelUp(homingMissilePrefab)
        );
    }

    // ── 슬롯 처리 ────────────────────────────────────────────

    void AcquireOrLevelUp(WeaponBase prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[WeaponSlotManager] 무기 프리팹이 Inspector에 연결되지 않았습니다.");
            return;
        }

        System.Type weaponType = prefab.GetType();

        // ① 이미 보유 중 → 레벨업
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].GetType() == weaponType)
            {
                if (slots[i].IsMaxLevel)
                    Debug.Log($"[WeaponSlotManager] {slots[i].WeaponName}은 이미 최고 레벨입니다.");
                else
                    slots[i].LevelUp();
                return;
            }
        }

        // ② 빈 슬롯 있음 → 신규 장착
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = SpawnWeapon(prefab);
                Debug.Log($"[WeaponSlotManager] 슬롯[{i}]에 {slots[i].WeaponName} 장착");
                return;
            }
        }

        // ③ 슬롯 꽉 참 → 슬롯[0] 강제 교체 (추후 교체 선택 UI로 대체)
        Debug.LogWarning("[WeaponSlotManager] 슬롯이 가득 찼습니다. 슬롯[0]을 새 무기로 교체합니다.");
        Destroy(slots[0].gameObject);
        slots[0] = SpawnWeapon(prefab);
    }

    WeaponBase SpawnWeapon(WeaponBase prefab)
    {
        Transform player = PlayerStats.Instance?.transform;
        WeaponBase weapon = Instantiate(
            prefab,
            player != null ? player.position : Vector3.zero,
            Quaternion.identity);

        if (player != null)
        {
            weapon.transform.SetParent(player);
            weapon.transform.localPosition = Vector3.zero;
        }
        return weapon;
    }

    /// <summary>타입으로 무기 직접 추가 (프리팹 없이). 이미 보유 중이면 레벨업.</summary>
    public bool AddWeapon(System.Type weaponType)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].GetType() == weaponType)
            {
                if (!slots[i].IsMaxLevel) slots[i].LevelUp();
                return true;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null) continue;
            var go = new GameObject(weaponType.Name);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            slots[i] = (WeaponBase)go.AddComponent(weaponType);
            Debug.Log($"[WeaponSlot] 슬롯[{i}]에 {slots[i].WeaponName} 장착 (타입 기반)");
            return true;
        }

        Debug.Log("[WeaponSlot] 슬롯 가득참");
        return false;
    }
}
