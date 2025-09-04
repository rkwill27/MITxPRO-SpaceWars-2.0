using System.Collections;
using Scripts.Gameplay;            // HealthPickup
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI
{
    public class SelectionManager : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private static bool _isTransitioning = false;

        public void RestartLevel()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            if (Time.timeScale != 1f) Time.timeScale = 1f;

            // Pause spawning before any destruction/unload begins
            HealthPickup.PauseSpawning();
            StartCoroutine(LoadSceneClean(SceneManager.GetActiveScene().name));
        }

        public void GoToMainMenu()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            if (Time.timeScale != 1f) Time.timeScale = 1f;

            HealthPickup.PauseSpawning();
            StartCoroutine(LoadSceneClean(mainMenuSceneName));
        }

        private IEnumerator LoadSceneClean(string sceneName)
        {
            // Clear any lingering pickups now
            HealthPickup.DestroyAllExisting();
            yield return null; // allow Destroy() to process
            yield return Resources.UnloadUnusedAssets();

            // OPTIONAL belt-and-suspenders: ensure spawning is re-enabled
            // as soon as the next scene has fully loaded.
            SceneManager.sceneLoaded += OnSceneLoadedResume;

            // Load the next scene (HealthPickup also resumes via its static listener)
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            _isTransitioning = false;
        }

        // One-shot resume handler; unsubscribes itself after firing once.
        private void OnSceneLoadedResume(Scene s, LoadSceneMode m)
        {
            HealthPickup.ResumeSpawning();
            SceneManager.sceneLoaded -= OnSceneLoadedResume;
        }

        public void QuitGame()
        {
            if (Time.timeScale != 1f) Time.timeScale = 1f;

            // Block any new drops and clean up existing pickups before quitting
            HealthPickup.PauseSpawning(quitting: true);
            HealthPickup.DestroyAllExisting();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
