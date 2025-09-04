using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour
{
    [Header("Spawner References (assigned at spawn)")]
    [HideInInspector] public EnemySpawner spawner;
    [HideInInspector] public SpawnWave wave;

    [Header("Pathing Settings")]
    private Transform[] patrolPoints;
    private int currentPointIndex = 0;
    private bool randomizeAfterFirst;
    private float moveSpeed;
    private float waitTimeAtPoints;
    private float smoothTime;

    private Vector3 velocity = Vector3.zero;
    private bool isWaiting = false;

    [Header("Lifetime")]
    [Tooltip("If > 0, despawn (Destroy root) after this many seconds.")]
    public float lifetimeSeconds = 0f;

    /* [Header("Firing Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f;
    private float fireCooldown = 0f; */

    private void OnEnable()
    {
        // Despawn after a lifetime, if set. Destroy the ROOT so spawner.RemoveEnemy runs.
        if (lifetimeSeconds > 0f)
            Destroy(transform.root.gameObject, lifetimeSeconds);
    }

    private void Update()
    {
        HandleMovement();
        //HandleFiring();
    }

    private void HandleMovement()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || isWaiting) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        if (targetPoint == null) return;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPoint.position,
            ref velocity,
            smoothTime,
            moveSpeed
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            StartCoroutine(WaitAndMoveNext());
        }
    }

    private IEnumerator WaitAndMoveNext()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtPoints);

        if (randomizeAfterFirst)
            currentPointIndex = Random.Range(0, patrolPoints.Length);
        else
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

        isWaiting = false;
    }

    /* private void HandleFiring()
    {
        if (bulletPrefab == null || firePoint == null) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            fireCooldown = fireRate;
        }
    } */

    public void Initialize(
        Transform[] patrolPoints,
        bool randomizeAfterFirst,
        float moveSpeed,
        float waitTimeAtPoints,
        float smoothTime
    )
    {
        this.patrolPoints = patrolPoints;
        this.randomizeAfterFirst = randomizeAfterFirst;
        this.moveSpeed = moveSpeed;
        this.waitTimeAtPoints = waitTimeAtPoints;
        this.smoothTime = smoothTime;

        if (patrolPoints != null && patrolPoints.Length > 0)
            currentPointIndex = 0;
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;

        // Use the ROOT object for removal
        GameObject root = gameObject != null ? gameObject.transform.root.gameObject : null;

        if (spawner != null && wave != null && root != null)
        {
            Debug.Log($"[EnemyPathing.OnDestroy] Destroyed {root.name} (ID:{root.GetInstanceID()}) | Wave:{wave.WaveName} | Removing from spawner");
            spawner.RemoveEnemy(root, wave);
        }
        /* else
        {
            Debug.LogWarning($"[EnemyPathing.OnDestroy] Missing references or root. spawnerNull:{spawner == null}, waveNull:{wave == null}, rootNull:{root == null}");
        } */
    }
}
