using UnityEngine;

namespace Scripts.Gameplay
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Health Pickup Settings")]
        public int healAmount = 1;
        public AudioClip pickupSound;

        private static bool applicationIsQuitting = false;

        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        private void OnDisable()
        {
            // If Unity is quitting, destroy immediately to prevent leftover objects
            if (applicationIsQuitting && gameObject != null)
            {
                DestroyImmediate(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (applicationIsQuitting) return;

            HitPoints playerHealth = other.GetComponent<HitPoints>();

            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.HealPlayer(healAmount);

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}
