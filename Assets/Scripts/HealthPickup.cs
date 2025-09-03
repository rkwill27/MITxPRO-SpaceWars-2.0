using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Gameplay
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Health Pickup Settings")]
        public int healAmount = 1;
        public AudioClip pickupSound;

        private static bool isQuitting = false;
        private static bool isSceneUnloading = false;

        private void OnEnable()
        {
            Application.quitting += HandleQuitting;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;   // NEW
        }

        private void OnDisable()
        {
            // IMPORTANT: unsubscribe to avoid keeping this instance alive across unloads
            Application.quitting -= HandleQuitting;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.sceneLoaded -= HandleSceneLoaded;    // NEW
        }

        private void OnDestroy()
        {
            // Do NOT spawn anything here.
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isQuitting || isSceneUnloading) return;

            var playerHealth = other.GetComponent<HitPoints>();
            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.HealPlayer(healAmount);

                // Avoid PlayClipAtPoint; use a persistent AudioManager instead
                if (pickupSound != null && AudioManager.instance != null)
                {
                    // AudioManager.instance.PlaySFXOneShot(pickupSound);
                }

                Destroy(gameObject);
            }
        }

        private void HandleQuitting() => isQuitting = true;
        private void HandleSceneUnloaded(Scene _) => isSceneUnloading = true;

        // NEW: reset scene-unloading guard after the next scene is ready
        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            isSceneUnloading = false;
            isQuitting = false; // safe to clear; we're in a fresh scene
        }
    }
}
