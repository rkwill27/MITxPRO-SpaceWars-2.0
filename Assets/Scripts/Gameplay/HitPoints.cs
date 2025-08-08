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
        public GameObject hitPointPrefab; // Prefab for each hit point icon
        public List<Transform> hitPointSlots; // Transforms where icons are placed

        [Header("Events")]
        public UnityEvent onDamaged;
        public UnityEvent onHealed;
        public UnityEvent onKilled;

        private List<GameObject> activeHitPoints = new List<GameObject>();

        public bool IsAlive => currentHitPoints > 0;

        private void Start()
        {
            // Safety check and clamping
            currentHitPoints = Mathf.Clamp(currentHitPoints, 0, maxHitPoints);

            Debug.Log("Initializing hit points...");
            Debug.Log("Current HP: " + currentHitPoints);
            Debug.Log("Hit Point Slots Count: " + hitPointSlots.Count);

            InitializeHitPoints();
        }

        private void InitializeHitPoints()
        {
            ClearHitPoints();

            for (int i = 0; i < currentHitPoints && i < hitPointSlots.Count; i++)
            {
                // Instantiate the prefab as a child of the slot
                GameObject hp = Instantiate(hitPointPrefab, hitPointSlots[i]);

                // Ensure it aligns perfectly to the slot's local position
                hp.transform.localPosition = Vector3.zero;

                // Optional: Reset rotation and scale
                hp.transform.localRotation = Quaternion.identity;
                hp.transform.localScale = Vector3.one;

                activeHitPoints.Add(hp);
            }
        }

        private void UpdateHitPointDisplay()
        {
            ClearHitPoints();

            for (int i = 0; i < currentHitPoints && i < hitPointSlots.Count; i++)
            {
                GameObject hp = Instantiate(hitPointPrefab, hitPointSlots[i]);
                hp.transform.localPosition = Vector3.zero;
                hp.transform.localRotation = Quaternion.identity;
                hp.transform.localScale = Vector3.one;

                activeHitPoints.Add(hp);
            }
        }

        private void ClearHitPoints()
        {
            foreach (var hp in activeHitPoints)
            {
                if (hp != null)
                    Destroy(hp);
            }
            activeHitPoints.Clear();
        }

        public void TakeDamage(float damage)
        {
            var wasDead = !IsAlive;

            if (damage < 0)
            {
                currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);
                onHealed.Invoke();
                UpdateHitPointDisplay();
                return;
            }

            if (damage > 0 && !wasDead)
            {
                currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);

                if (IsAlive)
                {
                    onDamaged.Invoke();
                }
                else
                {
                    onKilled.Invoke();
                }

                UpdateHitPointDisplay();
                return;
            }

            // If damage == 0 or already dead
            currentHitPoints = Mathf.Clamp(currentHitPoints - (int)damage, 0, maxHitPoints);
        }
    }
}
