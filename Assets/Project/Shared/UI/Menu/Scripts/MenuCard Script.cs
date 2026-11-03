using UnityEngine;
using UnityEngine.UI;

public class MenuCard : MonoBehaviour
{
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private string cardName;
    [SerializeField] private float selectedScaleMultiplier = 1.3f;
    [SerializeField] private bool disableOtherCards = true;
    [SerializeField] private GameObject playButton;

    private Image image;
    private Button button;
    private CardHover hover;
    private bool revealed;

    public bool IsRevealed => revealed;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        hover = GetComponent<CardHover>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (revealed) return;
        revealed = true;

        image.sprite = frontSprite;

        if (hover != null) hover.enabled = false;

        transform.SetAsLastSibling();

        RectTransform rect = (RectTransform)transform;
        rect.localScale = rect.localScale * selectedScaleMultiplier;

        if (disableOtherCards) DisableOtherCards();

        if (playButton != null) playButton.SetActive(true);

        Debug.Log("Carta revelada: " + cardName);
    }

    private void DisableOtherCards()
    {
        foreach (Transform sibling in transform.parent)
        {
            if (sibling == transform) continue;

            Button otherButton = sibling.GetComponent<Button>();
            if (otherButton != null) otherButton.interactable = false;

            CardHover otherHover = sibling.GetComponent<CardHover>();
            if (otherHover != null) otherHover.enabled = false;
        }
    }
}