using System.Collections;
using UnityEngine;

public class FallZone : MonoBehaviour
{
    [SerializeField] private CanvasGroup fade;          // opcional: imagem preta (Alpha 0)
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private int damage = 0;            // 0 = sem dano

    private bool busy;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (busy || other.isTrigger) return;
        var inv = other.GetComponentInParent<PlayerInventory>();
        if (inv == null || !Checkpoint.HasCheckpoint) return;

        StartCoroutine(Respawn(inv.transform));
    }

    private IEnumerator Respawn(Transform player)
    {
        busy = true;

        if (fade != null) yield return Fade(0f, 1f);

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        player.position = Checkpoint.Current;

        if (damage > 0)
        {
            var hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(damage);
        }

        if (fade != null) yield return Fade(1f, 0f);
        busy = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        fade.alpha = to;
    }
}