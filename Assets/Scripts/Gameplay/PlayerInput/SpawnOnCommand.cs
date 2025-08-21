using Scripts.Helpers;
using Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    public class SpawnOnCommand : InputHandlerBase
    {
        [Header("Projectile Settings")]
        public GameObject prefabSpawnMe;
        public Transform spawnedObjectParent;
        public SpawnInfo spawnInfo = new SpawnInfo();

        [Tooltip("Seconds between shots")]
        public float spawnInterval = 0.25f; // fire rate
        private float spawnTimer;

        [Tooltip("Projectile travel speed on Y axis")]
        public float projectileSpeed = 10f;

        [Tooltip("Seconds before projectile self-destructs")]
        public float projectileLifetime = 5f;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void Update()
        {
            if (PauseManager.IsPaused || !this.ShouldProcessInput) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnPrefab();
                spawnTimer = spawnInterval;
            }
        }

        private void SpawnPrefab()
        {
            if (this.prefabSpawnMe == null) return;

            // Spawn projectile (SpawnInfo returns Transform)
            Transform projectileTransform = this.spawnInfo.Spawn(
                this.transform,
                this.prefabSpawnMe.transform,
                this.spawnedObjectParent
            );

            GameObject projectile = projectileTransform.gameObject;

            // Make projectile move up the Y axis
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.up * projectileSpeed;
            }

            // Destroy projectile after lifetime
            Destroy(projectile, projectileLifetime);
        }
    }
}
