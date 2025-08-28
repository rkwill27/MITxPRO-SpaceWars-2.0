using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner; // Assign your EnemySpawner object
    public GameObject winScreenUI;    // Assign your Win Screen UI panel

    private bool winScreenShown = false;

    void Start()
    {
        if (winScreenUI != null)
            winScreenUI.SetActive(false); // Hide at start

        if (enemySpawner == null)
            Debug.LogError("[WinScreen] EnemySpawner not assigned!");
    }

    void Update()
    {
        if (winScreenShown || enemySpawner == null)
            return;

        // Remove destroyed enemies from the list
        enemySpawner.activeEnemies.RemoveAll(e => e == null);

        // Win condition: all waves spawned AND no active enemies
        if (enemySpawner.currentWaveIndex >= enemySpawner.spawnWaves.Count &&
            enemySpawner.activeEnemies.Count == 0)
        {
            ShowWinScreen();
        }
    }

    private void ShowWinScreen()
    {
        if (winScreenUI != null)
            winScreenUI.SetActive(true);

        Time.timeScale = 0f; // Pause game if desired
        winScreenShown = true;
        Debug.Log("[WinScreen] Win screen activated!");
    }
}
