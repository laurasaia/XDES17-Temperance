using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.1f;

    private PlayerWallJump wallJump;
    private PlayerHealth health;
    private PlayerDash dash;
    private float nextAttackTime;

    public bool IsAttacking { get; private set; }

    void Awake()
    {
        wallJump = GetComponent<PlayerWallJump>();
        health = GetComponent<PlayerHealth>();
        dash = GetComponent<PlayerDash>();
    }

    void Update()
    {
        if (health != null && (health.IsHurt || health.IsDead))
        {
            IsAttacking = false;
            return;
        }

        if (Mouse.current == null) return;

        bool onWall = wallJump != null && wallJump.IsWallSliding;
        bool dashBusy = dash != null && (dash.IsBusy || dash.ConsumedClickThisFrame);

        if (Mouse.current.leftButton.wasPressedThisFrame
            && !IsAttacking && !onWall && !dashBusy
            && Time.time >= nextAttackTime)
        {
            IsAttacking = true;
        }
    }

    // Chamado pelo PlayerMovementState quando o clip termina
    public void FinishAttack()
    {
        IsAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }
}