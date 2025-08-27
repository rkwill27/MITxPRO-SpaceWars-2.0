using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Optional: assign in Inspector
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;

    [Header("Fire Rate")]
    public float fireInterval = 0.25f;
    private float fireTimer;

    [Header("UI Cooldown")]
    public Image cooldownImage;
    private Coroutine cooldownCoroutine;

    private void Start()
    {
        fireTimer = fireInterval;

        if (projectilePrefab == null)
        {
            // Fallback: try to load from Resources folder
            projectilePrefab = Resources.Load<GameObject>("ProjectilePrefab");

            if (projectilePrefab == null)
            {
                Debug.LogError("Projectile prefab not assigned and not found in Resources/ProjectilePrefab!");
                return;
            }
        }

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
            cooldownImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f && projectilePrefab != null)
        {
            FireProjectile();
            fireTimer = fireInterval;
            StartCooldown();
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        // Spawn at firePoint position, no parent
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        // Ensure all SpriteRenderers are enabled
        foreach (SpriteRenderer sr in projectile.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = true;

        // Enable Animator if exists
        Animator anim = projectile.GetComponent<Animator>();
        if (anim != null) anim.enabled = true;

        // Attach ProjectileBehavior if not already attached
        ProjectileBehavior behavior = projectile.GetComponent<ProjectileBehavior>();
        if (behavior == null)
        {
            behavior = projectile.AddComponent<ProjectileBehavior>();
        }

        behavior.Initialize(Vector2.up, projectileSpeed, projectileLifetime);
    }


    private void StartCooldown()
    {
        if (cooldownImage == null) return;

        cooldownImage.gameObject.SetActive(true);

        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        cooldownCoroutine = StartCoroutine(ShrinkCooldownImage(cooldownImage, fireInterval));
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
