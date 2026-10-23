using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player: jump (Space), dash (Shift), 3 lives, hit feedback, death.
/// Uses ONLY the new Input System (InputAction objects created in code).
/// Needs: kinematic Rigidbody2D, trigger-capable Collider2D, SpriteRenderer, Animator.
/// Animator parameters: Grounded (bool), Hit (trigger), Dead (bool).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 3;

    [Header("Jump")]
    [SerializeField] private float groundY = -3f;
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private float gravity = -30f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 2.5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Hit feedback")]
    [SerializeField] private float invulnerabilityTime = 1.5f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.15f;

    public int MaxLives => maxLives;
    public int Lives { get; private set; }
    public bool IsDead { get; private set; }
    public event Action<int> LivesChanged;

    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    private Animator animator;
    private SpriteRenderer sr;
    private InputAction jumpAction;
    private InputAction dashAction;

    private float velocityY;
    private bool grounded = true;
    private bool dashing;
    private float nextDashTime;
    private float invulnerableUntil;
    private float homeX;

    private bool IsInvulnerable => dashing || Time.time < invulnerableUntil;
    private bool CanAct => !IsDead && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        Lives = maxLives;
        homeX = transform.position.x;

        // New Input System actions defined in code (no legacy Input class anywhere).
        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/shift");   // left or right shift
        dashAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void OnEnable()
    {
        jumpAction.performed += OnJump;
        dashAction.performed += OnDash;
        jumpAction.Enable();
        dashAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        dashAction.performed -= OnDash;
        jumpAction.Disable();
        dashAction.Disable();
    }

    private void OnDestroy()
    {
        jumpAction?.Dispose();
        dashAction?.Dispose();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!CanAct || !grounded) return;
        velocityY = jumpVelocity;
        grounded = false;
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (!CanAct || dashing || Time.time < nextDashTime) return;
        StartCoroutine(DashRoutine());
    }

    private void Update()
    {
        if (!IsDead && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            ApplyVerticalMotion();

        animator.SetBool(GroundedHash, grounded);
    }

    private void ApplyVerticalMotion()
    {
        if (grounded) return;

        velocityY += gravity * Time.deltaTime;
        Vector3 p = transform.position;
        p.y += velocityY * Time.deltaTime;

        if (p.y <= groundY)
        {
            p.y = groundY;
            velocityY = 0f;
            grounded = true;
        }
        transform.position = p;
    }

    private IEnumerator DashRoutine()
    {
        dashing = true;
        nextDashTime = Time.time + dashCooldown;

        float half = dashDuration * 0.5f;
        float t = 0f;

        // Burst forward...
        while (t < half)
        {
            if (GameManager.Instance.State == GameState.Playing) t += Time.deltaTime;
            SetX(Mathf.Lerp(homeX, homeX + dashDistance, t / half));
            yield return null;
        }

        // ...and glide back to the home position.
        t = 0f;
        while (t < half)
        {
            if (GameManager.Instance.State == GameState.Playing) t += Time.deltaTime;
            SetX(Mathf.Lerp(homeX + dashDistance, homeX, t / half));
            yield return null;
        }

        SetX(homeX);
        dashing = false;
    }

    private void SetX(float x)
    {
        Vector3 p = transform.position;
        p.x = x;
        transform.position = p;
    }

    public void TakeDamage()
    {
        if (IsDead || IsInvulnerable) return;

        Lives = Mathf.Max(0, Lives - 1);
        LivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger(HitHash);
        invulnerableUntil = Time.time + invulnerabilityTime;
        StartCoroutine(HitFeedbackRoutine());
    }

    private IEnumerator HitFeedbackRoutine()
    {
        // Short colour flash...
        sr.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.color = Color.white;

        // ...then blink while invulnerable.
        while (Time.time < invulnerableUntil && !IsDead)
        {
            SetAlpha(0.35f);
            yield return new WaitForSeconds(0.1f);
            SetAlpha(1f);
            yield return new WaitForSeconds(0.1f);
        }
        SetAlpha(1f);
    }

    private void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    private void Die()
    {
        IsDead = true;
        StopAllCoroutines();
        dashing = false;
        sr.color = Color.white;
        animator.SetBool(DeadHash, true);
        GameManager.Instance.GameOver();
    }
}
