using UnityEngine;

public class KeyHud : MonoBehaviour
{
    public static KeyHud Instance { get; private set; }

    [SerializeField] private GameObject keyIcon;

    // Posição do ícone na tela (para a chave voar até lá)
    public Vector3 IconScreenPosition => keyIcon.transform.position;

    void Awake() => Instance = this;
    void OnEnable()  => PlayerInventory.OnKeyChanged += Refresh;
    void OnDisable() => PlayerInventory.OnKeyChanged -= Refresh;

    void Start() => SetVisible(false);

    void Refresh(bool hasKey) => SetVisible(hasKey);

    // Esconde só a imagem, mantendo o objeto ativo para sabermos onde ele está
    private void SetVisible(bool v)
    {
        var img = keyIcon.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.enabled = v;
    }
}