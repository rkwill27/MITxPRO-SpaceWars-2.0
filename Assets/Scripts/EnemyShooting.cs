using System.Collections;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;     // The projectile prefab to spawn
    public Transform[] firePoints;          // Spawn points (one shot per point)
    public float fireRate = 1.5f;           // Time (seconds) between shots
    public float projectileSpeed = 5f;      // Speed the projectile travels

    private bool canShoot = true;

    private void Start()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyShooting] {gameObject.name} has no Fire Points assigned!");
        }
    }

    private void Update()
    {
        if (canShoot && projectilePrefab != null && firePoints != null && firePoints.Length > 0)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine()
    {
        canShoot = false;

        // Spawn one projectile at each fire point
        foreach (var fp in firePoints)
        {
            if (fp == null) continue;

            // Instantiate at each fire point's position
            GameObject projectile = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
            // If you want the projectile to inherit the fire point's rotation, use: fp.rotation

            // Add movement (requires Rigidbody2D on projectile)
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Current behavior: straight down on Y axis
                rb.velocity = Vector2.down * projectileSpeed;
                // Alternative to shoot along the fire point's "up" direction:
                // rb.velocity = (Vector2)fp.up * projectileSpeed;
            }
        }

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}
