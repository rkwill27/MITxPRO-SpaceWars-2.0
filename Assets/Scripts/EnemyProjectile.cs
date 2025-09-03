using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float lifeTime = 5f;

    private float lifeTimer;

    void Start()
    {
        lifeTimer = lifeTime;
    }

    void Update()
    {
        transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Optionally: Damage the player here
            Destroy(gameObject); // Destroy projectile on contact
        }
    }
}
