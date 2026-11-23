using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private string nextScene = "Intro Torre 2";
    [SerializeField] private string spawnIdInNextScene = "";

    [Header("Interação")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private InteractPrompt prompt;

    [Header("Fade de saída (opcional)")]
    [SerializeField] private CanvasGroup fade;      // imagem preta em tela cheia
    [SerializeField] private float fadeTime = 1f;

    [Header("Chave")]
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private Sprite keySprite;        // sprite da chave que desce até a porta
    [SerializeField] private float keyFlyTime = 0.8f;
    [SerializeField] private GameObject unlockEffect; // prefab da animação (opcional)
    [SerializeField] private Vector2 effectOffset = new Vector2(0f, 1f); // altura do destino da chave e do efeito

    [Header("Animação da porta (opcional)")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string unlockAnim = "DoorUnlock";

    private PlayerInventory player;
    private bool used;

    void Start() => player = FindAnyObjectByType<PlayerInventory>();

    void Update()
    {
        if (used || player == null) return;
        if (Vector2.Distance(player.transform.position, transform.position) > interactRange) return;

        if (requiresKey && prompt != null)
            prompt.SetVerb(player.HasKey ? "Abrir" : "Trancada");

        if (!GameInput.InteractPressed) return;

        if (requiresKey && !player.HasKey)
        {
            if (CameraShake.Instance != null) CameraShake.Instance.ShakeLight();
            return;
        }

        if (requiresKey) StartCoroutine(UnlockAndLeave());
        else StartCoroutine(Leave());
    }

    private IEnumerator UnlockAndLeave()
    {
        used = true;
        if (prompt != null) prompt.Hide();

        Camera cam = Camera.main;
        if (keySprite != null && KeyHud.Instance != null && cam != null)
        {
            // Sprite no mundo, começando onde o ícone da HUD está
            GameObject k = new GameObject("KeyFx");
            var sr = k.AddComponent<SpriteRenderer>();
            sr.sprite = keySprite;
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 50;

            Vector3 s = KeyHud.Instance.IconScreenPosition;
            s.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 start = cam.ScreenToWorldPoint(s);
            Vector3 end = transform.position + (Vector3)effectOffset;

            player.UseKey(); // o ícone some da HUD enquanto a chave voa

            float t = 0f;
            while (t < keyFlyTime)
            {
                t += Time.deltaTime;
                float p = t / keyFlyTime;
                p = p * p * (3f - 2f * p);
                k.transform.position = Vector3.Lerp(start, end, p);
                yield return null;
            }

            Destroy(k);
            if (unlockEffect != null) Instantiate(unlockEffect, end, Quaternion.identity);
            if (CameraShake.Instance != null) CameraShake.Instance.ShakeLight();
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            player.UseKey();
            if (unlockEffect != null)
                Instantiate(unlockEffect, transform.position + (Vector3)effectOffset, Quaternion.identity);
        }

        // Animação de destrancar a porta (vale nos dois casos)
        if (doorAnimator != null)
        {
            doorAnimator.Play(unlockAnim, 0, 0f);
            yield return null;
            while (doorAnimator.GetCurrentAnimatorStateInfo(0).IsName(unlockAnim) &&
                   doorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
            yield return new WaitForSeconds(0.3f);
        }

        yield return Leave();
    }

    private IEnumerator Leave()
    {
        used = true;
        if (prompt != null) prompt.Hide();

        if (fade != null)
        {
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                fade.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
                yield return null;
            }
            fade.alpha = 1f;
        }

        SpawnState.NextSpawnId = spawnIdInNextScene;
        SceneManager.LoadScene(nextScene);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        // Ponto onde a chave chega e o efeito nasce
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)effectOffset, 0.15f);
    }
}