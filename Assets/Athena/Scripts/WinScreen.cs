using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject winScreenUI;

    private bool winScreenShown = false;

    void Start()
    {
        if (winScreenUI != null)
            winScreenUI.SetActive(false);

        if (enemySpawner == null)
            Debug.LogError("[WinScreen] EnemySpawner not assigned!");
    }

    void Update()
    {
        if (winScreenShown || enemySpawner == null)
            return;

        enemySpawner.activeEnemies.RemoveAll(e => e == null);

        // Win condition: all waves spawned and all enemies destroyed
        bool allWavesEnded = true;
        bool allEnemiesDestroyed = true;

        foreach (var wave in enemySpawner.spawnWaves)
        {
            if (!wave.WaveEnded)
                allWavesEnded = false;
            if (!wave.AllEnemiesDestroyed)
                allEnemiesDestroyed = false;
        }

        if (allWavesEnded && allEnemiesDestroyed)
        {
            ShowWinScreen();
        }
    }

    private void ShowWinScreen()
    {
        if (winScreenUI != null)
            winScreenUI.SetActive(true);

        Time.timeScale = 0f;
        winScreenShown = true;
        Debug.Log("[WinScreen] Win screen activated!");
    }
}
