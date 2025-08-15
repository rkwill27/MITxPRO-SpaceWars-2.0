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

        public void HealPlayer(int healAmount)
        {
            currentHitPoints = Mathf.Clamp(currentHitPoints + healAmount, 0, maxHitPoints);
            onHealed.Invoke();
            UpdateHitPointDisplay();
        }

        private IEnumerator HandleDeath()
        {
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            yield return new WaitForSeconds(deathDelay);

            if (deathScreenUI != null)
            {
                deathScreenUI.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Death screen UI is not assigned in Inspector.");
            }

            Time.timeScale = 0f;
        }
    }
}
