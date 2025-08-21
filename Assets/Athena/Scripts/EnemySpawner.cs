using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Game Timer Reference")]
    public Timer gameTimer; // Drag your timer object here (must have elapsedTime)

    [Header("Spawn Waves")]
    public List<SpawnWave> spawnWaves = new List<SpawnWave>();

    private int currentWaveIndex = 0;
    private bool spawning = false;

    void Update()
    {
        if (gameTimer == null || spawning) return;

        if (currentWaveIndex < spawnWaves.Count)
        {
            SpawnWave wave = spawnWaves[currentWaveIndex];
            if (gameTimer.elapsedTime >= wave.startTime)
            {
                Debug.Log($"[Spawner] Starting wave {currentWaveIndex + 1} at {gameTimer.elapsedTime:F1}s");
                StartCoroutine(SpawnWaveCoroutine(wave));
                currentWaveIndex++;
            }
        }
    }

    IEnumerator SpawnWaveCoroutine(SpawnWave wave)
    {
        spawning = true;

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
                    // FIX: Convert List<Transform> to Transform[] for EnemyPathing
                    pathing.patrolPoints = wave.patrolPoints.ToArray();
                    Debug.Log($"[Spawner] Assigned {pathing.patrolPoints.Length} patrol points to {enemy.name}");
                }
                else
                {
                    Debug.LogWarning($"[Spawner] No patrol points assigned for {enemy.name}");
                }

                // Transfer movement settings
                pathing.randomizeAfterFirst = wave.randomizeAfterFirst;
                pathing.moveSpeed = wave.moveSpeed;
                pathing.waitTimeAtPoints = wave.waitTimeAtPoints;
                pathing.smoothTime = wave.smoothTime;
            }
            else
            {
                Debug.LogWarning($"[Spawner] Spawned {enemy.name} but it has no EnemyPathing component!");
            }

            yield return new WaitForSeconds(wave.spawnDelay);
        }

        spawning = false;
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
    public bool randomizeAfterFirst = false;

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
