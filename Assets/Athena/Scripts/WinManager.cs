/* sing UnityEngine;

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

        enemySpawner.OnEnemiesChanged += CheckWinCondition;
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
            enemySpawner.OnEnemiesChanged -= CheckWinCondition;
    }

    private void CheckWinCondition()
    {
        if (winScreenShown) return;

        bool allWavesCompleted = true;
        foreach (var wave in enemySpawner.activeWaves)
        {
            if (!wave.isCompleted)
            {
                allWavesCompleted = false;
                break;
            }
        }

        if (allWavesCompleted)
        {
            if (winScreenUI != null)
                winScreenUI.SetActive(true); // Show the win screen UI
            Time.timeScale = 0f;
            winScreenShown = true;
            Debug.Log("[WinManager] Win screen activated!");
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
} */
