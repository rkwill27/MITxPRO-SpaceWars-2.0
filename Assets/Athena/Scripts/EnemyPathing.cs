using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathing : MonoBehaviour
{
    [Header("Path Settings (assigned by Spawner)")]
    public List<Transform> patrolPoints;
    public bool randomizeAfterFirst = false;

    [Header("Movement Settings (assigned by Spawner)")]
    public float moveSpeed = 3f;
    public float waitTimeAtPoints = 1f;
    public float smoothTime = 0.2f;

    private int currentPoint = 0;
    private List<int> remainingPoints = new List<int>();
    private bool firstPointReached = false;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Vector2 velocitySmoothing;
    private Rigidbody2D rb;

    // ----------------------
    // Item Drop Settings
    // ----------------------
    [System.Serializable]
    public class ItemDrop
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float dropChancePercent = 25f; // Default 25%
    }

    [Header("Pickup Drop Settings")]
    public bool shouldDropItem = true;
    public ItemDrop[] itemsToDrop;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (randomizeAfterFirst && patrolPoints != null && patrolPoints.Count > 1)
        {
            for (int i = 1; i < patrolPoints.Count; i++)
                remainingPoints.Add(i);
        }
    }

    void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;

                if (randomizeAfterFirst && firstPointReached && remainingPoints.Count > 0)
                {
                    int nextIndex = Random.Range(0, remainingPoints.Count);
                    currentPoint = remainingPoints[nextIndex];
                    remainingPoints.RemoveAt(nextIndex);

                    if (remainingPoints.Count == 0)
                    {
                        for (int i = 1; i < patrolPoints.Count; i++)
                            remainingPoints.Add(i);
                    }
                }
                else
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Count;
                }
            }

            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 targetPos = patrolPoints[currentPoint].position;
        float distance = Vector2.Distance(rb.position, targetPos);

        if (distance > 0.1f)
        {
            Vector2 direction = (targetPos - rb.position).normalized;
            Vector2 desiredVelocity = direction * moveSpeed;
            rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref velocitySmoothing, smoothTime);

            if (direction.x != 0)
                transform.localScale = new Vector3(direction.x < 0 ? 1f : -1f, 1f, 1f);
        }
        else
        {
            rb.velocity = Vector2.zero;
            isWaiting = true;
            waitTimer = waitTimeAtPoints;

            if (!firstPointReached)
                firstPointReached = true;
        }
    }

    // ----------------------
    // Public Death Function
    // ----------------------
    public void Die()
    {
        DropItems();
        Destroy(gameObject);
    }

    // ----------------------
    // Item Drop Logic
    // ----------------------
    private void DropItems()
    {
        if (!shouldDropItem || itemsToDrop == null || itemsToDrop.Length == 0)
        {
            Debug.Log($"[EnemyPathing] {gameObject.name}: No items to drop.");
            return;
        }

        Debug.Log($"[EnemyPathing] {gameObject.name}: Attempting to drop items...");

        foreach (ItemDrop drop in itemsToDrop)
        {
            if (drop.itemPrefab == null)
            {
                Debug.LogWarning($"[EnemyPathing] Drop skipped: itemPrefab is null.");
                continue;
            }

            float roll = Random.Range(0f, 100f);
            Debug.Log($"[EnemyPathing] Rolled {roll:F1} for {drop.itemPrefab.name} (Chance {drop.dropChancePercent}%)");

            if (roll <= drop.dropChancePercent)
            {
                GameObject pickup = Instantiate(drop.itemPrefab, transform.position, Quaternion.identity);
                pickup.SetActive(true); // ensure visible
                Debug.Log($"[EnemyPathing] Dropped {drop.itemPrefab.name} at {transform.position}");
            }
            else
            {
                Debug.Log($"[EnemyPathing] {drop.itemPrefab.name} not dropped (roll {roll:F1} > chance {drop.dropChancePercent})");
            }
        }
    }
}
