using UnityEngine;
using System.Collections;

public class PlayerInvincibility : MonoBehaviour
{
    [Header("Invincibility Settings")]
    public float invincibilityTime = 1f;   // Damage i-frames duration
    public float blinkInterval = 0.1f;     // Blink speed

    [Header("Safeguards")]
    [Tooltip("If true, we force the player visible any time they are not blinking.")]
    public bool enforceVisibilityGuard = true;

    // Internal state split by source:
    private bool abilityInvincible = false;  // e.g., dash/shield/etc. (no blinking)
    private bool damageInvincible = false;  // only for post-damage i-frames (blinking)

    // Convenience properties for other systems
    public bool IsInvincible() => abilityInvincible || damageInvincible;
    public bool IsBlinking => damageInvincible; // blinking == damage i-frames

    private SpriteRenderer[] spriteRenderers;
    private Coroutine blinkRoutine;
    private float blinkEndTime = 0f;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            Debug.LogWarning("PlayerInvincibility: No SpriteRenderers found on this object or children.");

        // Always start visible
        ToggleRenderers(true);
    }

    private void OnEnable()
    {
        // Ensure we never come back enabled while invisible
        ToggleRenderers(true);
    }

    private void OnDisable()
    {
        // Clean up and ensure visibility
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
        abilityInvincible = false;
        damageInvincible = false;
        ToggleRenderers(true);
    }

    /// <summary>
    /// Called by abilities (dash/shield/etc.). Sets non-blinking invincibility.
    /// IMPORTANT: This never stops damage blinking and never hides the sprite.
    /// </summary>
    public void SetInvincible(bool value)
    {
        abilityInvincible = value;
        // If we are not actively blinking (damage), ensure visible now.
        EnsureVisibleIfNotBlinking();
    }

    /// <summary>
    /// Called when taking damage. Triggers timed invincibility WITH blinking.
    /// If already blinking, this extends the blink window.
    /// </summary>
    public void TriggerInvincibility()
    {
        float dur = Mathf.Max(0f, invincibilityTime);

        if (!damageInvincible)
        {
            damageInvincible = true;
            blinkEndTime = Time.time + dur;

            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(BlinkRoutine());
        }
        else
        {
            // Extend existing damage i-frames
            blinkEndTime = Mathf.Max(blinkEndTime, Time.time + dur);
        }
    }

    private IEnumerator BlinkRoutine()
    {
        // Blink ownership: only this routine is allowed to toggle visibility.
        while (Time.time < blinkEndTime)
        {
            ToggleRenderers(false);
            yield return new WaitForSeconds(blinkInterval);
            ToggleRenderers(true);
            yield return new WaitForSeconds(blinkInterval);
        }

        // End of damage i-frames: restore visibility and relinquish control
        damageInvincible = false;
        ToggleRenderers(true);
        blinkRoutine = null;

        // If ability invincibility is still active, we remain invincible but visible.
        // The visibility guard also covers this.
    }

    private void LateUpdate()
    {
        // Watchdog: if not actively blinking, player must be visible.
        if (enforceVisibilityGuard)
            EnsureVisibleIfNotBlinking();
    }

    private void EnsureVisibleIfNotBlinking()
    {
        if (damageInvincible) return; // blinking owns visibility
        if (spriteRenderers == null) return;

        // Only force if any renderer is currently off
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var r = spriteRenderers[i];
            if (r != null && !r.enabled)
            {
                ToggleRenderers(true);
                break;
            }
        }
    }

    private void ToggleRenderers(bool state)
    {
        if (spriteRenderers == null) return;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var r = spriteRenderers[i];
            if (r != null)
                r.enabled = state;
        }
    }
}
