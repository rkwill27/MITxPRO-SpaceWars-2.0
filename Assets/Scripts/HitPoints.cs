using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.Gameplay
{
    public class HitPoints : MonoBehaviour
    {
        public static HitPoints Instance { get; private set; }

        [Header("Health Settings")]
        [Range(1, 10)]
        public int maxHitPoints = 10;

        [Tooltip("Player's starting health (max 10).")]
        [Range(0, 10)]
        public int currentHitPoints = 10;

        [Header("UI Settings")]
        public GameObject hitPointPrefab;
        public List<RectTransform> hitPointSlots;

        [Header("Events")]
        public UnityEvent onDamaged;
        public UnityEvent onHealed;
        public UnityEvent onKilled;

        [Header("Death Settings")]
        public GameObject deathScreenUI;
        public float deathDelay = 2f;

        private readonly List<GameObject> activeIcons = new List<GameObject>();
        private Movement movementScript;

        private static bool applicationIsQuitting = false;
        private bool isBeingDestroyed = false; // NEW flag

        public bool IsAlive => currentHitPoints > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Avoid duplicate instances
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            currentHitPoints = Mathf.Clamp(currentHitPoints, 0, maxHitPoints);
            movementScript = GetComponent<Movement>();

            if (hitPointSlots.Count < maxHitPoints)
            {
                Debug.LogWarning("Not enough hitPointSlots assigned in Inspector.");
            }

            UpdateHitPointDisplay();
        }

        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        private void OnDestroy()
        {
            // Mark that this object is being destroyed so no new UI is created
            isBeingDestroyed = true;
        }

        private void UpdateHitPointDisplay()
        {
            // Prevent creating icons during quit or destruction
            if (applicationIsQuitting || isBeingDestroyed) return;

            ClearIcons();

            for (int i = 0; i < currentHitPoints && i < hitPointSlots.Count; i++)
            {
                RectTransform slot = hitPointSlots[i];
                if (slot == null) continue;

                GameObject icon = Instantiate(hitPointPrefab, slot);
                RectTransform iconRect = icon.GetComponent<RectTransform>();

                if (iconRect != null)
                {
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.anchoredPosition = Vector2.zero;
                    iconRect.localRotation = Quaternion.identity;
                    iconRect.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogError("HitPointPrefab must be a UI element with RectTransform.");
                }

                activeIcons.Add(icon);
            }
        }

        private void ClearIcons()
        {
            foreach (GameObject icon in activeIcons)
            {
                if (icon != null)
                    Destroy(icon);
            }
            activeIcons.Clear();
        }

        public void TakeDamage(float damage)
        {
            if (applicationIsQuitting || isBeingDestroyed) return;

            PlayerInvincibility invincibility = GetComponent<PlayerInvincibility>();
            if (invincibility != null && invincibility.IsInvincible())
            {
                // Ignore damage while invincible
                return;
            }

            bool wasDead = !IsAlive;

            if (damage < 0) // Healing
            {
                currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);
                onHealed.Invoke();
                UpdateHitPointDisplay();
                return;
            }

            if (damage > 0 && !wasDead) // Damage
            {
                currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);

                if (IsAlive)
                {
                    onDamaged.Invoke();

                    // 🔥 Only blink when damaged
                    if (invincibility != null)
                        invincibility.TriggerInvincibility();
                }
                else
                {
                    onKilled.Invoke();
                    StartCoroutine(HandleDeath());
                }

                UpdateHitPointDisplay();
                return;
            }

            currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);
        }



        public void HealPlayer(int healAmount)
        {
            if (applicationIsQuitting || isBeingDestroyed) return; // Stop updates when quitting

            currentHitPoints = Mathf.Clamp(currentHitPoints + healAmount, 0, maxHitPoints);
            onHealed.Invoke();
            UpdateHitPointDisplay();
        }

        private IEnumerator HandleDeath()
        {
            // Disable movement
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }

            // Stop input if PlayerInput exists
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }

            // Stop projectile firing
            var projectileSpawner = GetComponent<ProjectileSpawner>();
            if (projectileSpawner != null)
            {
                projectileSpawner.enabled = false;
            }

            // Stop rigidbody physics
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            // Wait before final death actions
            yield return new WaitForSeconds(deathDelay);

            // Show death screen
            if (deathScreenUI != null)
            {
                deathScreenUI.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Death screen UI is not assigned in Inspector.");
            }

            // Destroy player object after showing death screen
            Destroy(gameObject);
        }
    }
}
