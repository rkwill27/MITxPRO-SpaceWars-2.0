using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float timer;

    public void Initialize(Vector2 dir, float spd, float life)
    {
        direction = dir.normalized;
        speed = spd;
        lifetime = life;
        timer = 0f;
    }

    private void Update()
    {
        // Move projectile manually
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Destroy after lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Damage enemies
        if (other.CompareTag("Enemy"))
        {
            // Implement enemy damage logic here
            // Example:
            // EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            // if (enemy != null) enemy.TakeDamage(1);

            Destroy(gameObject); // Destroy projectile on hit
        }
    }
}
