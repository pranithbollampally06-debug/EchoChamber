using UnityEngine;
using UnityEngine.SceneManagement;

/// Every sound effect the game can play. Other scripts call AudioManager.Play(Sfx.X).
public enum Sfx { Jump, Land, RecordStart, RecordStop, GhostSpawn, PlateClick, DoorOpen, DoorClose, Death, LevelComplete }

/// Plays music and sound effects. Lives in the Title scene and survives scene loads.
/// While recording, the music is muffled and slowed, and a tick speeds up toward the 5s limit.
[RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.3f;   // kept low so SFX stay clear
    public float recordingCutoff = 900f;               // low-pass cutoff (Hz) while recording
    public float recordingPitch = 0.92f;               // music slows slightly while recording

    [Header("SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public AudioClip jump, land, recordStart, recordStop, ghostSpawn,
                     plateClick, doorOpen, doorClose, death, levelComplete;
    [Tooltip("Optional: ticks faster as the 5 second recording runs out")]
    public AudioClip tick;
    [Range(0f, 1f)] public float tickVolume = 0.5f;

    AudioSource musicSource, sfxSource;
    AudioLowPassFilter lowPass;
    bool recording;
    float nextTick;

    void Awake()
    {
        // A second manager (e.g. returning to the Title scene) removes itself
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;

        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
        if (music != null) musicSource.Play();

        // Separate child object so the low-pass only affects the music
        GameObject child = new GameObject("SFX");
        child.transform.SetParent(transform);
        sfxSource = child.AddComponent<AudioSource>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// A level reload mid-recording must not leave the music muffled.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => recording = false;

    /// Ease the music toward the muffled or normal sound.
    void Update()
    {
        float cutoff = recording ? recordingCutoff : 22000f;
        float pitch = recording ? recordingPitch : 1f;
        float t = 6f * Time.unscaledDeltaTime;
        lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, cutoff, t);
        musicSource.pitch = Mathf.Lerp(musicSource.pitch, pitch, t);
    }

    /// Play a sound effect. Safe to call when no AudioManager exists (it just stays silent).
    public static void Play(Sfx sfx, float volume = 1f)
    {
        if (Instance != null) Instance.PlayClip(sfx, volume);
    }

    /// Called by the recorder when recording starts or stops.
    public static void SetRecording(bool on)
    {
        if (Instance == null) return;
        Instance.recording = on;
        Instance.nextTick = 0f;
    }

    /// Called every tick while recording. progress is 0..1. Ticks get faster near the end.
    public static void Tick(float progress)
    {
        if (Instance == null || Instance.tick == null || Time.time < Instance.nextTick) return;
        Instance.nextTick = Time.time + Mathf.Lerp(0.5f, 0.08f, progress);
        Instance.sfxSource.PlayOneShot(Instance.tick, Instance.tickVolume * Instance.sfxVolume);
    }

    void PlayClip(Sfx sfx, float volume)
    {
        AudioClip clip = null;
        switch (sfx)
        {
            case Sfx.Jump: clip = jump; break;
            case Sfx.Land: clip = land; break;
            case Sfx.RecordStart: clip = recordStart; break;
            case Sfx.RecordStop: clip = recordStop; break;
            case Sfx.GhostSpawn: clip = ghostSpawn; break;
            case Sfx.PlateClick: clip = plateClick; break;
            case Sfx.DoorOpen: clip = doorOpen; break;
            case Sfx.DoorClose: clip = doorClose; break;
            case Sfx.Death: clip = death; break;
            case Sfx.LevelComplete: clip = levelComplete; break;
        }
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume * volume);
    }
}