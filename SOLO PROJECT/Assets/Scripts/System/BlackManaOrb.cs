using UnityEngine;

/// <summary>
/// 블랙 마나 오브
/// - 적 처치 시 드롭, 플레이어가 가까이 오면 자동으로 끌려가서 수집됨
/// - 수집 시 플레이어 생명유지시간 회복 (HealLifeTime)
/// </summary>
public class BlackManaOrb : MonoBehaviour
{
    [Header("설정 (BlackManaData SO 또는 직접 입력)")]
    public BlackManaData data;

    private float _healAmount   = 3f;
    private float _attractRadius = 2.5f;
    private float _moveSpeed    = 4f;
    private float _pickupRadius = 0.8f;

    private Transform _player;
    private bool _attracted = false;

    /// <summary>스포너/Enemy에서 heal량 직접 지정할 때 호출</summary>
    public void Init(float heal)
    {
        _healAmount = heal;
    }

    void Start()
    {
        if (data != null)
        {
            _healAmount    = data.healAmount;
            _attractRadius = data.attractRadius;
            _moveSpeed     = data.moveSpeed;
            _pickupRadius  = data.pickupRadius;
        }

        if (PlayerStats.Instance != null)
            _player = PlayerStats.Instance.transform;
    }

    void Update()
    {
        if (_player == null)
        {
            if (PlayerStats.Instance != null)
                _player = PlayerStats.Instance.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist <= _attractRadius)
            _attracted = true;

        if (_attracted)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                _player.position,
                _moveSpeed * Time.deltaTime
            );

            if (dist <= _pickupRadius)
            {
                PlayerStats.Instance?.HealLifeTime(_healAmount);
                Destroy(gameObject);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0f, 0.8f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, _attractRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _pickupRadius);
    }
#endif
}
