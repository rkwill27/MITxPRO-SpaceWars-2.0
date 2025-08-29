using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class DestroyOnContact : MonoBehaviour
    {
        public LayerMask thingsThatKillMe;
        public int scoreValue;
        protected bool isDoomed = false;

        [HideInInspector] public EnemySpawner spawner;
        [HideInInspector] public SpawnWave wave;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (isDoomed) return;
            if (!GeneralHelpers.IsInMask(this.thingsThatKillMe, other.gameObject)) return;

            isDoomed = true;

            if (spawner != null)
            {
                spawner.RemoveEnemy(this.gameObject, wave);
            }

            Destroy(this.gameObject);

            if (GameManager.Instance)
            {
                GameManager.Instance.AddScore(this.scoreValue);
            }
        }
    }
}
