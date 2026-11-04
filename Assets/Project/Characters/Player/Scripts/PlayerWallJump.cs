using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWallJump : MonoBehaviour
{
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckRadius = 0.1f;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float jumpForceY = 13f;
    [SerializeField] private float awayForceX = 5f;
    [SerializeField] private float lateralDecay = 7f; // quanto maior, mais cedo a queda fica reta

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerJump playerJump;
    private float wallCheckBaseX;
    private bool jumpRequested;

    public bool IsWallSliding { get; private set; }
    public bool IsWallJumping { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        wallCheckBaseX = Mathf.Abs(wallCheck.localPosition.x);
    }

    void Update()
    {
        float side = movement.FacingLeft ? -1f : 1f;
        wallCheck.localPosition = new Vector3(wallCheckBaseX * side, wallCheck.localPosition.y, 0f);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && IsWallSliding)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        bool grounded = playerJump != null && playerJump.IsGrounded;
        bool touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        IsWallSliding = touchingWall && !grounded && rb.linearVelocity.y < 0f;

        if (IsWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        if (jumpRequested)
        {
            jumpRequested = false;

            float away = movement.FacingLeft ? 1f : -1f; // oposto ao lado da parede
            rb.linearVelocity = new Vector2(away * awayForceX, jumpForceY);
            movement.FacingLeft = away < 0f;
            IsWallJumping = true;
        }
        else if (IsWallJumping)
        {
            // O impulso lateral vai se perdendo sozinho, sem corte brusco
            float x = Mathf.MoveTowards(rb.linearVelocity.x, 0f, lateralDecay * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

            if (grounded || x == 0f) IsWallJumping = false;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}