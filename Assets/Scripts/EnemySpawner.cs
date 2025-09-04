using Scripts.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Game Timer Reference")] public Timer gameTimer;
    [Header("Spawn Waves")] public List<SpawnWave> spawnWaves = new();
    [Header("Spawner Settings")] public bool allowWaveOverlap = true;
    [Header("Runtime Tracking")]
    public List<GameObject> activeEnemies = new();
    public List<int> currentWaveIndices = new();

    int nextWaveIndex = 0;
    public int NextWaveIndex => nextWaveIndex;

    // Tracks which prefab entry (and its drop table) each spawned enemy came from
    readonly Dictionary<int, PrefabFrequency> enemyMeta = new();

    void Update()
    {
        if (!gameTimer) return;

        while (nextWaveIndex < spawnWaves.Count)
        {
            var wave = spawnWaves[nextWaveIndex];
            if (gameTimer.elapsedTime < wave.startTime) break;

            StartCoroutine(SpawnWaveCoroutine(nextWaveIndex, wave));
            currentWaveIndices.Add(nextWaveIndex++);
            if (!allowWaveOverlap) break;
        }

        // Cleanup nulls and stale meta
        int before = activeEnemies.Count;
        activeEnemies.RemoveAll(e => e == null || e.Equals(null));
        if (before != activeEnemies.Count) Debug.Log($"[Spawner] Cleaned {before - activeEnemies.Count} null enemies.");
    }

    IEnumerator SpawnWaveCoroutine(int waveIndex, SpawnWave wave)
    {
        for (int i = 0; i < wave.quantity; i++)
        {
            if (wave.wavePrefabs.Count == 0 || wave.spawnPoints.Count == 0)
            { Debug.LogWarning("[Spawner] Wave skipped: Missing prefabs/spawn points."); yield break; }

            var pf = wave.GetRandomPrefabEntry();                        // which enemy type (and its drop table)
            var point = wave.spawnPoints[Random.Range(0, wave.spawnPoints.Count)];
            var rootEnemy = Instantiate(pf.prefab, point.position, point.rotation);

            activeEnemies.Add(rootEnemy);
            wave.spawnedEnemies.Add(rootEnemy);
            enemyMeta[rootEnemy.GetInstanceID()] = pf;                   // remember drop table for this enemy

            var path = rootEnemy.GetComponentInChildren<EnemyPathing>(true);
            if (path)
            {
                path.spawner = this; path.wave = wave;
                if (wave.patrolPoints.Count > 0)
                {
                    var patrol = new List<Transform>(wave.patrolPoints);
                    if (wave.randomizePatrolPoints)
                        for (int j = 0; j < patrol.Count; j++) { int r = Random.Range(j, patrol.Count); (patrol[j], patrol[r]) = (patrol[r], patrol[j]); }
                    path.Initialize(patrol.ToArray(), wave.randomizeAfterFirst, wave.moveSpeed, wave.waitTimeAtPoints, wave.smoothTime);
                }
            }

            yield return new WaitForSeconds(wave.spawnDelay);
        }

        wave.WaveEnded = true; currentWaveIndices.Remove(waveIndex);
    }

    /// Call this from enemy death (EnemyPathing/health script already knows the spawner+wave).
    public void RemoveEnemy(GameObject enemyRoot, SpawnWave wave)
    {
        if (enemyRoot) enemyRoot = enemyRoot.transform.root.gameObject;
        int id = enemyRoot ? enemyRoot.GetInstanceID() : -1;

        activeEnemies.Remove(enemyRoot);
        if (wave != null) { wave.spawnedEnemies.Remove(enemyRoot); wave.AllEnemiesDestroyed = wave.spawnedEnemies.Count == 0; }

        // --- Drop logic owned by the spawner ---
        if (enemyRoot && enemyMeta.TryGetValue(id, out var pf))
        {
            TryDrop(enemyRoot.transform.position, pf);
            enemyMeta.Remove(id);
        }
    }

    void TryDrop(Vector3 pos, PrefabFrequency pf)
    {
        if (pf == null || !HealthPickup.CanSpawnPickups) return;        // protects during quit/reload
        GameObject drop = pf.GetRandomDrop();
        if (!drop) return;
        HealthPickup.TrySpawn(drop, pos + Vector3.up * 0.5f, Quaternion.identity);
    }
}

[System.Serializable]
public class SpawnWave
{
    [Header("Wave Info")] public string WaveName = "Wave";
    [Header("Timing")] public float startTime = 0f; public int quantity = 3; public float spawnDelay = 0.5f;
    [Header("Prefabs with Frequencies")] public List<PrefabFrequency> wavePrefabs = new();
    [Header("Spawn Points")] public List<Transform> spawnPoints = new();
    [Header("Patrol Points")] public List<Transform> patrolPoints = new();
    public bool randomizePatrolPoints = false, randomizeAfterFirst = false;
    [Header("Movement Settings")] public float moveSpeed = 3f, waitTimeAtPoints = 1f, smoothTime = 0.2f;
    [Header("Wave Status Tracking")] public bool WaveEnded = false, AllEnemiesDestroyed = false;
    public List<GameObject> spawnedEnemies = new();

    public PrefabFrequency GetRandomPrefabEntry()
    {
        if (wavePrefabs.Count == 0) return null;
        int total = 0; foreach (var pf in wavePrefabs) total += Mathf.Max(1, pf.frequency);
        int roll = Random.Range(0, total), cum = 0;
        foreach (var pf in wavePrefabs) { cum += Mathf.Max(1, pf.frequency); if (roll < cum) return pf; }
        return wavePrefabs[0];
    }
}

[System.Serializable]
public class PrefabFrequency
{
    [Header("Enemy Prefab + Weight")] public GameObject prefab; public int frequency = 1;

    [Header("Drop Items (weighted)")]
    [Tooltip("Weighted list of items this enemy type can drop.")]
    public List<DropItem> dropItems = new();

    [Tooltip("Additional weight for dropping nothing. Set >0 to introduce 'no drop' chance.")]
    public int noDropWeight = 0;

    public GameObject GetRandomDrop()
    {
        int total = Mathf.Max(0, noDropWeight);
        for (int i = 0; i < dropItems.Count; i++) total += Mathf.Max(1, dropItems[i].frequency);
        if (total <= 0) return null;

        int roll = Random.Range(0, total);
        if (roll < noDropWeight) return null;
        roll -= noDropWeight;

        for (int i = 0; i < dropItems.Count; i++)
        {
            int w = Mathf.Max(1, dropItems[i].frequency);
            if (roll < w) return dropItems[i].prefab;
            roll -= w;
        }
        return null;
    }
}

[System.Serializable]
public class DropItem
{
    public GameObject prefab;
    public int frequency = 1;
}
