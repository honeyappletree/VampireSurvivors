using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlaceholderEnemyVisual : MonoBehaviour
{
    public enum EnemyType
    {
        ZombieGreen,
        SkeletonWhite,
        HoundSkeleton,
        ZombieRed,
        GhostPurple,
        Boss
    }

    public EnemyType enemyType = EnemyType.ZombieGreen;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color  = GetColor();
    }

    Color GetColor()
    {
        return enemyType switch
        {
            EnemyType.ZombieGreen   => new Color(0.2f, 0.8f, 0.2f),
            EnemyType.SkeletonWhite => new Color(0.9f, 0.9f, 0.9f),
            EnemyType.HoundSkeleton => new Color(0.7f, 0.7f, 0.5f),
            EnemyType.ZombieRed     => new Color(0.8f, 0.2f, 0.2f),
            EnemyType.GhostPurple   => new Color(0.6f, 0.2f, 0.8f),
            EnemyType.Boss          => new Color(1.0f, 0.4f, 0.0f),
            _                       => Color.white
        };
    }

    Sprite CreateCircleSprite()
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 1;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Mathf.Sqrt((x-cx)*(x-cx)+(y-cy)*(y-cy));
            tex.SetPixel(x, y, dist <= r
                ? Color.white
                : Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0,0,size,size),
            new Vector2(0.5f, 0.5f), 32);
    }
}
