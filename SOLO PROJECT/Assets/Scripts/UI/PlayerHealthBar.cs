using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("참조")]
    public PlayerStats playerStats;

    [Header("설정")]
    public float barWidth  = 0.9f;
    public float barHeight = 0.09f;
    public Vector3 offset  = new Vector3(0f, -0.6f, 0f);

    public Color colorFull = new Color(0.4f, 0.8f, 1.0f);
    public Color colorLow  = new Color(1.0f, 0.2f, 0.2f);

    GameObject     _bg;
    GameObject     _fill;
    SpriteRenderer _bgSr;
    SpriteRenderer _fillSr;

    void Start()
    {
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();

        transform.localPosition = offset;
        BuildBar();
    }

    void BuildBar()
    {
        _bg = new GameObject("HPBar_BG");
        _bg.transform.SetParent(transform);
        _bg.transform.localPosition = Vector3.zero;
        _bg.transform.localScale    = new Vector3(barWidth, barHeight, 1f);
        _bgSr = _bg.AddComponent<SpriteRenderer>();
        _bgSr.sprite       = CreatePixel();
        _bgSr.color        = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        _bgSr.sortingOrder = 10;

        _fill = new GameObject("HPBar_Fill");
        _fill.transform.SetParent(transform);
        _fill.transform.localPosition = new Vector3(-barWidth * 0.5f, 0f, -0.01f);
        _fill.transform.localScale    = new Vector3(barWidth, barHeight, 1f);
        _fillSr = _fill.AddComponent<SpriteRenderer>();
        _fillSr.sprite       = CreatePixel();
        _fillSr.color        = colorFull;
        _fillSr.sortingOrder = 11;
    }

    void LateUpdate()
    {
        if (playerStats == null || _fill == null) return;

        transform.localPosition = offset;

        float ratio = Mathf.Clamp01(
            playerStats.CurrentLifeTime / playerStats.MaxLifeTime);

        _fill.transform.localScale = new Vector3(
            barWidth * ratio, barHeight, 1f);

        _fill.transform.localPosition = new Vector3(
            -barWidth * 0.5f + barWidth * ratio * 0.5f,
            0f, -0.01f);

        _fillSr.color = Color.Lerp(colorLow, colorFull, ratio);
    }

    Sprite CreatePixel()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f), 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + offset,
            new Vector3(barWidth, barHeight, 0));
    }
}
