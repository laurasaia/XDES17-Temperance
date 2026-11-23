using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string id = "default";
    public bool isDefault = false;   // usado quando nenhum id foi pedido

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}