using UnityEngine;

/// <summary>
/// 경험치 오브
/// - 플레이어가 가까이 오면 자동으로 끌려가서 수집됨
/// </summary>
public class XPOrb : MonoBehaviour
{
    [Header("경험치")]
    public float xpAmount = 15f;

    [Header("자석 효과")]
    public float attractRadius = 3.5f;   // 이 거리 이내로 들어오면 끌림 시작
    public float attractSpeed = 7f;      // 끌려가는 속도
    public float collectRadius = 0.25f;  // 이 거리 이내면 수집

    private Transform player;
    private bool attracted = false;

    void Start()
    {
        if (PlayerStats.Instance != null)
            player = PlayerStats.Instance.transform;
    }

    void Update()
    {
        if (player == null)
        {
            if (PlayerStats.Instance != null)
                player = PlayerStats.Instance.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // 자석 범위 내에 들어오면 끌림 활성화
        if (dist <= attractRadius)
            attracted = true;

        if (attracted)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime
            );

            // 수집 범위에 도달하면 경험치 지급 후 소멸
            if (dist <= collectRadius)
            {
                PlayerStats.Instance?.AddXP(xpAmount);
                Destroy(gameObject);
            }
        }
    }
}
