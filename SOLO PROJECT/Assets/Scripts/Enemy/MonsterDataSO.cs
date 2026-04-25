using UnityEngine;

/// <summary>
/// ScriptableObject 기반 몬스터 데이터 컨테이너.
/// Unity 에디터에서 [우클릭 → Create → VampireSurvivor → MonsterData]로 에셋 생성.
/// 생성한 에셋을 Enemy 프리팹의 monsterData 슬롯에 연결하면 수치가 자동 적용된다.
///
/// GDD v2: maxHP = 몬스터 내구력 (생명유지시간과 통합 운용).
/// 처치 보상은 timeReward 단일 필드로 통합.
/// maxLifeTimeCap(90초) 초과분은 ExpOverflowConverter가 경험치로 자동 변환.
///
/// ─────────────────────────────────────────────────────
/// 체크리스트 v2 몬스터 6종 기준 수치
/// ─────────────────────────────────────────────────────
/// 이름              maxHP  속도  공격력  공격속도  회복(초)
/// 녹색 좀비            60   1.5     8     1.0      5
/// 흰색 해골            40   1.8     6     1.2      5
/// 흰색 해골개          30   3.5     5     2.0      5
/// 붉은 좀비           200   1.0    15     0.8      5
/// 보라 유령            50   1.2    12     1.5      5
/// 불멸의 사령관      1000   1.0    25     0.5     30    (보스)
/// ─────────────────────────────────────────────────────
/// </summary>
[CreateAssetMenu(menuName = "VampireSurvivor/MonsterData", fileName = "NewMonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string monsterName  = "이름 없음";

    [Header("스프라이트")]
    [SerializeField] private Sprite idleSprite;

    [Header("전투 스탯")]
    [SerializeField] private float maxHP         = 50f;   // 최대 HP (= 몬스터 내구력)
    [SerializeField] private float moveSpeed     = 2.0f;  // 이동 속도
    [SerializeField] private float attackDamage  = 10f;   // 공격력 (초당 접촉 데미지)
    [SerializeField] private float attackRate    = 1.0f;  // 공격 속도 (향후 패턴 확장용)

    [Header("처치 보상 (초과분은 ExpOverflowConverter → 경험치 자동 변환)")]
    [SerializeField] private float timeReward    = 5f;    // 처치 시 플레이어 생명유지시간 회복 (초)

    // ── 읽기 전용 프로퍼티 ───────────────────────────────────

    public string MonsterName   => monsterName;
    public Sprite IdleSprite    => idleSprite;
    public float  MaxHP         => maxHP;
    public float  MoveSpeed     => moveSpeed;
    public float  AttackDamage  => attackDamage;
    public float  AttackRate    => attackRate;
    public float  TimeReward    => timeReward;
}
