using UnityEngine;

/// <summary>
/// 생명유지시간이 maxLifeTimeCap(90초)에 도달했을 때
/// 초과 회복분을 경험치로 변환하는 시스템.
///
/// 연동 방식:
///   PlayerStats.AddTimeReward() 내부에서 초과분(초)을 계산한 뒤
///   ExpOverflowConverter.Instance.Convert(overflow)를 호출한다.
///
/// 변환 비율:
///   초과 1초 → xpPerSecond(기본값 10)점의 경험치
///   Inspector에서 xpPerSecond를 조정해 밸런스 튜닝 가능.
/// </summary>
public class ExpOverflowConverter : MonoBehaviour
{
    public static ExpOverflowConverter Instance { get; private set; }

    [Header("변환 비율")]
    [SerializeField] private float xpPerSecond = 10f; // 초과 1초당 전환되는 경험치량

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 초과 생명유지시간(초)을 경험치로 변환하여 플레이어에게 지급.
    /// PlayerStats.AddTimeReward()에서 호출된다.
    /// </summary>
    /// <param name="overflowSeconds">maxLifeTimeCap을 초과한 시간(초)</param>
    public void Convert(float overflowSeconds)
    {
        if (overflowSeconds <= 0f) return;

        float xp = overflowSeconds * xpPerSecond;
        PlayerStats.Instance?.AddXP(xp);

        Debug.Log($"[ExpOverflowConverter] 초과 {overflowSeconds:F2}초 → 경험치 {xp:F1}점 변환");
    }
}
