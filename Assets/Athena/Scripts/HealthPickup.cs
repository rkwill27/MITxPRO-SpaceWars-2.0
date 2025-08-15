using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float waitToBeCollected = 0.5f;
    public int healAmount = 1; // How much health to restore

    private GameObject deathScreen;

    void Start()
    {
        // Automatically find the Death Screen UI object by name in the scene
        deathScreen = GameObject.Find("DeathScreenUI");

        if (deathScreen == null)
        {
            Debug.LogWarning("DeathScreenUI not found in scene. Health pickups will not auto-destroy on game over.");
        }
    }

    void Update()
    {
        // Countdown before pickup can be collected
        if (waitToBeCollected > 0)
            waitToBeCollected -= Time.deltaTime;

        // Auto-destroy if game is over (timeScale = 0 and death screen is active)
        if (Time.timeScale == 0 && deathScreen != null && deathScreen.activeSelf)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (waitToBeCollected <= 0 && other.CompareTag("Player"))
        {
            // Heal the player if a HitPoints script exists
            if (Scripts.Gameplay.HitPoints.Instance != null)
            {
                Scripts.Gameplay.HitPoints.Instance.HealPlayer(healAmount);
            }

            Destroy(gameObject); // Remove the pickup when collected
        }
    }
}
