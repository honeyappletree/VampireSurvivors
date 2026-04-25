using UnityEngine;

[CreateAssetMenu(fileName = "BlackManaData", menuName = "Game/BlackManaData")]
public class BlackManaData : ScriptableObject
{
    [Header("회복량")]
    public float healAmount = 3f;         // 수집 시 플레이어 생명유지시간 회복 (초)

    [Header("자석 효과")]
    public float attractRadius = 2.5f;   // 끌림 시작 거리
    public float moveSpeed     = 4f;     // 끌려가는 속도
    public float pickupRadius  = 0.8f;   // 수집 거리
}
