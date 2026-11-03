using System;
using UnityEngine;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Run,
        Jump,
        Fall
    }

    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";

    public static Action<MoveState> OnPlayerMoveStateChanged;

    public MoveState CurrentMoveState { get; private set; } = MoveState.Idle;

    private Animator animator;
    private Rigidbody2D rb;
    private PlayerJump playerJump;
    private float xPosLastFrame;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
    }

    void Update()
    {
        bool grounded = playerJump != null && playerJump.IsGrounded;
        bool moved = !Mathf.Approximately(transform.position.x, xPosLastFrame);

        if (!grounded)
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
            case MoveState.Idle: HandleIdle(); break;
            case MoveState.Run:  HandleRun();  break;
            case MoveState.Jump: HandleJump(); break;
            case MoveState.Fall: HandleFall(); break;
            default:
                Debug.LogError($"Invalid movement state: {moveState}");
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }

    private void HandleIdle() => animator.Play(idleAnim);
    private void HandleRun()  => animator.Play(runAnim);
    private void HandleJump() => animator.Play(jumpAnim);
    private void HandleFall() => animator.Play(fallAnim);
}