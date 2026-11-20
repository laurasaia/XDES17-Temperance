using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsTutorial : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        public Sprite[] icons;       // 1 ou mais ícones (ex.: analógico e direcional)
        [TextArea] public string text;
        public float duration = 3f;
    }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image[] iconSlots;   // 2 Images lado a lado (usa quantas precisar)
    [SerializeField] private TMP_Text label;
    [SerializeField] private Step[] steps;

    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private float gapBetweenSteps = 0.3f;

    IEnumerator Start()
    {
        group.alpha = 0f;
        yield return new WaitForSeconds(startDelay);

        foreach (Step s in steps)
        {
            Show(s);
            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(s.duration);
            yield return Fade(1f, 0f);
            yield return new WaitForSeconds(gapBetweenSteps);
        }

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