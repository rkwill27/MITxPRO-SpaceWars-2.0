using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveEnemy
{
    [Tooltip("The enemy prefab to spawn.")]
    public GameObject prefab;

    [Tooltip("Relative frequency/weight this enemy will be chosen. Higher = more likely.")]
    public int weight = 1;
}

[System.Serializable]
public class SpawnWave
{
    [Tooltip("Time (in seconds) since the start of the game when this wave should spawn.")]
    public float triggerTime;

    [Tooltip("Number of objects to spawn in this wave.")]
    public int spawnCount = 3;

    [Tooltip("Delay between each individual spawn in this wave.")]
    public float spawnDelay = 0.2f;

    [Tooltip("Enemies that can spawn in this wave, with spawn frequency weights.")]
    public List<WaveEnemy> wavePrefabs = new List<WaveEnemy>();
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Points (Do Not Destroy)")]
    public List<Transform> spawnPoints;

    [Header("Custom Spawn Waves")]
    public List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Header("Game Timer Reference")]
    public Timer gameTimer;  // Drag your timer object here (must have elapsedTime)

    private HashSet<int> triggeredWaves = new HashSet<int>(); // Keeps track of which waves have already spawned

    void Update()
    {
        if (gameTimer == null || spawnPoints.Count == 0)
            return;

        float currentTime = gameTimer.elapsedTime;

        for (int i = 0; i < spawnWaves.Count; i++)
        {
            if (!triggeredWaves.Contains(i) && currentTime >= spawnWaves[i].triggerTime)
            {
                StartCoroutine(SpawnWaveCoroutine(spawnWaves[i]));
                triggeredWaves.Add(i);
            }
        }
    }

    IEnumerator SpawnWaveCoroutine(SpawnWave wave)
    {
        // If no prefabs for this wave, skip
        if (wave.wavePrefabs.Count == 0)
            yield break;

        // Make sure we don't try to use more spawn points than exist
        int actualSpawnCount = Mathf.Min(wave.spawnCount, spawnPoints.Count);

        // Shuffle the spawn points list to get unique random positions
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        ShuffleList(shuffledPoints);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            Transform spawnPoint = shuffledPoints[i];

            // Randomly select a prefab based on weight
            GameObject selectedPrefab = GetWeightedRandomPrefab(wave.wavePrefabs);

            // Spawn it at the chosen spawn point
            Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);

            // Optional delay before spawning the next object
            if (i < actualSpawnCount - 1 && wave.spawnDelay > 0f)
                yield return new WaitForSeconds(wave.spawnDelay);
        }
    }

    GameObject GetWeightedRandomPrefab(List<WaveEnemy> enemies)
    {
        int totalWeight = 0;
        foreach (var enemy in enemies)
            totalWeight += Mathf.Max(1, enemy.weight); // Ensure weight is at least 1

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var enemy in enemies)
        {
            cumulativeWeight += Mathf.Max(1, enemy.weight);
            if (randomValue < cumulativeWeight)
                return enemy.prefab;
        }

        // Fallback (should never hit)
        return enemies[0].prefab;
    }

    // Fisher-Yates Shuffle to randomize spawn points without duplicates
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
