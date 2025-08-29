using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class DestroyOnContact : MonoBehaviour
    {
        public LayerMask thingsThatKillMe;
        public int scoreValue;
        private bool isDoomed = false;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (isDoomed) return;
            if (!GeneralHelpers.IsInMask(this.thingsThatKillMe, other.gameObject)) return;

            isDoomed = true;

            // Find EnemyPathing on self or parents to get references
            EnemyPathing pathing = GetComponentInParent<EnemyPathing>();
            GameObject root = transform.root != null ? transform.root.gameObject : gameObject;

            Debug.Log($"[DestroyOnContact] Destroy request from '{name}' (child). Root='{root.name}' (ID:{root.GetInstanceID()}) | " +
                      $"HasPathing:{(pathing != null)} | SpawnerNull:{(pathing == null || pathing.spawner == null)} | WaveName:{(pathing != null && pathing.wave != null ? pathing.wave.WaveName : "NULL")}");

            // Notify spawner BEFORE destroy, using ROOT
            if (pathing != null && pathing.spawner != null && pathing.wave != null)
            {
                pathing.spawner.RemoveEnemy(root, pathing.wave);
            }
            else
            {
                Debug.LogWarning("[DestroyOnContact] Could not notify spawner: missing EnemyPathing/Spawner/Wave on parent.");
            }

            // Destroy ROOT enemy object (not just the child collider)
            Destroy(root);

            if (GameManager.Instance)
                GameManager.Instance.AddScore(this.scoreValue);
        }
    }
}
