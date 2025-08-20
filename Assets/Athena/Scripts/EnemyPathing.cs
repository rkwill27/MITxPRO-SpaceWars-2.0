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

    void Start()
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
}
