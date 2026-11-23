using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    [SerializeField] private string verb = "Abrir";
    [SerializeField] private float showRange = 1.5f;
    [SerializeField] private Sprite keyboardIcon;   // tecla E
    [SerializeField] private Sprite gamepadIcon;    // Triângulo
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float worldScale = 1.7f; 
    
    private Transform player;
    private bool forcedHidden;

    void Start()
    {
        var p = FindFirstObjectByType<PlayerInventory>();
        if (p != null) player = p.transform;

        label.text = verb;
        group.alpha = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // Mede até o objeto pai (baú, chave, porta), e não até o prompt flutuando acima dele
        Transform anchor = transform.parent != null ? transform.parent : transform;
        float dist = Vector2.Distance(player.position, anchor.position);
        bool show = !forcedHidden && dist <= showRange;

        // Troca o ícone conforme o último dispositivo usado
        bool usingPad = Gamepad.current != null &&
                        (Keyboard.current == null || !Keyboard.current.anyKey.isPressed);
        Sprite wanted = usingPad ? gamepadIcon : keyboardIcon;
        if (wanted != null && icon.sprite != wanted) icon.sprite = wanted;

        float targetAlpha = show ? 1f : 0f;
        group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    // Chamado pelo objeto quando a ação acaba (baú aberto, chave pega)
    public void Hide() => forcedHidden = true;
    public void Show() => forcedHidden = false;

    public void SetVerb(string newVerb)
    {
        verb = newVerb;
        if (label != null) label.text = newVerb;
    }

    void LateUpdate()
    {
        float p = transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f;
        float s = p < 0.0001f ? 1f : worldScale / p;
        transform.localScale = new Vector3(s, s, 1f);
    }
}