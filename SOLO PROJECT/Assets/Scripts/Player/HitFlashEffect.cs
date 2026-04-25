using System.Collections;
using UnityEngine;

public class HitFlashEffect : MonoBehaviour
{
    [Header("깜빡임 설정")]
    public float flashDuration  = 0.1f;
    public float flashInterval  = 0.1f;
    public int   flashCount     = 3;
    public Color flashColor     = Color.white;

    SpriteRenderer _sr;
    Color          _originalColor;
    bool           _isFlashing;

    void Awake()
    {
        _sr            = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _sr.color;
    }

    public void TriggerFlash()
    {
        if (_isFlashing) StopCoroutine(nameof(FlashRoutine));
        StartCoroutine(nameof(FlashRoutine));
    }

    IEnumerator FlashRoutine()
    {
        _isFlashing = true;

        for (int i = 0; i < flashCount; i++)
        {
            _sr.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            _sr.color = _originalColor;
            yield return new WaitForSeconds(flashInterval);
        }

        _sr.color   = _originalColor;
        _isFlashing = false;
    }
}
