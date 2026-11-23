using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        var player = FindAnyObjectByType<PlayerInventory>();
        if (player == null) return;

        SpawnPoint target = null;
        foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (sp.id == SpawnState.NextSpawnId) { target = sp; break; }
            if (sp.isDefault && target == null) target = sp;
        }

        if (target != null)
        {
            player.transform.position = target.transform.position;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        SpawnState.NextSpawnId = "";
    }
}