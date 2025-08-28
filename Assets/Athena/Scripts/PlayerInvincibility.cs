using UnityEngine;
using System.Collections;

public class PlayerInvincibility : MonoBehaviour
{
    [Header("Invincibility Settings")]
    public float invincibilityTime = 1f;       // Default 1s when damaged
    public float blinkInterval = 0.1f;         // Blink speed

    private bool isInvincible = false;
    private bool isBlinking = false;           // True only when invincibility comes from DAMAGE

    private SpriteRenderer[] spriteRenderers;
    private Coroutine invincibilityCoroutine;

    // Public properties for other scripts (HUD, VFX, AI, etc.)
    public bool IsInvincible() => isInvincible;
    public bool IsBlinking => isBlinking;

    private void Awake()
    {
        // Grab all renderers on player + children
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            Debug.LogWarning("PlayerInvincibility: No SpriteRenderers found on this object or children.");
    }

    /// <summary>
    /// Called by Shield or Dash turn invincibility on/off without blinking.
    /// </summary>
    public void SetInvincible(bool value)
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        isInvincible = value;
        isBlinking = false; // shield/dash never blink

        if (!value)
            ToggleRenderers(true); // Always reset visible
    }

    /// <summary>
    /// Called by HitPoints ? triggers timed invincibility WITH blinking.
    /// </summary>
    public void TriggerInvincibility()
    {
        if (!isInvincible)
            invincibilityCoroutine = StartCoroutine(InvincibilityTimer(invincibilityTime));
    }

    private IEnumerator InvincibilityTimer(float duration)
    {
        isInvincible = true;
        isBlinking = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            ToggleRenderers(false);
            yield return new WaitForSeconds(blinkInterval);
            ToggleRenderers(true);
            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval * 2f;
        }

        ToggleRenderers(true); // Reset visible
        isInvincible = false;
        isBlinking = false;
    }

    private void ToggleRenderers(bool state)
    {
        if (spriteRenderers == null) return;

        foreach (var renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.enabled = state;
        }
    }
}
