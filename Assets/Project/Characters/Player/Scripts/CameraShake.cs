using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<CinemachineImpulseSource>();
    }

    // Tremor leve: acertar ou levar dano
    public void ShakeLight(float dirX = 0f) => Shake(0.05f, dirX);

    // Tremor forte: matar um inimigo ou morrer
    public void ShakeHeavy(float dirX = 0f) => Shake(0.3f, dirX);

    private void Shake(float force, float dirX)
    {
        // Mais horizontal que vertical; dirX = -1, 0 ou 1 dá o sentido do golpe
        float x = dirX != 0f ? dirX : 1f;
        Vector3 velocity = new Vector3(x, 0.2f, 0f);
        source.GenerateImpulseWithVelocity(velocity * force);
    }
}