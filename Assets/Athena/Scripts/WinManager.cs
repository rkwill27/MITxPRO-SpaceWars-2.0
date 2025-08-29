using UnityEngine;

public class WinManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject winScreenUI;

    private bool winScreenShown = false;

    void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("[WinManager] EnemySpawner not assigned!");
            return;
        }

        if (winScreenUI != null)
            winScreenUI.SetActive(false);
    }

    void Update()
    {
        if (winScreenShown || enemySpawner == null) return;

        bool allWavesEnded = true;
        bool allEnemiesDestroyed = true;

        foreach (var wave in enemySpawner.spawnWaves)
        {
            if (!wave.WaveEnded) allWavesEnded = false;
            if (!wave.AllEnemiesDestroyed) allEnemiesDestroyed = false;
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
        Debug.Log("[WinManager] Win screen activated!");
    }
}
