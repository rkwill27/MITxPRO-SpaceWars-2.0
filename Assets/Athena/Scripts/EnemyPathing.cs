using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathing : MonoBehaviour
{
    [Header("Spawner Reference (assigned at spawn)")]
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public SpawnWave wave;

    [Header("Path Settings")]
    public Transform[] patrolPoints;
    private int currentPoint = 0;
    private List<int> remainingPoints = new List<int>();
    private bool firstPointReached = false;
    public bool randomizeAfterFirst = false;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waitTimeAtPoints = 1f;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    public float smoothTime = 0.2f;
    private Vector2 velocitySmoothing;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireInterval = 2f;
    private float fireTimer = 0f;

    [System.Serializable]
    public class ItemDrop
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float dropChancePercent;
    }

    [Header("Pickups")]
    public bool shouldDropItem;
    public ItemDrop[] itemsToDrop;

    private Rigidbody2D rb;
    private static bool applicationIsQuitting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        HandleMovement();
        HandleFiring();
    }

    public void Initialize(Transform[] points, bool randomize, float speed, float waitTime, float smooth)
    {
        patrolPoints = points;
        randomizeAfterFirst = randomize;
        moveSpeed = speed;
        waitTimeAtPoints = waitTime;
        smoothTime = smooth;

        currentPoint = 0;
        remainingPoints.Clear();
        firstPointReached = false;
        isWaiting = false;

        if (randomizeAfterFirst && patrolPoints.Length > 1)
        {
            for (int i = 1; i < patrolPoints.Length; i++)
                remainingPoints.Add(i);
        }
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
                        for (int i = 1; i < patrolPoints.Length; i++)
                            remainingPoints.Add(i);
                    }
                }
                else
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
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

    void HandleFiring()
    {
        fireTimer -= Time.fixedDeltaTime;
        if (fireTimer <= 0f && projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            fireTimer = fireInterval;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (applicationIsQuitting || !Application.isPlaying) return;

        // Cleanup in spawner
        if (spawner != null && wave != null)
        {
            spawner.RemoveEnemy(gameObject, wave);
            Debug.Log($"[EnemyPathing] {gameObject.name} destroyed and removed from wave.");
        }

        // Handle item drops
        if (shouldDropItem && itemsToDrop.Length > 0)
        {
            foreach (ItemDrop drop in itemsToDrop)
            {
                if (drop.itemPrefab == null) continue;
                float roll = Random.Range(0f, 100f);
                if (roll <= drop.dropChancePercent)
                {
                    Instantiate(drop.itemPrefab, transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
}
