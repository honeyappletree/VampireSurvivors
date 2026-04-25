using UnityEngine;

/// <summary>
/// 카메라가 플레이어를 부드럽게 따라가는 컴포넌트
/// Main Camera에 부착하여 사용
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target;

    [Header("부드러움 (높을수록 빠르게 따라감)")]
    [Range(1f, 20f)]
    public float smoothSpeed = 6f;

    [Header("오프셋 (카메라 위치 보정)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        if (target == null)
        {
            // PlayerStats가 있으면 자동으로 플레이어를 찾음
            if (PlayerStats.Instance != null)
                target = PlayerStats.Instance.transform;
            return;
        }

        Vector3 desiredPos = target.position + offset;
        desiredPos.z = transform.position.z; // Z축 고정 (2D)

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
