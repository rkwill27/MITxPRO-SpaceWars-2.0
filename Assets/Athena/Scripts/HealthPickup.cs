using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Gameplay
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Health Pickup Settings")]
        public int healAmount = 1;
        public AudioClip pickupSound;

        // Global guards
        private static bool isQuitting = false;
        private static bool isSceneUnloading = false;

        private void OnEnable()
        {
            // Subscribe once per domain reload; static flags cover restarts
            Application.quitting += HandleQuitting;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void OnDisable()
        {
            // No cleanup or destroy calls here; Unity is handling teardown.
        }

        private void OnDestroy()
        {
            // Absolutely do NOT spawn anything here.
            // (No particles, sounds, new pickups, etc.)
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // If the app is quitting or the scene is unloading, do nothing.
            if (isQuitting || isSceneUnloading) return;

            HitPoints playerHealth = other.GetComponent<HitPoints>();
            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.HealPlayer(healAmount);

                // Avoid PlayClipAtPoint (spawns a temp GO). Route through AudioManager instead.
                if (pickupSound != null && AudioManager.instance != null)
                {
                    // Implement PlaySFXOneShot in your AudioManager to call an AudioSource.PlayOneShot
                    // on a persistent (DontDestroyOnLoad) AudioSource without spawning new GameObjects.
                    // AudioManager.instance.PlaySFXOneShot(pickupSound);
                }

                // Safe during runtime; Unity handles this on scene unload anyway.
                Destroy(gameObject);
            }
        }

        private void HandleQuitting()
        {
            isQuitting = true;
        }

        private void HandleSceneUnloaded(Scene _)
        {
            // This fires during scene restart/transition
            isSceneUnloading = true;
        }
    }
}
