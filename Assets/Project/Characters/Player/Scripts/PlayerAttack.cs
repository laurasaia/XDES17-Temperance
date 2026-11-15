using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.1f;

    private PlayerWallJump wallJump;
    private PlayerHealth health;
    private float nextAttackTime;

    public bool IsAttacking { get; private set; }

    void Awake()
    {
        wallJump = GetComponent<PlayerWallJump>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Se foi ferido ou morreu, cancela o ataque para não travar
        if (health != null && (health.IsHurt || health.IsDead))
        {
            IsAttacking = false;
            return;
        }

        if (Mouse.current == null) return;

        bool onWall = wallJump != null && wallJump.IsWallSliding;

        if (Mouse.current.leftButton.wasPressedThisFrame
            && !IsAttacking && !onWall
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