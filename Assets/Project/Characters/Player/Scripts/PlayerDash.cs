using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashCooldown = 0.6f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerJump playerJump;
    private PlayerWallJump wallJump;
    private PlayerHealth health;
    private PlayerAttack attack;

    private float nextDashTime;
    private float originalGravity;
    private bool airDashUsed;
    private int lastClickFrame = -1;

    public bool IsDashing { get; private set; }
    public bool IsDashAttacking { get; private set; }
    public bool ConsumedClickThisFrame => lastClickFrame == Time.frameCount;
    public bool IsBusy => IsDashing || IsDashAttacking;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        wallJump = GetComponent<PlayerWallJump>();
        health = GetComponent<PlayerHealth>();
        attack = GetComponent<PlayerAttack>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (health != null && (health.IsHurt || health.IsDead))
        {
            if (IsBusy) EndDash();
            return;
        }

        bool grounded = playerJump != null && playerJump.IsGrounded;
        if (grounded) airDashUsed = false;

        if (IsDashAttacking) return;

        if (IsDashing)
        {
            if (GameInput.AttackPressed)
            {
                lastClickFrame = Time.frameCount;
                IsDashing = false;
                IsDashAttacking = true;

                rb.gravityScale = originalGravity;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        bool onWall = wallJump != null && wallJump.IsWallSliding;
        bool attacking = attack != null && attack.IsAttacking;
        bool canAir = grounded || !airDashUsed;

        if (GameInput.DashPressed && Time.time >= nextDashTime && !onWall && !attacking && canAir)
            StartDash(grounded);
    }

    void FixedUpdate()
    {
        if (!IsDashing) return;

        float dir = movement.FacingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
    }

    private void StartDash(bool grounded)
    {
        IsDashing = true;
        IsDashAttacking = false;
        if (!grounded) airDashUsed = true;

        originalGravity = rb.gravityScale > 0f ? rb.gravityScale : originalGravity;
        rb.gravityScale = 0f;
    }

    public void EndDash()
    {
        IsDashing = false;
        IsDashAttacking = false;
        nextDashTime = Time.time + dashCooldown;
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}