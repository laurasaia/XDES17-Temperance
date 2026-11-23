using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ResetOrb : MonoBehaviour
{
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private bool resetDash = true;
    [SerializeField] private bool resetJump = true;

    [Header("Animações (estados do Animator)")]
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string breakAnim = "Break";   // deixe vazio se não existir
    [SerializeField] private string respawnAnim = "";      // opcional

    [Header("Luz")]
    [SerializeField] private Light2D orbLight;              // arraste o Light 2D do orbe
    [SerializeField] private float lightFadeTime = 0.3f;

    private Animator animator;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool active = true;
    private float lightIntensity = 1f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (orbLight == null) orbLight = GetComponentInChildren<Light2D>();
        if (orbLight != null) lightIntensity = orbLight.intensity;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        // Só o corpo do Player conta (a hitbox de ataque é trigger)
        if (other.isTrigger) return;

        var inv = other.GetComponentInParent<PlayerInventory>();
        if (inv == null) return;

        var root = inv.transform;
        var dash = root.GetComponent<PlayerDash>();
        var jump = root.GetComponent<PlayerJump>();

        bool used = false;
        if (resetDash && dash != null) { dash.ResetAirDash(); used = true; }
        if (resetJump && jump != null) { jump.GrantExtraJump(); used = true; }

        if (used) StartCoroutine(BreakAndRespawn());
    }

    private IEnumerator BreakAndRespawn()
    {
        active = false;
        col.enabled = false;
        StartCoroutine(FadeLight(0f));

        if (animator != null && !string.IsNullOrEmpty(breakAnim))
        {
            animator.Play(breakAnim, 0, 0f);
            yield return null;
            while (animator.GetCurrentAnimatorStateInfo(0).IsName(breakAnim) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        sr.enabled = false; // some durante o cooldown
        yield return new WaitForSeconds(cooldown);

        sr.enabled = true;
        StartCoroutine(FadeLight(lightIntensity));

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(respawnAnim))
            {
                animator.Play(respawnAnim, 0, 0f);
                yield return null;
                while (animator.GetCurrentAnimatorStateInfo(0).IsName(respawnAnim) &&
                       animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                    yield return null;
            }

            animator.Play(idleAnim, 0, 0f); // volta para o Idle
        }

        col.enabled = true;
        active = true;
    }

    private IEnumerator FadeLight(float to)
    {
        if (orbLight == null) yield break;

        float from = orbLight.intensity;
        float t = 0f;
        while (t < lightFadeTime)
        {
            t += Time.deltaTime;
            orbLight.intensity = Mathf.Lerp(from, to, t / lightFadeTime);
            yield return null;
        }
        orbLight.intensity = to;
    }
}