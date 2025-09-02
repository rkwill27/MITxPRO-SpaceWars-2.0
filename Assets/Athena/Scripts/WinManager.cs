using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject winScreenUI;

    [Header("Win Settings")]
    public bool triggerWinScreen = true; // Option to trigger win screen
    public float winScreenDelay = 0f;    // Delay before showing win screen

    public bool loadNextLevelOnWin = false; // Option to load next scene
    public string nextSceneName;            // Name of scene to load
    public float nextSceneDelay = 0f;       // Delay before loading next scene

    private bool winConditionHandled = false;

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
        if (winConditionHandled || enemySpawner == null) return;

        bool allWavesEnded = true;
        bool allEnemiesDestroyed = true;

        foreach (var wave in enemySpawner.spawnWaves)
        {
            if (!wave.WaveEnded) allWavesEnded = false;
            if (!wave.AllEnemiesDestroyed) allEnemiesDestroyed = false;
        }

        if (allWavesEnded && allEnemiesDestroyed)
        {
            StartCoroutine(HandleWinCondition());
        }
    }

    private IEnumerator HandleWinCondition()
    {
        winConditionHandled = true;

        // Delay before win screen
        if (triggerWinScreen)
        {
            if (winScreenDelay > 0f)
                yield return new WaitForSeconds(winScreenDelay);

            if (winScreenUI != null)
            {
                winScreenUI.SetActive(true);
                Time.timeScale = 0f; // Pause the game
                Debug.Log("[WinManager] Win screen activated!");
            }
        }

        // Delay before loading next scene
        if (loadNextLevelOnWin && !string.IsNullOrEmpty(nextSceneName))
        {
            if (nextSceneDelay > 0f)
                yield return new WaitForSecondsRealtime(nextSceneDelay);

            Time.timeScale = 1f; // Resume time before switching scenes
            Debug.Log($"[WinManager] Loading next level: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
