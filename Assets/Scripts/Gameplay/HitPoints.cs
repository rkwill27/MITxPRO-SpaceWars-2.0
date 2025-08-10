using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Scripts.Gameplay
{
    public class HitPoints : MonoBehaviour
    {
        [Header("Health Settings")]
        [Range(1, 10)]
        public int maxHitPoints = 10;

        [Tooltip("Set this to the player's starting health in the editor (max 10).")]
        [Range(0, 10)]
        public int currentHitPoints = 10;

        [Header("UI Settings")]
        public GameObject hitPointPrefab;                   // Heart prefab (UI Image)
        public List<RectTransform> hitPointSlots;           // UI positions in Canvas

        [Header("Events")]
        public UnityEvent onDamaged;
        public UnityEvent onHealed;
        public UnityEvent onKilled;

        private readonly List<GameObject> activeIcons = new List<GameObject>();

        public bool IsAlive => currentHitPoints > 0;

        private void Start()
        {
            currentHitPoints = Mathf.Clamp(currentHitPoints, 0, maxHitPoints);

            if (hitPointSlots.Count < maxHitPoints)
            {
                Debug.LogWarning("Not enough hitPointSlots assigned in the Inspector. " +
                                 "Make sure you have 10 RectTransforms under the Canvas.");
            }

            UpdateHitPointDisplay();
        }

        private void UpdateHitPointDisplay()
        {
            ClearIcons();

            // Always fill from the first available slot
            for (int i = 0; i < currentHitPoints && i < hitPointSlots.Count; i++)
            {
                RectTransform slot = hitPointSlots[i];

                GameObject icon = Instantiate(hitPointPrefab, slot);
                RectTransform iconRect = icon.GetComponent<RectTransform>();

                // Reset to perfectly match slot position/size inside UI
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.localRotation = Quaternion.identity;
                iconRect.localScale = Vector3.one;

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
                    onDamaged.Invoke();
                else
                    onKilled.Invoke();

                UpdateHitPointDisplay();
                return;
            }

            // Edge case: damage == 0 or already dead
            currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);
        }
    }
}
