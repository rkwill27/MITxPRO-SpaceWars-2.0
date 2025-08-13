using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.Gameplay
{
    public class HitPoints : MonoBehaviour
    {
        [Header("Health Settings")]
        [Range(1, 10)]
        public int maxHitPoints = 10;

        [Tooltip("Player's starting health (max 10).")]
        [Range(0, 10)]
        public int currentHitPoints = 10;

        [Header("UI Settings")]
        public GameObject hitPointPrefab; // Heart prefab (UI Image)
        public List<RectTransform> hitPointSlots; // UI positions in Canvas

        [Header("Events")]
        public UnityEvent onDamaged;
        public UnityEvent onHealed;
        public UnityEvent onKilled;

        [Header("Death Settings")]
        public GameObject deathScreenUI; // Assign the death screen in Inspector
        public float deathDelay = 2f;    // Delay before showing death screen

        private readonly List<GameObject> activeIcons = new List<GameObject>();
        private Movement movementScript;

        public bool IsAlive => currentHitPoints > 0;

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

        private void UpdateHitPointDisplay()
        {
            ClearIcons();

            for (int i = 0; i < currentHitPoints && i < hitPointSlots.Count; i++)
            {
                RectTransform slot = hitPointSlots[i];

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

        private IEnumerator HandleDeath()
        {
            // Freeze player movement
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }

            // Stop Rigidbody motion
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            // Wait for death delay before showing death screen
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

            // Freeze all gameplay
            Time.timeScale = 0f;
        }
    }
}
