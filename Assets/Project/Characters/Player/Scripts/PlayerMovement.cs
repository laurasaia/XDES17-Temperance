using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    [Header("Física (atrito)")]
    [SerializeField] private PhysicsMaterial2D noFriction;
    [SerializeField] private PhysicsMaterial2D withFriction;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private PlayerJump playerJump;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        playerJump = GetComponent<PlayerJump>();
    }

    void Update()
    {
        moveInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput += 1f;
        }

        // Vira o sprite conforme a direção (mantém o último lado quando parado)
        if (moveInput > 0f) sr.flipX = false;
        else if (moveInput < 0f) sr.flipX = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Atrito só quando está parado E no chão. No ar ou andando: sem atrito.
        bool grounded = playerJump != null && playerJump.IsGrounded;
        PhysicsMaterial2D desired = (moveInput == 0f && grounded) ? withFriction : noFriction;

        if (col.sharedMaterial != desired)
        {
            col.sharedMaterial = desired;
        }
    }
}