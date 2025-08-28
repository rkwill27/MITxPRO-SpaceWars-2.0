using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Game Timer Reference")]
    public Timer gameTimer;

    [Header("Spawn Waves")]
    public List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Header("Spawner Settings")]
    public bool allowWaveOverlap = true;

    [Header("Runtime Tracking")]
    public List<GameObject> activeEnemies = new List<GameObject>();
    public List<int> currentWaveIndices = new List<int>();

    private int nextWaveIndex = 0;
    public int NextWaveIndex => nextWaveIndex; // Public getter

    void Update()
    {
        if (gameTimer == null) return;

        // Spawn waves that are ready
        while (nextWaveIndex < spawnWaves.Count)
        {
            SpawnWave wave = spawnWaves[nextWaveIndex];

            if (gameTimer.elapsedTime >= wave.startTime)
            {
                Debug.Log($"[Spawner] Starting wave {nextWaveIndex + 1} at {gameTimer.elapsedTime:F1}s");
                StartCoroutine(SpawnWaveCoroutine(nextWaveIndex, wave));

                currentWaveIndices.Add(nextWaveIndex);
                nextWaveIndex++;

                if (!allowWaveOverlap)
                    break;
            }
            else
            {
                break;
            }
        }

        // Clean destroyed enemies from global list
        activeEnemies.RemoveAll(e => e == null);

        // Update per-wave enemy states
        foreach (int waveIndex in currentWaveIndices)
        {
            SpawnWave wave = spawnWaves[waveIndex];
            if (wave.spawnedEnemies.Count > 0)
            {
                wave.spawnedEnemies.RemoveAll(e => e == null); // Remove destroyed
                wave.AllEnemiesDestroyed = wave.spawnedEnemies.Count == 0;
            }
        }
    }

    IEnumerator SpawnWaveCoroutine(int waveIndex, SpawnWave wave)
    {
        for (int i = 0; i < wave.quantity; i++)
        {
            if (wave.wavePrefabs.Count == 0 || wave.spawnPoints.Count == 0)
            {
                Debug.LogWarning("[Spawner] Wave skipped: No prefabs or spawn points assigned!");
                yield break;
            }

            GameObject selectedPrefab = wave.GetRandomPrefab();
            Transform chosenPoint = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Count)];

            GameObject enemy = Instantiate(selectedPrefab, chosenPoint.position, chosenPoint.rotation);
            activeEnemies.Add(enemy);
            wave.spawnedEnemies.Add(enemy); // Track enemy for this wave
            Debug.Log($"[Spawner] Spawned {enemy.name} at {chosenPoint.position}");

            EnemyPathing pathing = enemy.GetComponent<EnemyPathing>();
            if (pathing != null && wave.patrolPoints.Count > 0)
            {
                List<Transform> patrolCopy = new List<Transform>(wave.patrolPoints);

                if (wave.randomizePatrolPoints)
                {
                    for (int j = 0; j < patrolCopy.Count; j++)
                    {
                        int randIndex = Random.Range(j, patrolCopy.Count);
                        (patrolCopy[j], patrolCopy[randIndex]) = (patrolCopy[randIndex], patrolCopy[j]);
                    }
                }

                pathing.Initialize(
                    patrolCopy.ToArray(),
                    wave.randomizeAfterFirst,
                    wave.moveSpeed,
                    wave.waitTimeAtPoints,
                    wave.smoothTime
                );
            }

            yield return new WaitForSeconds(wave.spawnDelay);
        }

        // Mark wave as ended after all enemies spawned
        wave.WaveEnded = true;
        currentWaveIndices.Remove(waveIndex);
    }
}





[System.Serializable]
public class SpawnWave
{
    [Header("Timing")]
    public float startTime = 0f;
    public int quantity = 3;
    public float spawnDelay = 0.5f;

    [Header("Prefabs with Frequencies")]
    public List<PrefabFrequency> wavePrefabs = new List<PrefabFrequency>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Patrol Points")]
    public List<Transform> patrolPoints = new List<Transform>();
    public bool randomizePatrolPoints = false;
    public bool randomizeAfterFirst = false;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waitTimeAtPoints = 1f;
    public float smoothTime = 0.2f;

    [Header("Wave Status Tracking")]
    public bool WaveEnded = false;            // True when all enemies for this wave have spawned
    public bool AllEnemiesDestroyed = false;  // True when all spawned enemies are destroyed
    [HideInInspector] public List<GameObject> spawnedEnemies = new List<GameObject>();

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
        return wavePrefabs[0].prefab;
    }
}

[System.Serializable]
public class PrefabFrequency
{
    public GameObject prefab;
    public int frequency = 1;
}
