using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float airAcceleration = 150f;
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D withFriction;
    [SerializeField] private Transform attackHitbox;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private PlayerJump playerJump;
    private PlayerWallJump wallJump;
    private PlayerAttack attack;
    private PlayerHealth health;
    private float moveInput;

    public bool FacingLeft { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        playerJump = GetComponent<PlayerJump>();
        wallJump = GetComponent<PlayerWallJump>();
        attack = GetComponent<PlayerAttack>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        moveInput = 0f;

        if (health != null && (health.IsDead || health.IsHurt)) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput += 1f;
        }

        bool attacking = attack != null && attack.IsAttacking;
        bool grounded = playerJump != null && playerJump.IsGrounded;

        if (!(attacking && grounded))
        {
            if (moveInput > 0f) FacingLeft = false;
            else if (moveInput < 0f) FacingLeft = true;
        }
    }

    void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Durante o hurt, o knockback manda na velocidade
        if (health != null && health.IsHurt) return;

        bool grounded = playerJump != null && playerJump.IsGrounded;
        bool wallJumping = wallJump != null && wallJump.IsWallJumping;
        bool attacking = attack != null && attack.IsAttacking;

        if (grounded)
        {
            float x = attacking ? 0f : moveInput * speed;
            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        }
        else if (moveInput != 0f)
        {
            float x = Mathf.MoveTowards(rb.linearVelocity.x, moveInput * speed, airAcceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
        }
        else if (!wallJumping)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        bool stopped = grounded && (moveInput == 0f || attacking);
        PhysicsMaterial2D desired = stopped ? withFriction : noFriction;
        if (col.sharedMaterial != desired) col.sharedMaterial = desired;
    }

    void LateUpdate()
    {
        sr.flipX = FacingLeft;

        if (attackHitbox != null)
            attackHitbox.localScale = new Vector3(FacingLeft ? -1f : 1f, 1f, 1f);
    }
}