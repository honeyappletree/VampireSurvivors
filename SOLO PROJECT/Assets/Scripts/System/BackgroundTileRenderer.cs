using UnityEngine;
using System.Collections.Generic;

public class BackgroundTileRenderer : MonoBehaviour
{
    [Header("타일 설정")]
    public Sprite tileSprite;
    public int    extraTiles = 2;

    float    _tileSize;
    Camera   _cam;
    Transform _player;

    // 현재 생성된 타일 딕셔너리 (그리드 좌표 → GameObject)
    Dictionary<Vector2Int, GameObject> _tiles
        = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        _cam = Camera.main;

        // tileSize 자동 계산
        if (tileSprite != null)
            _tileSize = tileSprite.bounds.size.x;
        else
        {
            Debug.LogWarning("[BackgroundTileRenderer] tileSprite 없음");
            return;
        }

        // 플레이어 찾기
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        UpdateTiles();
    }

    void Update()
    {
        UpdateTiles();
    }

    void UpdateTiles()
    {
        if (tileSprite == null || _tileSize <= 0f) return;

        // 기준 위치 (플레이어 또는 카메라)
        Vector3 center = _player != null
            ? _player.position
            : _cam.transform.position;

        float halfH = _cam.orthographicSize;
        float halfW = halfH * _cam.aspect;

        int tilesX = Mathf.CeilToInt(halfW / _tileSize) + extraTiles;
        int tilesY = Mathf.CeilToInt(halfH / _tileSize) + extraTiles;

        // 현재 필요한 그리드 좌표 범위
        int centerX = Mathf.RoundToInt(center.x / _tileSize);
        int centerY = Mathf.RoundToInt(center.y / _tileSize);

        HashSet<Vector2Int> needed = new HashSet<Vector2Int>();
        for (int y = centerY - tilesY; y <= centerY + tilesY; y++)
        for (int x = centerX - tilesX; x <= centerX + tilesX; x++)
            needed.Add(new Vector2Int(x, y));

        // 범위 벗어난 타일 제거
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kv in _tiles)
            if (!needed.Contains(kv.Key))
            {
                Destroy(kv.Value);
                toRemove.Add(kv.Key);
            }
        foreach (var key in toRemove)
            _tiles.Remove(key);

        // 새로 필요한 타일 생성
        foreach (var coord in needed)
        {
            if (_tiles.ContainsKey(coord)) continue;

            var go = new GameObject($"Tile_{coord.x}_{coord.y}");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(
                coord.x * _tileSize,
                coord.y * _tileSize,
                0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = tileSprite;
            sr.sortingOrder = -10;

            _tiles[coord] = go;
        }
    }
}
