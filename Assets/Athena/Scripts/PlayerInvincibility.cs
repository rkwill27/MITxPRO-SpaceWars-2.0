using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    private bool isInvincible = false;

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    // Optional: Make it timed
    public void SetInvincibleForDuration(float duration)
    {
        if (isInvincible) return;
        isInvincible = true;
        StartCoroutine(InvincibilityTimer(duration));
    }

    private System.Collections.IEnumerator InvincibilityTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}
