using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSequence : MonoBehaviour
{
    [SerializeField] private CanvasGroup title;
    [SerializeField] private CanvasGroup card;
    [SerializeField] private string nextScene = "The Tower";

    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float titleHold = 3f;
    [SerializeField] private float cardHold = 3f;

    IEnumerator Start()
    {
        title.alpha = 0f;
        card.alpha = 0f;

        yield return new WaitForSeconds(0.5f);

        // Título: fade in, fica, e some direto (sem fade out)
        yield return Fade(title, 0f, 1f);
        yield return new WaitForSeconds(titleHold);
        title.alpha = 0f;

        // Carta: fade in, fica, fade out
        yield return Fade(card, 0f, 1f);
        yield return new WaitForSeconds(cardHold);
        yield return Fade(card, 1f, 0f);

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Fade(CanvasGroup g, float from, float to)
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