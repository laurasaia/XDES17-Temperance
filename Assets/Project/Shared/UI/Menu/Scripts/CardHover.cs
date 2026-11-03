using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScaleMultiplier = 1.15f;
    [SerializeField] private float hoverLift = 40f;

    private RectTransform rect;
    private Vector2 originalPos;
    private Vector3 originalScale;
    private bool hovered;

    void Awake()
    {
        rect = (RectTransform)transform;
        originalPos = rect.anchoredPosition;
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enabled || hovered) return;
        hovered = true;

        originalPos = rect.anchoredPosition;
        originalScale = rect.localScale;

        // Só sobe e cresce um pouco. A camada não muda.
        rect.localScale = originalScale * Mathf.Max(1f, hoverScaleMultiplier);
        rect.anchoredPosition = originalPos + Vector2.up * hoverLift;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled || !hovered) return;
        hovered = false;

        rect.localScale = originalScale;
        rect.anchoredPosition = originalPos;
    }
}