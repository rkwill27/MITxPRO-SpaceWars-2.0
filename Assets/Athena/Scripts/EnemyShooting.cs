using System.Collections;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;   // The projectile prefab to spawn
    public Transform firePoint;           // Where the projectile will spawn from
    public float fireRate = 1.5f;         // Time (seconds) between shots
    public float projectileSpeed = 5f;    // Speed the projectile travels

    private bool canShoot = true;

    private void Start()
    {
        if (firePoint == null)
        {
            Debug.LogWarning($"[EnemyShooting] {gameObject.name} has no Fire Point assigned!");
        }
    }

    private void Update()
    {
        if (canShoot && projectilePrefab != null && firePoint != null)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        canShoot = false;

        // Spawn projectile at firePoint position, facing firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Add movement (requires Rigidbody2D on projectile)
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Always move straight down the Y-axis
            rb.velocity = Vector2.down * projectileSpeed;
        }

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}
