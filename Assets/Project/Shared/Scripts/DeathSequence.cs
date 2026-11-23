using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSequence : MonoBehaviour
{
    public static DeathSequence Instance { get; private set; }

    [SerializeField] private CanvasGroup root;     // o painel todo (o próprio DeathScreen)
    [SerializeField] private CanvasGroup card;
    [SerializeField] private CanvasGroup title;    // opcional
    [SerializeField] private CanvasGroup prompt;   // "Pressione E para continuar" (opcional)

    [SerializeField] private float fadeTime = 0.8f;
    [SerializeField] private float inputDelay = 1f; // evita pular sem querer logo após a morte

    private bool playing;

    void Awake()
    {
        Instance = this;
        root.alpha = 0f;
        card.alpha = 0f;
        if (title != null) title.alpha = 0f;
        if (prompt != null) prompt.alpha = 0f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Play()
    {
        if (!playing) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        playing = true;
        root.blocksRaycasts = true;

        yield return Fade(root, 0f, 1f);        // escurece a tela
        yield return Fade(card, 0f, 1f);        // aparece a carta
        if (title != null) yield return Fade(title, 0f, 1f);

        // Pausa curta para o jogador não confirmar sem querer
        yield return new WaitForSecondsRealtime(inputDelay);
        if (prompt != null) yield return Fade(prompt, 0f, 1f);

        // Espera o jogador confirmar
        while (!AnyConfirm())
            yield return null;

        SpawnState.NextSpawnId = "";
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool AnyConfirm()
    {
        return GameInput.InteractPressed || GameInput.JumpPressed ||
               GameInput.AttackPressed || GameInput.DashPressed;
    }

    private IEnumerator Fade(CanvasGroup g, float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        g.alpha = to;
    }
}