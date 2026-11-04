using System;
using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Run,
        Jump,
        Fall,
        WallSlide,
        Attack
    }

    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";
    private const string wallSlideAnim = "WallSlide";
    private const string attackAnim = "Attack";

    public static Action<MoveState> OnPlayerMoveStateChanged;

    public MoveState CurrentMoveState { get; private set; } = MoveState.Idle;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerJump playerJump;
    private PlayerWallJump wallJump;
    private PlayerAttack attack;
    private float xPosLastFrame;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        wallJump = GetComponent<PlayerWallJump>();
        attack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        bool grounded = playerJump != null && playerJump.IsGrounded;
        bool moved = !Mathf.Approximately(transform.position.x, xPosLastFrame);
        bool wallSliding = wallJump != null && wallJump.IsWallSliding;

        if (attack != null && attack.IsAttacking)
        {
            SetMoveState(MoveState.Attack);

            // O clip decide quando o ataque acaba
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(attackAnim) && info.normalizedTime >= 1f)
            {
                attack.FinishAttack();
            }
        }
        else if (wallSliding)
        {
            SetMoveState(MoveState.WallSlide);
        }
        else if (!grounded)
        {
            SetMoveState(rb.linearVelocity.y > 0.1f ? MoveState.Jump : MoveState.Fall);
        }
        else if (moved)
        {
            SetMoveState(MoveState.Run);
        }
        else
        {
            SetMoveState(MoveState.Idle);
        }

        xPosLastFrame = transform.position.x;
    }

    public void SetMoveState(MoveState moveState)
    {
        if (moveState == CurrentMoveState) return;

        switch (moveState)
        {
            case MoveState.Idle:      animator.Play(idleAnim); break;
            case MoveState.Run:       animator.Play(runAnim); break;
            case MoveState.Jump:      animator.Play(jumpAnim); break;
            case MoveState.Fall:      animator.Play(fallAnim); break;
            case MoveState.WallSlide: animator.Play(wallSlideAnim); break;
            case MoveState.Attack:    animator.Play(attackAnim, 0, 0f); break;
            default:
                Debug.LogError($"Invalid movement state: {moveState}");
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }
}