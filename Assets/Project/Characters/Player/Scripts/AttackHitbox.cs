using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damage = 4;
    [SerializeField] private Vector2 knockback = new Vector2(2f, 2f);

    private readonly HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();
    private BoxCollider2D box;
    private bool wasEnabled;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Limpa a lista cada vez que o collider liga de novo (cada golpe pode acertar uma vez)
        if (box.enabled && !wasEnabled) alreadyHit.Clear();
        wasEnabled = box.enabled;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;
        if (!alreadyHit.Add(other)) return;

        other.GetComponent<IDamageable>()?.TakeDamage(damage);

        // Empurra para longe do Player
        float dir = other.transform.position.x >= transform.position.x ? 1f : -1f;
        other.GetComponent<Slime>()?.ApplyKnockback(new Vector2(dir * knockback.x, knockback.y));
    }
}