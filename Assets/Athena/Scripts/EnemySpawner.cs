using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn (Random Selection)")]
    public List<GameObject> prefabsToSpawn;

    [Header("Spawn Points (Do Not Destroy)")]
    public List<Transform> spawnPoints;

    [Header("Spawning Settings")]
    public int spawnCount = 3;           // Number of enemies to spawn at once
    public float spawnInterval = 5f;     // Time between spawn waves

    [Header("Game Timer Reference")]
    public Timer gameTimer;              // Drag your timer object here (must have elapsedTime)

    private float nextSpawnTime = 0f;

    void Update()
    {
        if (gameTimer == null || prefabsToSpawn.Count == 0 || spawnPoints.Count == 0)
            return;

        if (gameTimer.elapsedTime >= nextSpawnTime)
        {
            SpawnPrefabs();
            nextSpawnTime = gameTimer.elapsedTime + spawnInterval;
        }
    }

    void SpawnPrefabs()
    {
        // Make sure we don't try to use more spawn points than exist
        int actualSpawnCount = Mathf.Min(spawnCount, spawnPoints.Count);

        // Shuffle the spawn points list to get unique random positions
        List<Transform> shuffledPoints = new List<Transform>(spawnPoints);
        ShuffleList(shuffledPoints);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            Transform spawnPoint = shuffledPoints[i];

            // Randomly select a prefab to spawn
            GameObject randomPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];

            // Spawn it at the chosen spawn point
            Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);
        }
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
