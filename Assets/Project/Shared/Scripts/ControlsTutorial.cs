using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsTutorial : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public Sprite[] icons;       // 1 ou mais ícones
        [TextArea] public string text;
        public float duration = 3f;
    }

    // Lembra, enquanto o jogo estiver aberto, que o tutorial já foi mostrado
    private static bool alreadyShown;

    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image[] iconSlots;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Step[] steps;

    [SerializeField] private float startDelay = 4f;   // espera antes de começar
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private float gapBetweenSteps = 0.3f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => alreadyShown = false;
    IEnumerator Start()
    {
        group.alpha = 0f;

        if (alreadyShown)
        {
            gameObject.SetActive(false);
            yield break;
        }

        yield return new WaitForSeconds(startDelay);

        foreach (Step s in steps)
        {
            Show(s);
            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(s.duration);
            yield return Fade(1f, 0f);
            yield return new WaitForSeconds(gapBetweenSteps);
        }

        alreadyShown = true;
        gameObject.SetActive(false);
    }

    private void Show(Step s)
    {
        label.text = s.text;

        for (int i = 0; i < iconSlots.Length; i++)
        {
            bool has = s.icons != null && i < s.icons.Length && s.icons[i] != null;
            iconSlots[i].gameObject.SetActive(has);
            if (has) iconSlots[i].sprite = s.icons[i];
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        group.alpha = to;
    }
}