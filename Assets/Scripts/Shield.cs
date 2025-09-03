using UnityEngine;
using UnityEngine.UI; // Needed for Image
using System.Collections;

public class Shield : MonoBehaviour
{
    [Header("Shield Settings")]
    public GameObject shield;
    public float shieldCooldown = 5f;
    public float shieldTime = 2f;
    public bool isShieldActive = false;

    [Header("Cooldown UI")]
    public Image cooldownImage; // UI image shown while shield is active

    private float shieldCooldownTimer = 0f;
    private PlayerInvincibility invincibility;

    void Start()
    {
        invincibility = GetComponent<PlayerInvincibility>();

        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(false); // Always start disabled
            cooldownImage.fillAmount = 0f;             // Ensure empty at start
        }
    }

    void Update()
    {
        if (shieldCooldownTimer > 0f)
        {
            shieldCooldownTimer -= Time.deltaTime;

            if (shieldCooldownTimer <= 0f && cooldownImage != null)
            {
                cooldownImage.gameObject.SetActive(false); // Hide when cooldown ends
                cooldownImage.fillAmount = 0f;
            }
        }

        if (Input.GetMouseButtonDown(0) && shieldCooldownTimer <= 0f && !isShieldActive)
        {
            StartCoroutine(ActivateShield());

            AudioManager.instance.PlaySFX(2);
        }
    }

    private IEnumerator ActivateShield()
    {
        isShieldActive = true;
        shield.SetActive(true);
        shieldCooldownTimer = shieldCooldown;

        // Show cooldown image
        if (cooldownImage != null)
        {
            cooldownImage.gameObject.SetActive(true);
            StartCoroutine(ShrinkCooldownImage(cooldownImage, shieldTime));
        }

        invincibility?.SetInvincible(true);

        yield return new WaitForSeconds(shieldTime);

        invincibility?.SetInvincible(false);
        isShieldActive = false;
        shield.SetActive(false);
    }

    private IEnumerator ShrinkCooldownImage(Image img, float duration)
    {
        img.fillAmount = 1f; // Start full
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.fillAmount = Mathf.Clamp01(1f - (elapsed / duration)); // Shrink to 0
            yield return null;
        }

        img.fillAmount = 0f; // Ensure empty at the end
    }
}
