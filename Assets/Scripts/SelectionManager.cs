using System.Collections;
using Scripts.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI
{
    public class SelectionManager : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private static bool _isTransitioning = false;

        public void GoToMainMenu()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            if (Time.timeScale != 1f) Time.timeScale = 1f;
            TrySetQuittingOrRestarting();

            // CHANGED: use the serialized field, not the hard-coded string
            StartCoroutine(LoadSceneClean(mainMenuSceneName));
        }

        // NEW: call this from your death screen “Restart” button
        public void RestartLevel()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;
            if (Time.timeScale != 1f) Time.timeScale = 1f;
            TrySetQuittingOrRestarting();
            var current = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneClean(current));
        }

        private IEnumerator LoadSceneClean(string sceneName)
        {
            // NEW: validate the scene is in Build Settings before doing cleanup
            if (!CanLoadSceneByName(sceneName))
            {
                Debug.LogError("SelectionManager: Scene '" + sceneName + "' is not in Build Settings. " +
                               "Open File -> Build Settings and add it to 'Scenes In Build'.");
                _isTransitioning = false;
                yield break;
            }

            // 1) Proactively destroy any existing HealthPickup instances
            var pickups = FindObjectsOfType<HealthPickup>(includeInactive: true);
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null)
                    Destroy(pickups[i].gameObject);
            }

            // 2) Give Unity a frame to process destroys
            yield return null;

            // 3) Optional: free unused references
            yield return Resources.UnloadUnusedAssets();

            // 4) Load the scene
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            _isTransitioning = false; // reset for next time
        }

        private static void TrySetQuittingOrRestarting()
        {
            try { GameManager.IsQuittingOrRestarting = true; } catch { }
        }

        public static void GoToMainMenuStatic(string sceneName = "MainMenu")
        {
            if (Time.timeScale != 1f) Time.timeScale = 1f;
            TrySetQuittingOrRestarting();

            var pickups = Object.FindObjectsOfType<HealthPickup>(includeInactive: true);
            for (int i = 0; i < pickups.Length; i++)
                if (pickups[i] != null) Object.Destroy(pickups[i].gameObject);

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            if (Time.timeScale != 1f) Time.timeScale = 1f;
            TrySetQuittingOrRestarting();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // NEW: build-settings guard that works in player builds
        private static bool CanLoadSceneByName(string sceneName)
        {
            return Application.CanStreamedLevelBeLoaded(sceneName);
        }
    }
}
