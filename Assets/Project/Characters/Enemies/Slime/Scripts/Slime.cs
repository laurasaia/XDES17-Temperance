using System.Collections;
using UnityEngine;

public class Slime : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Chase, Attack, Hurt, Dead }

    [Header("Vida e dano")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int damage = 2;
    [SerializeField] private int contactDamage = 1;

    [Header("Movimento")]
    [SerializeField] private float patrolSpeed = 1.5f; // 0 = fica parado até ver o player
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private bool canChase = true;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Combate")]
    [SerializeField] private float sightRange = 6f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackHitDelay = 0.3f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Resistência a stun")]
    [SerializeField] private bool armorWhileAttacking = true; // não é interrompido enquanto ataca
    [SerializeField] private float hurtImmunityTime = 0.8f;   // tempo sem poder ser atordoado após um Hurt

    private float knockbackUntil;
    private float nextHurtAllowed;

    private const string idleAnim = "Idle";
    private const string walkAnim = "Walk";
    private const string attackAnim = "Attack";
    private const string hurtAnim = "Hurt";
    private const string deathAnim = "Death";

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Transform player;
    private PlayerHealth playerHealth;

    private State state = State.Patrol;
    private int health;
    private int dir = 1;
    private string currentAnim;
    private float nextAttackTime;
    private Coroutine actionRoutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    void Start()
    {
        PlayerHealth p = FindFirstObjectByType<PlayerHealth>();
        if (p != null)
        {
            player = p.transform;
            playerHealth = p;
        }
    }

    void FixedUpdate()
    {
        if (state == State.Dead) return;
        if (Time.time < knockbackUntil) return;

        if (state == State.Hurt || state == State.Attack)
        {
            Stop();
            return;
        }

        float dx = 0f;
        bool sees = false;

        if (player != null && playerHealth != null && !playerHealth.IsDead)
        {
            dx = player.position.x - transform.position.x;
            float dy = Mathf.Abs(player.position.y - transform.position.y);
            sees = canChase && Mathf.Abs(dx) <= sightRange && dy <= 2f;
        }

        if (sees)
        {
            state = State.Chase;
            dir = dx >= 0f ? 1 : -1;
            UpdateFlip();

            if (Mathf.Abs(dx) <= attackRange)
            {
                Stop();
                PlayAnim(idleAnim);
                if (Time.time >= nextAttackTime) actionRoutine = StartCoroutine(Attack());
            }
            else
            {
                Move(chaseSpeed);
            }
        }
        else
        {
            state = State.Patrol;

            if (patrolSpeed <= 0f)
            {
                Stop();
                PlayAnim(idleAnim);
            }
            else
            {
                if (!PathClear()) dir = -dir;
                UpdateFlip();
                Move(patrolSpeed);
            }
        }
    }

    public void ApplyKnockback(Vector2 velocity)
    {
        if (state == State.Dead) return;

        rb.linearVelocity = velocity;
        knockbackUntil = Time.time + 0.2f;
    }

    // Chamado por um Animation Event no clip Attack, no frame do golpe
    public void DealAttackDamage()
    {
        if (state != State.Attack || player == null || playerHealth == null) return;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        if (dx <= attackRange + 0.2f && dy <= 1.5f)
            playerHealth.TakeDamage(damage, transform.position);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (state == State.Dead) return;

        PlayerHealth p = collision.collider.GetComponentInParent<PlayerHealth>();
        if (p != null)
            p.TakeDamage(contactDamage, transform.position);
    }

        public void TakeDamage(int amount)
    {
        if (state == State.Dead) return;

        health -= amount;

        if (health <= 0)
        {
            if (actionRoutine != null) StopCoroutine(actionRoutine);
            actionRoutine = StartCoroutine(Die());
            return;
        }

        // Armadura durante o ataque e imunidade a stun logo depois de um Hurt
        if (armorWhileAttacking && state == State.Attack) return;
        if (Time.time < nextHurtAllowed) return;

        if (actionRoutine != null) StopCoroutine(actionRoutine);
        actionRoutine = StartCoroutine(Hurt());
    }

    private IEnumerator Hurt()
    {
        state = State.Hurt;
        PlayAnim(hurtAnim, true);
        yield return WaitForAnimation(hurtAnim);

        nextHurtAllowed = Time.time + hurtImmunityTime;
        state = State.Patrol;
        actionRoutine = null;
    }

    private IEnumerator Die()
    {
        state = State.Dead;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        PlayAnim(deathAnim, true);
        yield return WaitForAnimation(deathAnim);

    }

    private IEnumerator Attack()
    {
        state = State.Attack;
        PlayAnim(attackAnim, true);

        yield return WaitForAnimation(attackAnim);

        nextAttackTime = Time.time + attackCooldown;
        state = State.Patrol;
        actionRoutine = null;
    }

    private void Move(float speed)
    {
        if (!PathClear())
        {
            Stop();
            PlayAnim(idleAnim);
            return;
        }

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        PlayAnim(walkAnim);
    }

    private void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // Há chão à frente e nenhuma parede bloqueando?
    private bool PathClear()
    {
        Bounds b = col.bounds;
        float frontX = b.center.x + dir * (b.extents.x + 0.05f);

        bool wall = Physics2D.Raycast(b.center, Vector2.right * dir, b.extents.x + 0.1f, groundLayer);
        bool ground = Physics2D.Raycast(new Vector2(frontX, b.min.y + 0.1f), Vector2.down, 0.4f, groundLayer);

        return !wall && ground;
    }

    private void UpdateFlip()
    {
        sr.flipX = spriteFacesRight ? dir < 0 : dir > 0;
    }

    private void PlayAnim(string anim, bool restart = false)
    {
        if (anim == currentAnim && !restart) return;
        currentAnim = anim;
        animator.Play(anim, 0, 0f);
    }

    private IEnumerator WaitForAnimation(string anim)
    {
        yield return null;

        float timeout = 2f;
        while (timeout > 0f && !animator.GetCurrentAnimatorStateInfo(0).IsName(anim))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
    }
}