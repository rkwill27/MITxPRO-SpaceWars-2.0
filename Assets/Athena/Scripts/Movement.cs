using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    public static Movement instance;

    private void Awake()
    {
        instance = this;
    }

    public float moveSpeed;
    private Vector2 moveInput;

    // Dash
    private float activeMoveSpeed;
    public float dashSpeed = 8f, dashLength = 0.5f, dashCooldown = 1f;
    private float dashCounter;
    private float dashCooldownCounter;

    private Rigidbody2D theRB;
    private PlayerInvincibility invincibility;

    [Header("UI")]
    public Image dashCooldownImage; // UI cooldown indicator for dash

    void Start()
    {
        theRB = GetComponent<Rigidbody2D>();
        theRB.freezeRotation = true; // Lock rotation so the player doesn't spin

        invincibility = GetComponent<PlayerInvincibility>();

        if (dashCooldownImage != null)
        {
            dashCooldownImage.gameObject.SetActive(false);
            dashCooldownImage.fillAmount = 0f;
        }
    }

    void Update()
    {
        // Movement input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        activeMoveSpeed = moveSpeed;

        // Handle dash input
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownCounter <= 0 && dashCounter <= 0)
        {
            // Start dash
            dashCounter = dashLength;
            dashCooldownCounter = dashCooldown;

            // Start cooldown image
            if (dashCooldownImage != null)
            {
                dashCooldownImage.gameObject.SetActive(true);
                StartCoroutine(ShrinkCooldownImage(dashCooldownImage, dashCooldown));
            }

            // Set invincible during dash
            if (invincibility != null)
            {
                invincibility.SetInvincibleForDuration(dashLength);
            }
        }

        // Dash countdown
        if (dashCounter > 0)
        {
            activeMoveSpeed = dashSpeed;
            dashCounter -= Time.deltaTime;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        // Dash cooldown countdown
        if (dashCooldownCounter > 0)
        {
            dashCooldownCounter -= Time.deltaTime;

            if (dashCooldownCounter <= 0 && dashCooldownImage != null)
            {
                dashCooldownImage.gameObject.SetActive(false);
                dashCooldownImage.fillAmount = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        theRB.velocity = moveInput * activeMoveSpeed;
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
    }
}
