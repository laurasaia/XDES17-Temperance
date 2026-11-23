using System.Collections;
using UnityEngine;

public class ActIntroOverlay : MonoBehaviour
{
    public static ActIntroOverlay Instance { get; private set; }

    [SerializeField] private CanvasGroup root;
    [SerializeField] private CanvasGroup title;
    [SerializeField] private CanvasGroup card;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float titleHold = 3f;
    [SerializeField] private float cardHold = 3f;

    [Header("Música")]
    [SerializeField] private AudioSource music;
    [SerializeField] private float musicFadeTime = 2f;
    [SerializeField] private float musicVolume = 0.4f;

    void Awake()
    {
        Instance = this;
        root.alpha = 0f;
        title.alpha = 0f;
        card.alpha = 0f;
    }

    public IEnumerator Play()
    {
        root.blocksRaycasts = true;
        yield return Fade(root, 0f, 1f);

        yield return Fade(title, 0f, 1f);
        yield return new WaitForSeconds(titleHold);
        title.alpha = 0f;

        yield return Fade(card, 0f, 1f);
        yield return new WaitForSeconds(cardHold);
        yield return Fade(card, 1f, 0f);

        yield return Fade(root, 1f, 0f);
        root.blocksRaycasts = false;

        // Começa a música da fase com fade in
        if (music != null)
        {
            music.volume = 0f;
            music.Play();

            float m = 0f;
            while (m < musicFadeTime)
            {
                m += Time.deltaTime;
                music.volume = Mathf.Lerp(0f, musicVolume, m / musicFadeTime);
                yield return null;
            }
            music.volume = musicVolume;
        }
    }

    private IEnumerator Fade(CanvasGroup g, float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        g.alpha = to;
    }
}