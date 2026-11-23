using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LevelFadeIn : MonoBehaviour
{
    [SerializeField] private float holdBlack = 0.5f;
    [SerializeField] private float duration = 2.5f;

    private CanvasGroup g;

    void Awake()
    {
        g = GetComponent<CanvasGroup>();
        g.alpha = 1f;               // preto já no carregamento
    }

    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(holdBlack);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        g.alpha = 0f;
        gameObject.SetActive(false);
    }
}