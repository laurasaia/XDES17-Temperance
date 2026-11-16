using System.Collections;
using UnityEngine;

public class Skeleton : MonoBehaviour, IDamageable, IKnockbackable
{
    private enum State { Patrol, React, Chase, Attack, Hurt, Dead }

    [Header("Vida")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int contactDamage = 1;

    [Header("Movimento")]
    [SerializeField] private float patrolSpeed = 1.5f; // 0 = fica parado até ver o player
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private bool canChase = true;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Combate")]
    [SerializeField] private Transform attackHitbox; // filho com Box Collider 2D (trigger) animado no clip Attack
    [SerializeField] private float sightRange = 7f;
    [SerializeField] private float attackRange = 1.5f; // só decide quando começar o ataque
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Reação ao notar o player")]
    [SerializeField] private bool playReact = true;
    [SerializeField] private float reactCooldown = 5f;

    [Header("Resistência a stun")]
    [SerializeField] private bool armorWhileAttacking = true;
    [SerializeField] private float hurtImmunityTime = 0.8f;

    private const string idleAnim = "Idle";
    private const string walkAnim = "Walk";
    private const string attackAnim = "Attack";
    private const string reactAnim = "React";
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
    private bool wasSeeing;
    private float nextAttackTime;
    private float nextReactTime;
    private float nextHurtAllowed;
    private float knockbackUntil;
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

        if (state == State.Hurt || state == State.Attack || state == State.React)
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
            dir = dx >= 0f ? 1 : -1;
            UpdateFlip();

            // Acabou de notar o player: toca o React antes de perseguir
            if (!wasSeeing && playReact && Time.time >= nextReactTime)
            {
                wasSeeing = true;
                state = State.React; // define já aqui, para não haver frame "livre"
                actionRoutine = StartCoroutine(React());
                return;
            }
            wasSeeing = true;

            state = State.Chase;

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
            wasSeeing = false;
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (state == State.Dead || state == State.Attack || state == State.React) return;

        PlayerHealth p = collision.collider.GetComponentInParent<PlayerHealth>();
        if (p != null && contactDamage > 0) p.TakeDamage(contactDamage, transform.position);
    }

    public void ApplyKnockback(Vector2 velocity)
    {
        if (state == State.Dead) return;

        rb.linearVelocity = velocity;
        knockbackUntil = Time.time + 0.2f;
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

        if (armorWhileAttacking && state == State.Attack) return;
        if (Time.time < nextHurtAllowed) return;

        if (actionRoutine != null) StopCoroutine(actionRoutine);
        actionRoutine = StartCoroutine(Hurt());
    }

    private IEnumerator React()
    {
        state = State.React;
        nextReactTime = Time.time + reactCooldown;
        Stop();
        PlayAnim(reactAnim, true);
        yield return WaitForAnimation(reactAnim);

        // Força o próximo frame a pedir Walk/Idle de novo
        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }

    private IEnumerator Hurt()
    {
        state = State.Hurt;
        DisableHitbox();
        PlayAnim(hurtAnim, true);
        yield return WaitForAnimation(hurtAnim);

        nextHurtAllowed = Time.time + hurtImmunityTime;
        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }

    private IEnumerator Die()
    {
        if (CameraShake.Instance != null) CameraShake.Instance.ShakeHeavy();
        state = State.Dead;
        DisableHitbox();
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        PlayAnim(deathAnim, true);
        yield return WaitForAnimation(deathAnim);

        // O corpo fica no último quadro do Death
        enabled = false;
    }

    private IEnumerator Attack()
    {
        state = State.Attack;
        PlayAnim(attackAnim, true);

        yield return WaitForAnimation(attackAnim);

        DisableHitbox();
        nextAttackTime = Time.time + attackCooldown;
        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }

    // Garante que a hitbox nunca fique ligada se o ataque for interrompido
    private void DisableHitbox()
    {
        if (attackHitbox == null) return;
        BoxCollider2D b = attackHitbox.GetComponent<BoxCollider2D>();
        if (b != null) b.enabled = false;
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

        // Se está andando e o Animator não está em Walk, volta para Walk
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnim))
            PlayAnim(walkAnim, true);
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

    // Parede: terreno OU outro inimigo
    int wallMask = groundLayer | (1 << gameObject.layer);

    RaycastHit2D[] hits = Physics2D.RaycastAll(b.center, Vector2.right * dir, b.extents.x + 0.1f, wallMask);
    bool wall = false;
    foreach (RaycastHit2D h in hits)
    {
        if (h.collider == col) continue;          // ignora o próprio collider
        if (h.collider.isTrigger) continue;       // ignora hitboxes (trigger)
        wall = true;
        break;
    }

    bool ground = Physics2D.Raycast(new Vector2(frontX, b.min.y + 0.1f), Vector2.down, 0.4f, groundLayer);

    return !wall && ground;
}

    private void UpdateFlip()
    {
        sr.flipX = spriteFacesRight ? dir < 0 : dir > 0;

        // Espelha a hitbox pela escala (inverte Offset e Size do collider)
        if (attackHitbox != null)
        {
            float side = spriteFacesRight ? dir : -dir;
            attackHitbox.localScale = new Vector3(side, 1f, 1f);
        }
    }

    private void PlayAnim(string anim, bool restart = false)
    {
        bool alreadyPlaying = animator.GetCurrentAnimatorStateInfo(0).IsName(anim);

        if (alreadyPlaying && anim == currentAnim && !restart) return;

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

        // Limite de segurança: nunca fica preso esperando um clip em loop
        float maxWait = 3f;
        while (maxWait > 0f &&
               animator.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            maxWait -= Time.deltaTime;
            yield return null;
        }
    }
}