using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 8;

    private BoxCollider2D box;
    private bool wasEnabled;
    private bool hitThisSwing;

    void Awake() => box = GetComponent<BoxCollider2D>();

    void Update()
    {
        if (box.enabled && !wasEnabled) hitThisSwing = false;
        wasEnabled = box.enabled;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (hitThisSwing) return;

        PlayerHealth p = other.GetComponentInParent<PlayerHealth>();
        if (p == null) return;

        // Só considera o golpe gasto se o dano realmente entrou
        if (p.TryTakeDamage(damage, transform.position))
            hitThisSwing = true;
    }
}