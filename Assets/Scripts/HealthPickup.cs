using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Gameplay
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Health Pickup Settings")]
        public int healAmount = 1;
        public AudioClip pickupSound;

        [Header("Lifetime")]
        [Tooltip("If > 0, the pickup will auto-despawn after this many seconds.")]
        public float lifetimeSeconds = 0f;

        // --- Global guards ---
        private static bool isQuitting, isSceneUnloading, isManuallyPaused;

        // Hook global events ONCE, even if no pickups exist in the scene
        static HealthPickup()
        {
            Application.quitting += () => isQuitting = true;
            SceneManager.sceneUnloaded += _ => isSceneUnloading = true;
            SceneManager.sceneLoaded += (_, __) => ResumeSpawning();  // <-- auto-reactivate after any load
        }

        public static bool CanSpawnPickups => !(isQuitting || isSceneUnloading || isManuallyPaused);

        public static void PauseSpawning(bool quitting = false)
        {
            isManuallyPaused = true;
            isSceneUnloading = true;
            if (quitting) isQuitting = true;
        }

        public static void ResumeSpawning()
        {
            isManuallyPaused = false;
            isSceneUnloading = false;
            isQuitting = false;
        }

        public static int DestroyAllExisting()
        {
            var pickups = FindObjectsOfType<HealthPickup>(includeInactive: true);
            for (int i = 0; i < pickups.Length; i++)
                if (pickups[i]) Destroy(pickups[i].gameObject);
            return pickups.Length;
        }

        public static GameObject TrySpawn(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (!CanSpawnPickups || !prefab) return null;
            return Instantiate(prefab, pos, rot, parent);
        }

        private void Awake()
        {
            // If something slipped in during shutdown, remove it immediately
            if (!CanSpawnPickups) { Destroy(gameObject); return; }

            if (lifetimeSeconds > 0f)
                Destroy(gameObject, lifetimeSeconds);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CanSpawnPickups) return;

            var hp = other.GetComponent<HitPoints>();
            if (hp != null && hp.IsAlive)
            {
                hp.HealPlayer(healAmount);
                // if (pickupSound && AudioManager.instance) AudioManager.instance.PlaySFXOneShot(pickupSound);
                Destroy(gameObject);
            }
        }
    }
}
