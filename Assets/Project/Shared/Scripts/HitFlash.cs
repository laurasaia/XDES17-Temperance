using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float flashTime = 0.08f;
    [SerializeField] private int flashes = 2;   // quantas piscadas por golpe

    private SpriteRenderer sr;
    private Color original;
    private Coroutine routine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        original = sr.color;
    }

    public void Flash()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        for (int i = 0; i < flashes; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(flashTime);
            sr.color = original;
            yield return new WaitForSeconds(flashTime);
        }
        sr.color = original;
        routine = null;
    }

    void OnDisable()
    {
        if (sr != null) sr.color = original;
    }
}