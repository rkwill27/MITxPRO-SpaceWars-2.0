using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    public GameObject shield;
    public float shieldCooldown = 5f;
    public float shieldTime = 2f;
    public bool isShieldActive = false;

    private float shieldCooldownTimer = 0f;
    private PlayerInvincibility invincibility;

    void Start()
    {
        invincibility = GetComponent<PlayerInvincibility>();
    }

    void Update()
    {
        if (shieldCooldownTimer > 0f)
            shieldCooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && shieldCooldownTimer <= 0f && !isShieldActive)
        {
            StartCoroutine(ActivateShield());
        }
    }

    private IEnumerator ActivateShield()
    {
        isShieldActive = true;
        shield.SetActive(true);
        shieldCooldownTimer = shieldCooldown;

        invincibility?.SetInvincible(true);

        yield return new WaitForSeconds(shieldTime);

        invincibility?.SetInvincible(false);
        isShieldActive = false;
        shield.SetActive(false);
    }
}
