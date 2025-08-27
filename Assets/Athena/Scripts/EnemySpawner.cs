using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Game Timer Reference")]
    public Timer gameTimer; // Drag your timer object here (must have elapsedTime)

    [Header("Spawn Waves")]
    public List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Header("Spawner Settings")]
    public bool allowWaveOverlap = true; // allow multiple waves to fire at once

    private int currentWaveIndex = 0;

    void Update()
    {
        if (gameTimer == null) return;

        // Check waves until we reach one that shouldn't spawn yet
        while (currentWaveIndex < spawnWaves.Count)
        {
            SpawnWave wave = spawnWaves[currentWaveIndex];

            if (gameTimer.elapsedTime >= wave.startTime)
            {
                Debug.Log($"[Spawner] Starting wave {currentWaveIndex + 1} at {gameTimer.elapsedTime:F1}s");
                StartCoroutine(SpawnWaveCoroutine(wave));

                currentWaveIndex++;

                // If overlap disabled only spawn one per frame
                if (!allowWaveOverlap)
                    break;
            }
            else
            {
                // Stop checking once we find a wave not ready yet
                break;
            }
        }
    }

    IEnumerator SpawnWaveCoroutine(SpawnWave wave)
    {
        for (int i = 0; i < wave.quantity; i++)
        {
            if (wave.wavePrefabs.Count == 0 || wave.spawnPoints.Count == 0)
            {
                Debug.LogWarning("[Spawner] Wave skipped: No prefabs or spawn points assigned!");
                yield break;
            }

            // Pick a prefab using frequency weights
            GameObject selectedPrefab = wave.GetRandomPrefab();
            Transform chosenPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Count)];

            GameObject enemy = Instantiate(selectedPrefab, chosenPoint.position, chosenPoint.rotation);
            Debug.Log($"[Spawner] Spawned {enemy.name} at {chosenPoint.position}");

            // Apply pathing data if the prefab has EnemyPathing
            EnemyPathing pathing = enemy.GetComponent<EnemyPathing>();
            if (pathing != null)
            {
                if (wave.patrolPoints != null && wave.patrolPoints.Count > 0)
                {
                    // Copy & shuffle patrol points if requested
                    List<Transform> patrolCopy = new List<Transform>(wave.patrolPoints);

                    if (wave.randomizePatrolPoints)
                    {
                        for (int j = 0; j < patrolCopy.Count; j++)
                        {
                            int randIndex = Random.Range(j, patrolCopy.Count);
                            (patrolCopy[j], patrolCopy[randIndex]) = (patrolCopy[randIndex], patrolCopy[j]);
                        }
                        Debug.Log($"[Spawner] Patrol points randomized for {enemy.name}");
                    }

                    pathing.Initialize(
                        patrolCopy.ToArray(),
                        wave.randomizeAfterFirst,
                        wave.moveSpeed,
                        wave.waitTimeAtPoints,
                        wave.smoothTime
                    );
                    Debug.Log($"[Spawner] Initialized pathing with {patrolCopy.Count} patrol points for {enemy.name}");
                }
                else
                {
                    Debug.LogWarning($"[Spawner] No patrol points assigned for {enemy.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[Spawner] Spawned {enemy.name} but it has no EnemyPathing component!");
            }

            // Small delay between enemies in the same wave
            yield return new WaitForSeconds(wave.spawnDelay);
        }
    }
}

[System.Serializable]
public class SpawnWave
{
    [Header("Timing")]
    public float startTime = 0f;   // When wave starts in seconds
    public int quantity = 3;       // Number of enemies
    public float spawnDelay = 0.5f; // Delay between spawns in the same wave

    [Header("Prefabs with Frequencies")]
    public List<PrefabFrequency> wavePrefabs = new List<PrefabFrequency>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Patrol Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    public bool randomizePatrolPoints = false; // Shuffle order before assigning
    public bool randomizeAfterFirst = false;   // Randomize after reaching first point

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waitTimeAtPoints = 1f;
    public float smoothTime = 0.2f;

    public GameObject GetRandomPrefab()
    {
        if (wavePrefabs.Count == 0) return null;

        int totalWeight = 0;
        foreach (var pf in wavePrefabs) totalWeight += Mathf.Max(1, pf.frequency);

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var pf in wavePrefabs)
        {
            cumulative += Mathf.Max(1, pf.frequency);
            if (roll < cumulative)
                return pf.prefab;
        }
        return wavePrefabs[0].prefab; // fallback
    }
}

[System.Serializable]
public class PrefabFrequency
{
    public GameObject prefab;
    public int frequency = 1; // Higher = more likely
}
