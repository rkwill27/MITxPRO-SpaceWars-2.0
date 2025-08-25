using Scripts.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyItemDrop : MonoBehaviour
{
    [Header("Item Drop Settings")]
    public bool shouldDropItem = true;
    [Range(0f, 100f)] public float dropChancePercent = 100f; // 100 for testing
    public GameObject itemToDrop; // Drag your Health Pickup prefab here

    private static bool applicationIsQuitting = false;
    private static bool sceneUnloading = false;

    private void OnDestroy()
    {
        // Avoid spawning items if quitting, unloading, or restarting the scene
        if (applicationIsQuitting || sceneUnloading || GameManager.IsQuittingOrRestarting) return;

        if (shouldDropItem && itemToDrop != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= dropChancePercent)
            {
                // Spawn the pickup slightly above enemy position for visibility
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject pickup = Instantiate(itemToDrop, spawnPos, Quaternion.identity);

                // Use prefab’s original localScale
                pickup.transform.localScale = itemToDrop.transform.localScale;

                // Ensure pickup is visible
                SpriteRenderer sr = pickup.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 10;
                }

                Debug.Log($"[EnemyItemDrop] Dropped {pickup.name} at {spawnPos}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        sceneUnloading = true;
    }
}
