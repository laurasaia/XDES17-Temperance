using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Vector2 Current { get; private set; }
    public static bool HasCheckpoint { get; private set; }

    [SerializeField] private bool setOnStart = false;   // marque no ponto inicial do parkour
    [SerializeField] private Vector2 respawnOffset = Vector2.zero;

    void Start()
    {
        if (setOnStart) Set();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInParent<PlayerInventory>() != null) Set();
    }

    private void Set()
    {
        Current = (Vector2)transform.position + respawnOffset;
        HasCheckpoint = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + respawnOffset, 0.3f);
    }
}