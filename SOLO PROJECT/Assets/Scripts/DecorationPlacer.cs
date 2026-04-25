// ================================================================
// DecorationPlacer.cs
// 경로: Assets/Scripts/System/DecorationPlacer.cs
//
// 배경 위에 장식 오브젝트(바위, 방패, 검, 깃발)를 랜덤 배치
// 사용법: 빈 GameObject에 추가 → Inspector에서 스프라이트 연결
// ================================================================

using UnityEngine;
using System.Collections.Generic;

public class DecorationPlacer : MonoBehaviour
{
    [System.Serializable]
    public class DecoGroup
    {
        public string     groupName;
        public Sprite[]   sprites;
        [Range(0, 20)]
        public int        count     = 5;
        [Range(0f, 1f)]
        public float      spawnChance = 0.8f;
        public Vector2    scaleRange  = new Vector2(0.8f, 1.4f);
    }

    [Header("배치 범위")]
    [SerializeField] Vector2  mapSize     = new Vector2(50f, 50f);
    [SerializeField] float    edgeMargin  = 3f;

    [Header("정렬")]
    [SerializeField] int      sortingOrder = -5;  // 바닥 위, 캐릭터 아래

    [Header("데코 그룹")]
    [SerializeField] DecoGroup[] groups;

    [Header("플레이어 근처 제외")]
    [SerializeField] float    clearRadius = 5f;   // 시작 지점 주변 빈 공간

    void Start()
    {
        PlaceAll();
    }

    public void PlaceAll()
    {
        // 기존 데코 정리
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        if (groups == null || groups.Length == 0) return;

        foreach (var group in groups)
        {
            if (group.sprites == null || group.sprites.Length == 0) continue;

            for (int i = 0; i < group.count; i++)
            {
                if (Random.value > group.spawnChance) continue;

                Vector2 pos = GetRandomPosition();
                PlaceDeco(group, pos);
            }
        }
    }

    void PlaceDeco(DecoGroup group, Vector2 pos)
    {
        var go = new GameObject($"Deco_{group.groupName}");
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        // 랜덤 스케일 & Z회전 (살짝 기울이기)
        float scale = Random.Range(group.scaleRange.x, group.scaleRange.y);
        float rot   = Random.Range(-15f, 15f);
        go.transform.localScale    = Vector3.one * scale;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rot);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = group.sprites[Random.Range(0, group.sprites.Length)];
        sr.sortingOrder = sortingOrder;

        // 원근감 — y좌표 기반 sortingOrder 미세 조정
        sr.sortingOrder = sortingOrder - Mathf.RoundToInt(pos.y);
    }

    Vector2 GetRandomPosition()
    {
        float halfX = mapSize.x * 0.5f - edgeMargin;
        float halfY = mapSize.y * 0.5f - edgeMargin;
        Vector2 pos;
        int tries = 0;
        do
        {
            pos = new Vector2(
                Random.Range(-halfX, halfX),
                Random.Range(-halfY, halfY)
            );
            tries++;
        }
        while (pos.magnitude < clearRadius && tries < 30);
        return pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(mapSize.x, mapSize.y, 0f));
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, clearRadius);
    }
#endif
}
