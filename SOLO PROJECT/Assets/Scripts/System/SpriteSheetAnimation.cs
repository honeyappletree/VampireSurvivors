using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimation : MonoBehaviour
{
    [Header("스프라이트 시트")]
    public Texture2D sheetTexture;

    [Header("Walk 설정")]
    public int   walkFrameCount = 6;
    public int   walkStartRow   = 0;
    public float walkFPS        = 10f;

    [Header("Death 설정")]
    public int   deathFrameCount = 4;
    public int   deathStartRow   = 1;
    public float deathFPS        = 8f;

    [Header("시트 구조")]
    public int totalCols = 6;
    public int totalRows = 2;

    SpriteRenderer _sr;
    Sprite[]       _sprites;

    float _timer;
    int   _currentFrame;
    bool  _isDead;
    bool  _deathFinished;
    int   _frameCount;
    int   _startRow;
    float _fps;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        BuildSprites();
        PlayWalk();
    }

    void BuildSprites()
    {
        if (sheetTexture == null) return;

        int fw    = sheetTexture.width  / totalCols;
        int fh    = sheetTexture.height / totalRows;
        int total = totalCols * totalRows;

        _sprites = new Sprite[total];

        for (int row = 0; row < totalRows; row++)
        for (int col = 0; col < totalCols; col++)
        {
            int idx    = row * totalCols + col;
            int texRow = (totalRows - 1 - row); // Unity 텍스처는 좌하단 기준
            var rect   = new Rect(col * fw, texRow * fh, fw, fh);

            _sprites[idx] = Sprite.Create(
                sheetTexture, rect,
                new Vector2(0.5f, 0f), 64f);
        }
    }

    public void PlayWalk()
    {
        _isDead        = false;
        _deathFinished = false;
        _frameCount    = walkFrameCount;
        _startRow      = walkStartRow;
        _fps           = walkFPS;
        _currentFrame  = 0;
        _timer         = 0f;
        ApplyFrame();
    }

    public void PlayDeath()
    {
        if (_isDead) return;
        _isDead       = true;
        _frameCount   = deathFrameCount;
        _startRow     = deathStartRow;
        _fps          = deathFPS;
        _currentFrame = 0;
        _timer        = 0f;
        ApplyFrame();
    }

    void Update()
    {
        if (_deathFinished) return;

        _timer += Time.deltaTime;
        if (_timer < 1f / _fps) return;

        _timer = 0f;
        _currentFrame++;

        if (_isDead && _currentFrame >= _frameCount)
        {
            _currentFrame  = _frameCount - 1;
            _deathFinished = true;
        }
        else
        {
            _currentFrame %= _frameCount;
        }

        ApplyFrame();
    }

    void ApplyFrame()
    {
        if (_sprites == null || _sr == null) return;

        int idx = _startRow * totalCols + _currentFrame;
        if (idx < _sprites.Length && _sprites[idx] != null)
            _sr.sprite = _sprites[idx];
    }
}
