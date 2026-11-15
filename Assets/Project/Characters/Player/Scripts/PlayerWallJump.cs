using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWallJump : MonoBehaviour
{
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckRadius = 0.1f;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float jumpForceY = 12f;
    [SerializeField] private float awayForceX = 5f;
    [SerializeField] private float lateralDecay = 7f;
    [SerializeField] private float wallStickTime = 0.1f; // tempo grudado ao apertar a direção oposta

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerJump playerJump;
    private float wallCheckBaseX;
    private bool jumpRequested;
    private float stickCounter;

    public bool IsWallSliding { get; private set; }
    public bool IsWallJumping { get; private set; }

    // True enquanto o personagem está "preso" à parede e o movimento lateral deve ser ignorado
    public bool IsSticking => IsWallSliding && stickCounter > 0f;

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

        bool sliding = touchingWall && !grounded && rb.linearVelocity.y < 0f;

        // Direção oposta à parede pressionada?
        float wallSide = movement.FacingLeft ? -1f : 1f;
        float input = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) input -= 1f;
            if (Keyboard.current.dKey.isPressed) input += 1f;
        }
        bool pressingAway = input != 0f && Mathf.Sign(input) != wallSide;

        if (sliding)
        {
            // Começa o stick ao apertar a direção oposta; senão mantém o tempo cheio
            if (pressingAway) stickCounter = Mathf.Max(0f, stickCounter - Time.fixedDeltaTime);
            else stickCounter = wallStickTime;
        }
        else
        {
            stickCounter = wallStickTime;
        }

        IsWallSliding = sliding;

        if (IsWallSliding)
        {
            rb.linearVelocity = new Vector2(IsSticking ? 0f : rb.linearVelocity.x,
                                            Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }

        if (jumpRequested)
        {
            jumpRequested = false;

            float away = -wallSide;
            rb.linearVelocity = new Vector2(away * awayForceX, jumpForceY);
            movement.FacingLeft = away < 0f;
            IsWallJumping = true;
        }
        else if (IsWallJumping)
        {
            float x = Mathf.MoveTowards(rb.linearVelocity.x, 0f, lateralDecay * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

            if (grounded || x == 0f) IsWallJumping = false;
        }
    }
}