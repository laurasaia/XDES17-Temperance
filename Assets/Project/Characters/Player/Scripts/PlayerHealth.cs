using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float hurtDuration = 0.3f;
    [SerializeField] private Vector2 knockback = new Vector2(4f, 5f);
    [SerializeField] private float restartDelay = 1.5f;

    private Rigidbody2D rb;
    private int health;
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

    public void TakeDamage(int amount) => TakeDamage(amount, transform.position);

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (IsDead || Time.time < invulnerableUntil) return;

        health -= amount;
        invulnerableUntil = Time.time + invulnerabilityTime;

        if (health <= 0)
        {
            IsDead = true;
            rb.linearVelocity = Vector2.zero;
            Invoke(nameof(Restart), restartDelay);
            return;
        }

        hurtUntil = Time.time + hurtDuration;

        float dir = transform.position.x >= sourcePosition.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * knockback.x, knockback.y);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}