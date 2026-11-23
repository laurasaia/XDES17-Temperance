using System.Collections;
using UnityEngine;

public class StarCardPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private float flyTime = 0.8f;
    [SerializeField] private InteractPrompt prompt;
    [SerializeField] private float holdAfterPickup = 1f;

    [Header("Flutuar")]
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 2f;

    private PlayerInventory player;
    private bool collected;
    private Vector3 basePos;

    void Start()
    {
        player = FindAnyObjectByType<PlayerInventory>();
        basePos = transform.position;
    }

    void Update()
    {
        if (collected) return;

        transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);

        if (player == null) return;
        if (Vector2.Distance(player.transform.position, basePos) > pickupRange) return;

        if (GameInput.InteractPressed)
            StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        collected = true;
        if (prompt != null) prompt.Hide();

        Camera cam = Camera.main;
        Vector3 start = transform.position;

        float t = 0f;
        while (t < flyTime)
        {
            t += Time.deltaTime;
            float k = t / flyTime;
            k = k * k * (3f - 2f * k);

            Vector3 target = start;
            if (CardHud.Instance != null && cam != null)
            {
                Vector3 screen = CardHud.Instance.IconScreenPosition;
                screen.z = Mathf.Abs(cam.transform.position.z - start.z);
                target = cam.ScreenToWorldPoint(screen);
            }

            transform.position = Vector3.Lerp(start, target, k);
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, k);
            yield return null;
        }

        if (CardHud.Instance != null) CardHud.Instance.Show();
        if (CameraShake.Instance != null) CameraShake.Instance.ShakeLight();
        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(holdAfterPickup);

        // Mostra "Ato 2, A Lua" por cima da fase, sem recarregar a cena
        if (ActIntroOverlay.Instance != null)
            yield return ActIntroOverlay.Instance.Play();

        gameObject.SetActive(false);
    }
}