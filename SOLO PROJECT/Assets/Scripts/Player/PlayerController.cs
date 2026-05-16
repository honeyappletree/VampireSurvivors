using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// WASD 플레이어 이동 처리 (New/Old Input System 모두 지원)
/// 직접 위치 이동 방식 (MovePosition) — 관성/슬라이딩 없음
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D    _rb;
    SpriteRenderer _sr;
    Animator       _anim;
    Vector2        _moveDir;
    Vector2        _lastMoveDir = Vector2.right;
    bool           _isDead;

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _sr   = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();

        _rb.gravityScale    = 0f;
        _rb.linearDamping   = 0f;
        _rb.angularDamping  = 0f;
        _rb.constraints     = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        var slot = GetComponent<WeaponSlotManager>();
        if (slot == null)
            slot = FindObjectOfType<WeaponSlotManager>();
        if (slot != null)
            slot.AddWeapon(typeof(DarkBolt));
    }

    void Update()
    {
        if (_isDead) return;

        if (GameManager.Instance == null)
        {
            _moveDir = Vector2.zero;
            return;
        }

        // 사망: GameOver 전환 감지
        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            _isDead  = true;
            _moveDir = Vector2.zero;
            _anim.SetBool("isMoving", false);
            return;
        }

        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            _moveDir = Vector2.zero;
            _anim.SetBool("isMoving", false);
            return;
        }

        float x = 0f, y = 0f;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    y += 1f;
        }
#else
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
#endif

        _moveDir = new Vector2(x, y).normalized;

        // 이동 파라미터 갱신
        _anim.SetBool("isMoving", _moveDir != Vector2.zero);

        // 이동 방향에 따라 좌우 반전 (멈출 때는 마지막 방향 유지)
        if (_moveDir != Vector2.zero) _lastMoveDir = _moveDir;
        if (_lastMoveDir.x < 0f)      _sr.flipX = true;
        else if (_lastMoveDir.x > 0f) _sr.flipX = false;
    }

    void FixedUpdate()
    {
        if (_isDead) return;
        if (PlayerStats.Instance == null) return;
        _rb.MovePosition(_rb.position + _moveDir * PlayerStats.Instance.moveSpeed * Time.fixedDeltaTime);
    }
}
