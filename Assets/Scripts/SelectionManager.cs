using System.Collections;
using Scripts.Gameplay;            // For HealthPickup and GameManager guard
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI
{
    /// <summary>
    /// SelectionManager
    /// - Hook GoToMainMenu() to your Main Menu button's OnClick.
    /// - Cleans up HealthPickup objects before loading the menu.
    /// - Sets GameManager.IsQuittingOrRestarting = true to prevent late spawns.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        // Re-entry guard so we don't trigger twice
        private static bool _isTransitioning = false;

        public void GoToMainMenu()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            // Ensure gameplay is unpaused
            if (Time.timeScale != 1f) Time.timeScale = 1f;

            // Signal other systems not to spawn/detach new objects during transition
            try
            {
                GameManager.IsQuittingOrRestarting = true;
            }
            catch { /* If GameManager not present, ignore */ }

            StartCoroutine(LoadMainMenuRoutine());
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            // 1) Proactively destroy any existing HealthPickup instances
            var pickups = FindObjectsOfType<HealthPickup>(includeInactive: true);
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null && pickups[i].gameObject != null)
                {
                    // Use Destroy (not DestroyImmediate) to avoid activation/deactivation errors
                    Destroy(pickups[i].gameObject);
                }
            }

            // 2) Give Unity a frame to process the destroys
            yield return null;

            // Optional: free unused references before switching (safe and lightweight)
            yield return Resources.UnloadUnusedAssets();

            // 3) Load the main menu
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Static helper if you prefer calling from other scripts.
        /// </summary>
        public static void GoToMainMenuStatic(string sceneName = "MainMenu")
        {
            // Ensure gameplay is unpaused
            if (Time.timeScale != 1f) Time.timeScale = 1f;

            try
            {
                GameManager.IsQuittingOrRestarting = true;
            }
            catch { /* If GameManager not present, ignore */ }

            // Do a best-effort cleanup if a SelectionManager instance isn't around.
            var pickups = Object.FindObjectsOfType<HealthPickup>(includeInactive: true);
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null && pickups[i].gameObject != null)
                    Object.Destroy(pickups[i].gameObject);
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
