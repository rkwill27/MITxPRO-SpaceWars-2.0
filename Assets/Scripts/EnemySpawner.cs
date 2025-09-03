using Scripts.Gameplay;
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
    public List<GameObject> activeEnemies = new List<GameObject>();   // ALWAYS track the ROOT enemy object here
    public List<int> currentWaveIndices = new List<int>();

    private int nextWaveIndex = 0;
    public int NextWaveIndex => nextWaveIndex;

    void Update()
    {
        if (gameTimer == null) return;

        // Spawn waves that are ready
        while (nextWaveIndex < spawnWaves.Count)
        {
            SpawnWave wave = spawnWaves[nextWaveIndex];

            if (gameTimer.elapsedTime >= wave.startTime)
            {
                Debug.Log($"[Spawner] Starting wave {nextWaveIndex + 1} ({wave.WaveName}) at {gameTimer.elapsedTime:F1}s");
                StartCoroutine(SpawnWaveCoroutine(nextWaveIndex, wave));

                currentWaveIndices.Add(nextWaveIndex);
                nextWaveIndex++;

                if (!allowWaveOverlap) break;
            }
            else break;
        }

        // Safety cleanup
        int before = activeEnemies.Count;
        activeEnemies.RemoveAll(e => e == null || e.Equals(null));
        if (before != activeEnemies.Count)
            Debug.Log($"[Spawner] Cleaned {before - activeEnemies.Count} null enemies. Remaining: {activeEnemies.Count}");
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

            // Instantiate the ROOT enemy object
            GameObject rootEnemy = Instantiate(selectedPrefab, chosenPoint.position, chosenPoint.rotation);

            // Track the ROOT enemy in both lists
            activeEnemies.Add(rootEnemy);
            wave.spawnedEnemies.Add(rootEnemy);

            // Find EnemyPathing even if it's on a child and assign refs
            EnemyPathing pathing = rootEnemy.GetComponentInChildren<EnemyPathing>(true);
            if (pathing != null)
            {
                pathing.spawner = this;
                pathing.wave = wave;
            }
            else
            {
                Debug.LogWarning($"[Spawner] EnemyPathing not found on {rootEnemy.name} or its children. Removal on death may not call back.");
            }

            Debug.Log($"[Spawner] Spawned {rootEnemy.name} (ID:{rootEnemy.GetInstanceID()}) at {chosenPoint.position} (Wave: {wave.WaveName})");

            // Initialize movement if available
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

        wave.WaveEnded = true;
        currentWaveIndices.Remove(waveIndex);
        Debug.Log($"[Spawner] Wave {waveIndex} ({wave.WaveName}) ended. WaveEnded = {wave.WaveEnded}");
    }

    /// <summary>
    /// Called when an enemy dies. Removes the ROOT enemy object from global and wave lists.
    /// </summary>
    public void RemoveEnemy(GameObject enemyRoot, SpawnWave wave)
    {
        // Make sure we use the root object for tracking/removal
        if (enemyRoot != null)
            enemyRoot = enemyRoot.transform.root.gameObject;

        string enemyLabel = enemyRoot != null ? $"{enemyRoot.name} (ID:{enemyRoot.GetInstanceID()})" : "NULL";
        Debug.Log($"[Spawner.RemoveEnemy] Called for {enemyLabel} | Wave={(wave != null ? wave.WaveName : "NULL")}");

        bool removedFromActive = activeEnemies.Remove(enemyRoot);
        bool removedFromWave = false;

        if (wave != null)
        {
            removedFromWave = wave.spawnedEnemies.Remove(enemyRoot);
            wave.AllEnemiesDestroyed = wave.spawnedEnemies.Count == 0;
        }

        Debug.Log($"[Spawner.RemoveEnemy] removedFromActive={removedFromActive}, removedFromWave={removedFromWave}, " +
                  $"activeLeft={activeEnemies.Count}, inWaveLeft={(wave != null ? wave.spawnedEnemies.Count : 0)}, " +
                  $"WaveDestroyed={(wave != null ? wave.AllEnemiesDestroyed : false)}");
    }
}

[System.Serializable]
public class SpawnWave
{
    [Header("Wave Info")]
    public string WaveName = "Wave";

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
    public bool WaveEnded = false;
    public bool AllEnemiesDestroyed = false;
    public List<GameObject> spawnedEnemies = new List<GameObject>();

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
