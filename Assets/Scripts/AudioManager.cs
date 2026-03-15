using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class GameSound
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Music Settings")]
    [Tooltip("Check this if this is background music that should smoothly crossfade!")]
    public bool isMusic = false;

    [Header("Auto-Wiring Settings")]
    public GameObject triggerObject;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("The Sound Library")]
    public GameSound[] sounds;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusicClip;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Lower this number to make the fade faster! Try 0.5 or 0.2.")]
    public float musicFadeTime = 0.5f;

    private Coroutine currentFade;

    void Awake()
    {
        // Safe Singleton: Ensures only ONE AudioManager ever exists in the scene
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 1. Play Default Background Music
        if (musicSource != null && backgroundMusicClip != null)
        {
            musicSource.clip = backgroundMusicClip;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        // 2. The Auto-Wiring
        foreach (GameSound s in sounds)
        {
            if (s.triggerObject != null)
            {
                Button btn = s.triggerObject.GetComponent<Button>();

                if (btn != null)
                {
                    string nameToPlay = s.soundName;
                    btn.onClick.AddListener(() => PlaySound(nameToPlay));
                }
                else
                {
                    AutoSoundTrigger trigger = s.triggerObject.AddComponent<AutoSoundTrigger>();
                    trigger.Initialize(s.soundName, s.isMusic);
                }
            }
        }
    }

    public void PlaySound(string nameToPlay)
    {
        foreach (GameSound s in sounds)
        {
            if (s.soundName == nameToPlay)
            {
                if (s.clip == null) return;

                if (s.isMusic)
                {
                    if (currentFade != null) StopCoroutine(currentFade);
                    currentFade = StartCoroutine(CrossfadeMusic(s.clip, s.volume));
                    return;
                }

                // --- THE FIX: Clean SFX Playback without the rewind bug! ---
                AudioSource tempSpeaker = gameObject.AddComponent<AudioSource>();
                tempSpeaker.clip = s.clip;
                tempSpeaker.volume = s.volume;
                tempSpeaker.Play();

                // Destroy the audio source component right after the clip naturally finishes playing
                Destroy(tempSpeaker, s.clip.length);
                return;
            }
        }
        Debug.LogWarning("Audio Manager: Could not find sound '" + nameToPlay + "'");
    }

    public void PlayDefaultMusic()
    {
        if (musicSource != null && backgroundMusicClip != null && musicSource.clip != backgroundMusicClip)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(CrossfadeMusic(backgroundMusicClip, musicVolume));
        }
    }

    public void StopSound(string nameToStop)
    {
        // Look through all the temporary speakers we created
        AudioSource[] activeSpeakers = GetComponents<AudioSource>();

        foreach (GameSound s in sounds)
        {
            if (s.soundName == nameToStop)
            {
                // Find the speaker playing this exact clip and destroy it!
                foreach (AudioSource speaker in activeSpeakers)
                {
                    if (speaker != musicSource && speaker.clip == s.clip)
                    {
                        Destroy(speaker);
                    }
                }
                return;
            }
        }
    }

    // --- THE SNAPPY FADE LOGIC ---
    private IEnumerator CrossfadeMusic(AudioClip newClip, float targetVol)
    {
        if (musicSource == null) yield break;

        float startVol = musicSource.volume;

        // Fade OUT fast
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVol * (Time.deltaTime / musicFadeTime);
            yield return null;
        }

        // Swap the track
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        // Fade IN fast
        while (musicSource.volume < targetVol)
        {
            musicSource.volume += targetVol * (Time.deltaTime / musicFadeTime);
            yield return null;
        }

        musicSource.volume = targetVol;
    }
}

// --- THE INVISIBLE HELPER SCRIPT ---
public class AutoSoundTrigger : MonoBehaviour
{
    private string soundToPlay;
    private bool isMusicTrack;
    private bool isReady = false;

    public void Initialize(string soundName, bool isMusic)
    {
        soundToPlay = soundName;
        isMusicTrack = isMusic;
        isReady = true;

        if (gameObject.activeInHierarchy && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(soundToPlay);
        }
    }

    // Plays when the screen turns ON
    void OnEnable()
    {
        if (isReady && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(soundToPlay);
        }
    }

    // Reverts the music when the screen turns OFF!
    void OnDisable()
    {
        if (isReady && isMusicTrack && AudioManager.instance != null)
        {
            AudioManager.instance.PlayDefaultMusic();
        }
    }
}