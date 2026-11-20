using System.Collections;
using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private float flyTime = 0.6f;
    [SerializeField] private float pickupDelay = 0.5f;
    [SerializeField] private LayerMask groundLayer;   
    [SerializeField] private string idle = "Idle";
    [SerializeField] private InteractPrompt prompt;

    private PlayerInventory player;
    private Animator animator;
    private Collider2D solidCol;
    private bool collected;
    private bool landed;
    private float spawnTime;

    void Awake() => spawnTime = Time.time;

    void Start()
    {
        player = FindFirstObjectByType<PlayerInventory>();
        solidCol = GetComponent<Collider2D>();

        animator = GetComponent<Animator>();
        if (animator != null) animator.speed = 0f; // parada no 1º quadro até cair
    }

    void Update()
    {
        CheckLanding();

        if (collected || player == null) return;
        if (Time.time - spawnTime < pickupDelay) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > pickupRange) return;

        if (GameInput.InteractPressed)
            StartCoroutine(FlyToHud(player));
    }

    // Quando encosta no chão, começa o Idle
    private void CheckLanding()
    {
        if (landed || animator == null || solidCol == null) return;
        if (Time.time - spawnTime < 0.15f) return; // ignora o instante do pulo

        if (solidCol.IsTouchingLayers(groundLayer))
        {
            landed = true;
            animator.speed = 1f;
            animator.Play(idle, 0, 0f);
        }
    }

    private IEnumerator FlyToHud(PlayerInventory inv)
    {
        collected = true;
        if (prompt != null) prompt.Hide();

        foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        Camera cam = Camera.main;
        Vector3 start = transform.position;

        float t = 0f;
        while (t < flyTime)
        {
            t += Time.deltaTime;
            float k = t / flyTime;
            k = k * k * (3f - 2f * k);

            Vector3 target = start;
            if (KeyHud.Instance != null && cam != null)
            {
                Vector3 screen = KeyHud.Instance.IconScreenPosition;
                screen.z = Mathf.Abs(cam.transform.position.z - start.z);
                target = cam.ScreenToWorldPoint(screen);
            }

            transform.position = Vector3.Lerp(start, target, k);
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, k);
            yield return null;
        }

        inv.GiveKey();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}