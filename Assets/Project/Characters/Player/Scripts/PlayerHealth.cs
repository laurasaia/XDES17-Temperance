using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private Vector2 knockback = new Vector2(4f, 5f);
    [SerializeField] private float restartDelay = 1.5f;

    private Rigidbody2D rb;
    private int health;
    private int lastDamage;
    private float invulnerableUntil;
    private float hurtUntil;

    public int Health => health;
    public int MaxHealth => maxHealth;
    public bool IsHurt => Time.time < hurtUntil;
    public bool IsDead { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
    }

    void Update()
    {
        // Terminou a janela de invulnerabilidade: o próximo dano conta normalmente
        if (Time.time >= invulnerableUntil) lastDamage = 0;
    }

    public void TakeDamage(int amount) => TryTakeDamage(amount, transform.position);

    public void TakeDamage(int amount, Vector2 sourcePosition) => TryTakeDamage(amount, sourcePosition);

    // Devolve true se o dano realmente entrou (quem ataca usa isso para saber se o golpe foi gasto)
    public bool TryTakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsDead) return false;

        // Durante a invulnerabilidade, só passa dano MAIOR que o último recebido
        // (assim o contato fraco não "come" o golpe forte da arma)
        if (Time.time < invulnerableUntil && amount <= lastDamage) return false;

        lastDamage = amount;
        health -= amount;
        invulnerableUntil = Time.time + invulnerabilityTime;

        Debug.Log($"Player levou {amount} de dano. Vida: {health}");

        float dirX = transform.position.x >= sourcePosition.x ? 1f : -1f;

        if (health <= 0)
        {
            IsDead = true;
            rb.linearVelocity = Vector2.zero;

            if (CameraShake.Instance != null) CameraShake.Instance.ShakeHeavy(dirX);

            Invoke(nameof(Restart), restartDelay);
            return true;
        }

        if (CameraShake.Instance != null) CameraShake.Instance.ShakeLight(dirX);

        hurtUntil = Time.time + hurtDuration;
        rb.linearVelocity = new Vector2(dirX * knockback.x, knockback.y);
        return true;
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}