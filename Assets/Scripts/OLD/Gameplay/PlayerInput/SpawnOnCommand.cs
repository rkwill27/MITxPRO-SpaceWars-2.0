using Scripts.Helpers;
using Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

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

        [Header("UI")]
        public Image spawnCooldownImage; // UI cooldown indicator for spawns

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Hide inherited member instead of overriding
        private new void Start()
        {
            // Prevent invisible first projectile firing on frame 0
            spawnTimer = spawnInterval;

            if (spawnCooldownImage != null)
            {
                spawnCooldownImage.gameObject.SetActive(false);
                spawnCooldownImage.fillAmount = 0f;
            }
        }

        // Hide inherited member instead of overriding
        private new void Update()
        {
            if (PauseManager.IsPaused || !this.ShouldProcessInput) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnPrefab();
                spawnTimer = spawnInterval;

                // Start cooldown indicator
                if (spawnCooldownImage != null)
                {
                    spawnCooldownImage.gameObject.SetActive(true);
                    StartCoroutine(ShrinkCooldownImage(spawnCooldownImage, spawnInterval));
                }
            }
        }

        private void SpawnPrefab()
        {
            if (this.prefabSpawnMe == null) return;

            // Spawn projectile using SpawnInfo
            Transform projectileTransform = this.spawnInfo.Spawn(
                this.transform,
                this.prefabSpawnMe.transform,
                this.spawnedObjectParent
            );

            if (projectileTransform == null) return;

            GameObject projectile = projectileTransform.gameObject;

            // Ensure projectile is active (fixes frame 0 invisibility)
            projectile.SetActive(true);

            // Enable all SpriteRenderers in this projectile, including children
            foreach (SpriteRenderer sr in projectile.GetComponentsInChildren<SpriteRenderer>(true))
            {
                sr.enabled = true;
            }

            // Enable Animator if it exists
            Animator animator = projectile.GetComponent<Animator>();
            if (animator != null) animator.enabled = true;

            // Make projectile move up the Y axis
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true; // ensure physics is active
                rb.velocity = Vector2.up * projectileSpeed;
            }

            // Destroy projectile after lifetime
            Destroy(projectile, projectileLifetime);
        }

        private IEnumerator ShrinkCooldownImage(Image img, float duration)
        {
            img.fillAmount = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                img.fillAmount = Mathf.Clamp01(1f - (elapsed / duration));
                yield return null;
            }

            img.fillAmount = 0f;
            img.gameObject.SetActive(false);
        }
    }
}
