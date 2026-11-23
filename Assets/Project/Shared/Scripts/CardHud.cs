using UnityEngine;
using UnityEngine.UI;

public class CardHud : MonoBehaviour
{
    public static CardHud Instance { get; private set; }

    [SerializeField] private Image cardIcon;

    public Vector3 IconScreenPosition => cardIcon.transform.position;

    void Awake()
    {
        Instance = this;
        cardIcon.enabled = false;   
    }

    public void Show() => cardIcon.enabled = true;
}