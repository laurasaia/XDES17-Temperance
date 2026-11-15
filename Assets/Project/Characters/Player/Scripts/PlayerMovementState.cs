using System;
using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState { Idle, Run, Jump, Fall, WallSlide, Attack, Hurt, Death }

    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";
    private const string wallSlideAnim = "WallSlide";
    private const string attackAnim = "Attack";
    private const string hurtAnim = "Hurt";
    private const string deathAnim = "Death";

    public static Action<MoveState> OnPlayerMoveStateChanged;

    public MoveState CurrentMoveState { get; private set; } = MoveState.Idle;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerJump playerJump;
    private PlayerWallJump wallJump;
    private PlayerAttack attack;
    private PlayerHealth health;
    private float xPosLastFrame;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        wallJump = GetComponent<PlayerWallJump>();
        attack = GetComponent<PlayerAttack>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        bool grounded = playerJump != null && playerJump.IsGrounded;
        bool moved = !Mathf.Approximately(transform.position.x, xPosLastFrame);
        bool wallSliding = wallJump != null && wallJump.IsWallSliding;

        if (health != null && health.IsDead)
        {
            SetMoveState(MoveState.Death);
        }
        else if (health != null && health.IsHurt)
        {
            SetMoveState(MoveState.Hurt);
        }
        else if (attack != null && attack.IsAttacking)
        {
            SetMoveState(MoveState.Attack);

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(attackAnim) && info.normalizedTime >= 1f)
                attack.FinishAttack();
        }
        else if (wallSliding) SetMoveState(MoveState.WallSlide);
        else if (!grounded) SetMoveState(rb.linearVelocity.y > 0.1f ? MoveState.Jump : MoveState.Fall);
        else if (moved) SetMoveState(MoveState.Run);
        else SetMoveState(MoveState.Idle);

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
            case MoveState.Hurt:      animator.Play(hurtAnim, 0, 0f); break;
            case MoveState.Death:     animator.Play(deathAnim, 0, 0f); break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }
}