using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource menuMusic;
    //public AudioSource battleSelectMusic;
    public AudioSource[] bgm;
    private int currentBGM;
    private bool playingBGM;

    public AudioSource[] sfx;

    // Whitelists for fast checks
    private HashSet<AudioSource> bgmSet = new HashSet<AudioSource>();
    private HashSet<AudioSource> sfxSet = new HashSet<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Build whitelists
            RebuildSets();

            // Safety: prevent any music from starting automatically.
            if (menuMusic != null) menuMusic.playOnAwake = false;
            if (bgm != null)
            {
                foreach (var track in bgm)
                {
                    if (track != null) track.playOnAwake = false;
                }
            }
        }
        else if (instance != this)
        {
            // Mute the duplicate immediately so it can't play anything before being destroyed.
            if (menuMusic != null) menuMusic.Stop();
            if (bgm != null)
            {
                foreach (var track in bgm)
                {
                    if (track != null) track.Stop();
                }
            }
            if (sfx != null)
            {
                foreach (var fx in sfx)
                {
                    if (fx != null) fx.Stop();
                }
            }
            Destroy(gameObject);
            return;
        }
    }

    private void RebuildSets()
    {
        bgmSet.Clear();
        if (bgm != null)
        {
            foreach (var a in bgm) if (a != null) bgmSet.Add(a);
        }

        sfxSet.Clear();
        if (sfx != null)
        {
            foreach (var a in sfx) if (a != null) sfxSet.Add(a);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // (no changes)
    }

    // Update is called once per frame
    void Update()
    {
        // If menu music is playing, do NOT advance/rotate BGM and make sure none of the BGM is active.
        if (menuMusic != null && menuMusic.isPlaying)
        {
            if (bgm != null)
            {
                for (int i = 0; i < bgm.Length; i++)
                {
                    if (bgm[i] != null)
                    {
                        bgm[i].Stop();
                        bgm[i].enabled = false; // disable all BGM while on menu
                    }
                }
            }
            playingBGM = false;

            // While in menu, also make sure nothing else random is playing
            StopNonWhitelistedAudio(allowMenu: true);
            return;
        }

        // Gameplay: enforce single, whitelisted BGM
        LockToCurrentBGM();
        StopNonWhitelistedAudio(allowMenu: false);

        if (playingBGM && bgm != null && bgm.Length > 0)
        {
            if (bgm[currentBGM] != null && bgm[currentBGM].isPlaying == false)
            {
                currentBGM++;
                if (currentBGM >= bgm.Length)
                {
                    currentBGM = 0;
                }

                if (bgm[currentBGM] != null)
                {
                    bgm[currentBGM].enabled = true;
                    bgm[currentBGM].loop = false;
                    bgm[currentBGM].Play();
                    // Re-enforce the single-source invariant immediately after switching
                    LockToCurrentBGM();
                    StopNonWhitelistedAudio(allowMenu: false);
                }
            }
        }
    }

    public void StopMusic()
    {
        if (menuMusic != null) menuMusic.Stop();
        //battleSelectMusic.Stop();
        if (bgm != null)
        {
            foreach (AudioSource track in bgm)
            {
                if (track != null)
                {
                    track.Stop();
                    track.enabled = false; // keep them disabled when globally stopped
                }
            }
        }

        playingBGM = false;
    }

    public void PlayMenuMusic()
    {
        // Hard-stop everything first to guarantee no overlap.
        StopMusic();

        // Keep your structure: explicitly (re)start menu music.
        if (menuMusic != null && menuMusic.isPlaying == false)
        {
            menuMusic.loop = true;
            menuMusic.Play();
        }

        // Ensure BGM rotator is OFF while in menu.
        playingBGM = false;

        // STRICT MENU-ONLY ENFORCEMENT:
        var allSources = FindObjectsOfType<AudioSource>(true);
        foreach (var src in allSources)
        {
            if (src == null) continue;
            if (menuMusic != null && src == menuMusic) continue;
            // If you want UI SFX while on menu, skip those here instead of stopping them.
            // if (sfx != null && System.Array.IndexOf(sfx, src) >= 0) continue;
            if (src.isPlaying) src.Stop();
            // Also disable any BGM components
            if (bgmSet.Contains(src)) src.enabled = false;
        }
    }

    /* public void PlayBattleSelectMusic()
    {
        if (battleSelectMusic.isPlaying == false)
            StopMusic();
        battleSelectMusic.Play();
    } */

    public void PlayBGM()
    {
        // Rebuild sets in case you've changed arrays in inspector
        RebuildSets();

        // Switching to gameplay: stop everything first.
        StopMusic();

        if (bgm == null || bgm.Length == 0 || bgmSet.Count == 0)
        {
            Debug.LogError("[AudioManager] No BGM sources assigned. Only AudioManager.bgm is allowed to play BGM.");
            // Kill any non-whitelisted audio just in case.
            StopNonWhitelistedAudio(allowMenu: false);
            return;
        }

        // Ensure all bgm are stopped and disabled before starting one (prevents overlaps from stray states).
        for (int i = 0; i < bgm.Length; i++)
        {
            if (bgm[i] != null)
            {
                bgm[i].Stop();
                bgm[i].enabled = false;
                bgm[i].loop = false;
                bgm[i].time = 0f;
            }
        }

        currentBGM = Random.Range(0, bgm.Length);
        // Skip null holes safely
        int guard = 0;
        while ((bgm[currentBGM] == null) && guard++ < 128)
        {
            currentBGM = (currentBGM + 1) % bgm.Length;
        }
        if (bgm[currentBGM] == null)
        {
            Debug.LogError("[AudioManager] All entries in BGM array are null.");
            StopNonWhitelistedAudio(allowMenu: false);
            return;
        }

        bgm[currentBGM].enabled = true;   // << allow only the chosen track to run
        bgm[currentBGM].loop = false;     // rotation handled by Update
        bgm[currentBGM].Play();
        playingBGM = true;

        // Double-check whitelist invariant right after starting.
        LockToCurrentBGM();
        StopNonWhitelistedAudio(allowMenu: false);
    }

    public void PlaySFX(int sfxToPlay)
    {
        if (sfx == null || sfxToPlay < 0 || sfxToPlay >= sfx.Length) return;
        var src = sfx[sfxToPlay];
        if (src == null) return;

        // Keep your behavior but avoid clipping: stop then play.
        src.Stop();
        src.Play();
    }

    // --- Helpers ---

    // Ensures only the currentBGM source is enabled/playing; forcibly disables others in the BGM list.
    private void LockToCurrentBGM()
    {
        if (!playingBGM || bgm == null || bgm.Length == 0) return;

        // Fix invalid index/null current by choosing first valid
        if (currentBGM < 0 || currentBGM >= bgm.Length || bgm[currentBGM] == null)
        {
            for (int i = 0; i < bgm.Length; i++)
            {
                if (bgm[i] != null)
                {
                    currentBGM = i;
                    break;
                }
            }
        }

        for (int i = 0; i < bgm.Length; i++)
        {
            var src = bgm[i];
            if (src == null) continue;

            if (i == currentBGM)
            {
                if (!src.enabled) src.enabled = true;
                if (!src.isPlaying) src.Play();
            }
            else
            {
                if (src.isPlaying) src.Stop();
                if (src.enabled) src.enabled = false; // prevents external scripts from re-playing it
                src.time = 0f;
            }
        }
    }

    // Stops anything not explicitly whitelisted (menu, currentBGM, sfx array).
    private void StopNonWhitelistedAudio(bool allowMenu)
    {
        var all = FindObjectsOfType<AudioSource>(true);
        AudioSource current = (playingBGM && bgm != null && currentBGM >= 0 && currentBGM < bgm.Length) ? bgm[currentBGM] : null;

        foreach (var src in all)
        {
            if (src == null) continue;

            // Whitelist checks:
            if (allowMenu && menuMusic != null && src == menuMusic) continue;
            if (current != null && src == current) continue;
            if (sfxSet.Contains(src)) continue; // keep your SFX alive

            // If it is a BGM candidate but not the current one, kill it.
            if (bgmSet.Contains(src) && src != current)
            {
                if (src.isPlaying) src.Stop();
                src.enabled = false;
                continue;
            }

            // For any other AudioSource in the scene: if it's playing and NOT whitelisted, stop it.
            // (Covers rogue BGM on separate GameObjects not wired into AudioManager.bgm)
            if (src.isPlaying)
            {
                src.Stop();
            }
        }
    }
}
