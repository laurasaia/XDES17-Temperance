using System.Collections;
using UnityEngine;

public class Mimic : MonoBehaviour, IDamageable, IKnockbackable
{
    private enum State { Closed, Awakening, Patrol, Chase, Bite, Lick, Hurt, Dead }

    [Header("Vida e dano")]
    [SerializeField] private int maxHealth = 70;
    [SerializeField] private int biteDamage = 6;
    [SerializeField] private int contactDamage = 1;

    [Header("Interação (báu fechado)")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private float openPause = 0.5f;
    [SerializeField] private InteractPrompt prompt;

    [Header("Movimento")]
    [SerializeField] private float patrolSpeed = 1f; // 0 = fica parado até ver o player
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private bool canChase = true;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Combate")]
    [SerializeField] private Transform lickHitbox; // filho com Box Collider 2D (trigger) + EnemyAttackHitbox
    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float biteRange = 1.3f;
    [SerializeField] private float lickRange = 3.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float biteLunge = 3f;        // avanço durante a mordida (0 = sem avanço)
    [SerializeField] private float biteLungeTime = 0.25f;
    [SerializeField] private float biteRecovery = 0.8f;   // pausa parado depois da mordida
    [SerializeField] private float lickRecovery = 0.5f;   // pausa parado depois da lambida

    [Header("Resistência a stun")]
    [SerializeField] private bool armorWhileAttacking = true;
    [SerializeField] private float hurtImmunityTime = 1f;

    [Header("Drop")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Vector2 dropOffset = new Vector2(0f, 0.5f);
    [SerializeField] private float dropDelay = 0.8f;                        // espera antes de a chave nascer
    [SerializeField] private Vector2 dropImpulse = new Vector2(0.3f, 2f);

    private const string idleClosedAnim = "IdleC";
    private const string idleOpenAnim = "IdleO";
    private const string idleTransformedAnim = "IdleT";
    private const string openingAnim = "Opening";
    private const string transformAnim = "Transform";
    private const string walkAnim = "Walk";
    private const string biteAnim = "Attack1";
    private const string lickAnim = "Attack2";
    private const string hurtAnim = "Hurt";
    private const string deathAnim = "Death";

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Transform player;
    private PlayerHealth playerHealth;

    private State state = State.Closed;
    private int health;
    private int dir = 1;
    private string currentAnim;
    private bool biteActive; // true só enquanto a mordida pode ferir
    private float nextAttackTime;
    private float nextHurtAllowed;
    private float knockbackUntil;
    private Coroutine actionRoutine;

    private bool IsActive => state != State.Closed && state != State.Awakening && state != State.Dead;

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

        PlayAnim(idleClosedAnim, true);
    }

    void Update()
    {
        // Báu fechado: o player abre com E ou Triangulo
        if (state != State.Closed || player == null) return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist <= interactRange && GameInput.InteractPressed)
            actionRoutine = StartCoroutine(Awaken());
    }

