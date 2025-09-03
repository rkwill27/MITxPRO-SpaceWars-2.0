using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Reflection;

public class WinManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public GameObject winScreenUI;

    [Header("Win Screen")]
    public bool triggerWinScreen = true;
    public float winScreenDelay = 0f;

    [Header("Next Level")]
    public bool loadNextLevelOnWin = false;
    public string nextSceneName;
    public float nextSceneDelay = 0f;

    [Header("Player Handling")]
    public GameObject player;

    public bool freezePlayerOnWin = true;
    public float freezeControlDelay = 0f;

    public bool pauseProjectileSpawnersOnWin = true;

    public bool repositionPlayerOnWin = true;
    public Transform resetPosition;
    public float waitAtResetPosition = 0f;
    public Transform resetEnd;
    public float waitAtResetEnd = 0f;

    [Tooltip("Field or property name on the player's movement script (e.g., moveSpeed, speed).")]
    public string speedFieldName = "moveSpeed";

    private bool winSequenceStarted = false;

    void Start()
    {
        if (winScreenUI != null) winScreenUI.SetActive(false);
    }

    void Update()
    {
        if (winSequenceStarted) return;
        if (enemySpawner == null) return;

        if (IsWinConditionMet())
        {
            winSequenceStarted = true;
            StartCoroutine(RunWinSequence());
        }
    }

    private bool IsWinConditionMet()
    {
        if (enemySpawner.spawnWaves == null || enemySpawner.spawnWaves.Count == 0)
            return false;

        for (int i = 0; i < enemySpawner.spawnWaves.Count; i++)
        {
            var w = enemySpawner.spawnWaves[i];
            if (!w.WaveEnded || !w.AllEnemiesDestroyed)
                return false;
        }
        return true;
    }

    private IEnumerator RunWinSequence()
    {
        // Player-related handling runs alongside UI/scene work (but only if those toggles are on)
        if (freezePlayerOnWin || pauseProjectileSpawnersOnWin || repositionPlayerOnWin)
            StartCoroutine(HandlePlayerFlow());

        // Win screen (optional)
        if (triggerWinScreen)
        {
            if (winScreenDelay > 0f)
                yield return new WaitForSeconds(winScreenDelay);

            if (winScreenUI != null)
            {
                winScreenUI.SetActive(true);
                // Pause gameplay AFTER showing the UI; player escort uses unscaled time so it remains visible.
                Time.timeScale = 0f;
            }
        }

        // Scene transition (optional)
        if (loadNextLevelOnWin && !string.IsNullOrEmpty(nextSceneName))
        {
            if (nextSceneDelay > 0f)
                yield return new WaitForSecondsRealtime(nextSceneDelay);

            Time.timeScale = 1f; // resume before switching scenes
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator HandlePlayerFlow()
    {
        if (player == null) yield break;

        if (freezeControlDelay > 0f)
            yield return new WaitForSecondsRealtime(freezeControlDelay);

        // Pause projectile spawners first (optional)
        if (pauseProjectileSpawnersOnWin)
            PauseProjectileSpawners(player);

        // Freeze player controls (optional)
        if (freezePlayerOnWin)
        {
            // Disable all scripts directly on the player
            var directScripts = player.GetComponents<MonoBehaviour>();
            foreach (var s in directScripts)
            {
                if (s != null && s.enabled) s.enabled = false;
            }

            // Deactivate PlayerInput if present
            var pi = player.GetComponent<PlayerInput>();
            if (pi != null) { try { pi.DeactivateInput(); } catch { } }
        }

        // Reposition/escort (optional)
        if (repositionPlayerOnWin)
        {
            float speed = DetectPlayerMoveSpeed();
            if (speed <= 0f)
            {
                speed = 5f; // fallback so the sequence still plays
                Debug.LogWarning("[WinManager] Could not detect player move speed. Using fallback: " + speed);
            }

            if (resetPosition != null)
            {
                yield return MoveTransformUnscaled(player.transform, resetPosition.position, speed);
                if (waitAtResetPosition > 0f)
                    yield return new WaitForSecondsRealtime(waitAtResetPosition);
            }

            if (resetEnd != null)
            {
                yield return MoveTransformUnscaled(player.transform, resetEnd.position, speed);
                if (waitAtResetEnd > 0f)
                    yield return new WaitForSecondsRealtime(waitAtResetEnd);
            }
        }
    }

    private void PauseProjectileSpawners(GameObject root)
    {
        // Player and children: look for likely spawner scripts and stop them
        var allBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in allBehaviours)
        {
            if (mb == null) continue;
            string name = mb.GetType().Name.ToLowerInvariant();

            bool looksLikeSpawner =
                (name.Contains("projectile") && (name.Contains("spawn") || name.Contains("emit") || name.Contains("shoot"))) ||
                name.Contains("weapon") || name.Contains("gun");

            if (!looksLikeSpawner) continue;

            // Call Pause() if it exists
            var pause = mb.GetType().GetMethod("Pause", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (pause != null && pause.GetParameters().Length == 0)
            {
                try { pause.Invoke(mb, null); } catch { }
            }

            // Stop any coroutines on that spawner
            try { mb.StopAllCoroutines(); } catch { }

            // Disable the component to prevent further updates
            if (mb.enabled) mb.enabled = false;
        }
    }


    private float DetectPlayerMoveSpeed()
    {
        if (player == null) return 0f;

        string[] candidates = string.IsNullOrEmpty(speedFieldName)
            ? new[] { "moveSpeed", "speed", "Speed", "MoveSpeed" }
            : new[] { speedFieldName, "moveSpeed", "speed", "Speed", "MoveSpeed" };

        var comps = player.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();

            foreach (var n in candidates)
            {
                var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.PropertyType == typeof(float))
                {
                    object v = p.GetValue(c, null);
                    if (v is float pf && pf > 0f) return pf;
                }

                var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.FieldType == typeof(float))
                {
                    object v = f.GetValue(c);
                    if (v is float ff && ff > 0f) return ff;
                }
            }
        }
        return 0f;
    }

    private IEnumerator MoveTransformUnscaled(Transform target, Vector3 destination, float unitsPerSecond)
    {
        if (target == null) yield break;
        if (unitsPerSecond <= 0f) unitsPerSecond = 0.01f;

        const float epsilon = 0.01f;
        while (true)
        {
            Vector3 toDest = destination - target.position;
            float dist = toDest.magnitude;

            if (dist <= epsilon)
            {
                target.position = destination;
                yield break;
            }

            float step = unitsPerSecond * Time.unscaledDeltaTime;
            if (step >= dist)
            {
                target.position = destination;
                yield break;
            }

            target.position += toDest.normalized * step;
            yield return null;
        }
    }
}
