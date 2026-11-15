using UnityEngine;
using UnityEngine.InputSystem;

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

    // True no frame em que o clique foi consumido pelo dash (o ataque comum deve ignorá-lo)
    public bool ConsumedClickThisFrame => lastClickFrame == Time.frameCount;

    // True enquanto o dash ou o DashAttack estão ativos
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

        if (IsDashAttacking) return; // quem encerra é o PlayerMovementState, ao fim do clip

        if (IsDashing)
        {
            // Clique durante o dash: encerra o movimento e termina com o ataque
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                lastClickFrame = Time.frameCount;
                IsDashing = false;
                IsDashAttacking = true;

                rb.gravityScale = originalGravity;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }

        if (Keyboard.current == null) return;

        bool onWall = wallJump != null && wallJump.IsWallSliding;
        bool attacking = attack != null && attack.IsAttacking;
        bool canAir = grounded || !airDashUsed;

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame
            && Time.time >= nextDashTime && !onWall && !attacking && canAir)
        {
            StartDash(grounded);
        }
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

    // Chamado pelo PlayerMovementState ao fim do clip (Dash ou DashAttack)
    public void EndDash()
    {
        IsDashing = false;
        IsDashAttacking = false;
        nextDashTime = Time.time + dashCooldown;
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
}