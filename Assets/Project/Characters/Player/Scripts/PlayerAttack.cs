using UnityEngine;

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

        bool onWall = wallJump != null && wallJump.IsWallSliding;
        bool dashBusy = dash != null && (dash.IsBusy || dash.ConsumedClickThisFrame);

        if (GameInput.AttackPressed && !IsAttacking && !onWall && !dashBusy && Time.time >= nextAttackTime)
            IsAttacking = true;
    }

    public void FinishAttack()
    {
        IsAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }
}