    void FixedUpdate()
    {
        if (state == State.Dead) return;

        // Fechado ou transformando: fica parado
        if (state == State.Closed || state == State.Awakening)
        {
            Stop();
            return;
        }

        if (Time.time < knockbackUntil) return;

        // Durante Hurt, mordida e lambida a própria corrotina controla a velocidade
        if (state == State.Bite)
            return;

        if (state == State.Hurt || state == State.Lick)
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
            state = State.Chase;

            float distX = Mathf.Abs(dx);

            if (distX <= lickRange && Time.time >= nextAttackTime)
            {
                Stop();
                actionRoutine = StartCoroutine(distX <= biteRange ? Bite() : Lick());
            }
            else if (distX <= biteRange)
            {
                Stop();
                PlayAnim(idleTransformedAnim);
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
                PlayAnim(idleTransformedAnim);
            }
            else
            {
                if (!PathClear()) dir = -dir;
                UpdateFlip();
                Move(patrolSpeed);
            }
        }
    }

    // Mordida: o corpo é a hitbox, só fere enquanto biteActive; fora disso, dano de contato fraco
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsActive || state == State.Lick || state == State.Hurt) return;

        PlayerHealth p = collision.collider.GetComponentInParent<PlayerHealth>();
        if (p == null) return;

        if (biteActive) p.TakeDamage(biteDamage, transform.position);
        else if (state != State.Bite && contactDamage > 0) p.TakeDamage(contactDamage, transform.position);
    }

    public void ApplyKnockback(Vector2 velocity)
    {
        if (!IsActive) return;

        rb.linearVelocity = velocity;
        knockbackUntil = Time.time + 0.15f; // mímico pesado: empurrão curto
    }

    public void TakeDamage(int amount)
    {
        // Báu fechado (ou transformando) não recebe dano
        if (!IsActive) return;

        health -= amount;

        if (health <= 0)
        {
            if (actionRoutine != null) StopCoroutine(actionRoutine);
            biteActive = false;
            actionRoutine = StartCoroutine(Die());
            return;
        }

        if (armorWhileAttacking && (state == State.Bite || state == State.Lick)) return;
        if (Time.time < nextHurtAllowed) return;

        if (actionRoutine != null) StopCoroutine(actionRoutine);
        biteActive = false;
        actionRoutine = StartCoroutine(Hurt());
    }

    private IEnumerator Awaken()
    {
        state = State.Awakening;
        if (prompt != null) prompt.Hide();
        
        PlayAnim(openingAnim, true);
        yield return WaitForAnimation(openingAnim);

        PlayAnim(idleOpenAnim, true);
        yield return new WaitForSeconds(openPause);

        PlayAnim(transformAnim, true);
        yield return WaitForAnimation(transformAnim);

        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }
    public void BiteStart()
    {
        if (state != State.Bite) return;
        biteActive = true;

        // Usa o próprio Box Collider do Mimic como hitbox da mordida
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        Collider2D[] hits = new Collider2D[8];
        int count = col.Overlap(filter, hits);

        for (int i = 0; i < count; i++)
        {
            PlayerHealth p = hits[i].GetComponentInParent<PlayerHealth>();
            if (p != null)
            {
                p.TakeDamage(biteDamage, transform.position);
                break;
            }
        }
    }
    // Animation Event no Attack1: frame logo depois de fechar
    public void BiteEnd()
    {
        biteActive = false;
    }

    private IEnumerator Bite()
    {
        state = State.Bite;
        UpdateFlip();
        PlayAnim(biteAnim, true);

        // Avanço curto na direção do player, sem ferir ainda
        biteActive = false;
        float t = 0f;
        while (t < biteLungeTime)
        {
            rb.linearVelocity = new Vector2(dir * biteLunge, rb.linearVelocity.y);
            t += Time.deltaTime;
            yield return null;
        }
        Stop();

        yield return WaitForAnimation(biteAnim);
        biteActive = false;

        // Recuperação: parado de boca fechada antes de voltar a andar
        PlayAnim(idleTransformedAnim, true);
        yield return new WaitForSeconds(biteRecovery);

        nextAttackTime = Time.time + attackCooldown;
        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }

    private IEnumerator Lick()
    {
        state = State.Lick;
        UpdateFlip(); // garante o lado certo da hitbox no momento do golpe
        PlayAnim(lickAnim, true);
        yield return WaitForAnimation(lickAnim);

        DisableLickHitbox();

        PlayAnim(idleTransformedAnim, true);
        yield return new WaitForSeconds(lickRecovery);

        nextAttackTime = Time.time + attackCooldown;
        currentAnim = null;
        state = State.Patrol;
        actionRoutine = null;
    }

    private IEnumerator Hurt()
    {
        state = State.Hurt;
        DisableLickHitbox();
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

        StartCoroutine(DropKey());

        DisableLickHitbox();
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        PlayAnim(deathAnim, true);
        yield return WaitForAnimation(deathAnim);

        // O corpo fica no último quadro do Death
        enabled = false;
    }

    private IEnumerator DropKey()
    {
        yield return new WaitForSeconds(dropDelay);
        if (keyPrefab == null) yield break;

        GameObject key = Instantiate(keyPrefab, (Vector2)transform.position + dropOffset, Quaternion.identity);

        Rigidbody2D krb = key.GetComponent<Rigidbody2D>();
        if (krb != null)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            krb.linearVelocity = new Vector2(side * dropImpulse.x, dropImpulse.y);
        }
    }

    private void DisableLickHitbox()
    {
        if (lickHitbox == null) return;
        BoxCollider2D b = lickHitbox.GetComponent<BoxCollider2D>();
        if (b != null) b.enabled = false;
    }

    private void Move(float speed)
    {
        if (!PathClear())
        {
            Stop();
            PlayAnim(idleTransformedAnim);
            return;
        }

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(walkAnim))
            PlayAnim(walkAnim, true);
    }

    private void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

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
        if (lickHitbox != null)
        {
            float side = spriteFacesRight ? dir : -dir;
            lickHitbox.localScale = new Vector3(side, 1f, 1f);
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

        float maxWait = 4f;
        while (maxWait > 0f &&
               animator.GetCurrentAnimatorStateInfo(0).IsName(anim) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            maxWait -= Time.deltaTime;
            yield return null;
        }
    }
}