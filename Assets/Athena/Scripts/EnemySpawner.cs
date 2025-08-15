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

    [Tooltip("Optional: Spawn points for this wave (leave empty to use global spawn points).")]
    public List<Transform> waveSpawnPoints = new List<Transform>();

    [Tooltip("If true, each spawn will use a random point from the list instead of sequential order.")]
    public bool randomizeSpawnPoints = true;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Global Spawn Points (used if wave has none set)")]
    public List<Transform> globalSpawnPoints = new List<Transform>();

    [Header("Custom Spawn Waves")]
    public List<SpawnWave> spawnWaves = new List<SpawnWave>();

    [Header("Game Timer Reference")]
    public Timer gameTimer;  // Drag your timer object here (must have elapsedTime)

    private HashSet<int> triggeredWaves = new HashSet<int>(); // Keeps track of which waves have already spawned

    void Update()
    {
        if (gameTimer == null)
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
        if (wave.wavePrefabs.Count == 0)
            yield break;

        List<Transform> spawnPointsToUse = wave.waveSpawnPoints.Count > 0 ? wave.waveSpawnPoints : globalSpawnPoints;
        if (spawnPointsToUse.Count == 0)
            yield break;

        // Optionally shuffle if not randomizing per spawn
        if (!wave.randomizeSpawnPoints)
            ShuffleList(spawnPointsToUse);

        for (int i = 0; i < wave.spawnCount; i++)
        {
            Transform chosenPoint;
            if (wave.randomizeSpawnPoints)
            {
                chosenPoint = spawnPointsToUse[Random.Range(0, spawnPointsToUse.Count)];
            }
            else
            {
                chosenPoint = spawnPointsToUse[i % spawnPointsToUse.Count];
            }

            GameObject selectedPrefab = GetWeightedRandomPrefab(wave.wavePrefabs);
            Instantiate(selectedPrefab, chosenPoint.position, chosenPoint.rotation);

            if (i < wave.spawnCount - 1 && wave.spawnDelay > 0f)
                yield return new WaitForSeconds(wave.spawnDelay);
        }
    }

    GameObject GetWeightedRandomPrefab(List<WaveEnemy> enemies)
    {
        int totalWeight = 0;
        foreach (var enemy in enemies)
            totalWeight += Mathf.Max(1, enemy.weight);

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var enemy in enemies)
        {
            cumulativeWeight += Mathf.Max(1, enemy.weight);
            if (randomValue < cumulativeWeight)
                return enemy.prefab;
        }

        return enemies[0].prefab; // fallback
    }

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
