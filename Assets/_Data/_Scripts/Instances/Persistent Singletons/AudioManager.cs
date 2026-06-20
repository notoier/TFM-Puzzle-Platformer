using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// </summary>
public class AudioManager : PersistentSingleton<AudioManager>
{
    private readonly Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    
    [Header("General Audio")]
    [SerializeField] private bool isSfxOn = true;
    [SerializeField] private bool isMusicOn = true;

    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float effectVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        
    [Header("Distant Volume")]
    [SerializeField] private AnimationCurve distanceVolumeCurve;
    [SerializeField] private float maxAudibleDistance = 12f;
    [SerializeField, Range(0f, 1f)] private float minimumVolume = 0.1f;
    
    private readonly Dictionary<AudioSource, float> baseEffectVolumes = new();
    private float activeMusicBaseVolume = 1f;
    
    [Header("Music Fade")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private float defaultMusicFadeDuration = 1.5f;

    private AudioSource activeMusicSource;
    private Coroutine musicFadeCoroutine;
    
    [Header("Debug")]
    [SerializeField] private AudioClip debugSound;
    [SerializeField] private AudioClip debugMusic;
    [SerializeField] private bool playDebugEffect;
    [SerializeField] private bool playDebugMusic;

    protected override void Awake()
    {
        base.Awake();
        if (!musicSourceA || !musicSourceB)
        {
            Debug.LogError(
                "AudioManager: musicSourceA y musicSourceB deben estar asignados.",
                this
            );

            return;
        }

        activeMusicSource = musicSourceA;

        ConfigureMusicSource(musicSourceA);
        ConfigureMusicSource(musicSourceB);
    }

    private void Start()
    {
        if (playDebugEffect && debugSound)
            PlayLoopEffect(debugSound, transform);

        if (playDebugMusic && debugMusic)
            PlayMusic(debugMusic);
    }

    private void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.mute = !isMusicOn;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StopSound(AudioClip clip, Transform emitter)
    {
        if (!clip || !emitter)
            return;

        string key = $"{emitter.GetInstanceID()}_{clip.name}";

        if (audioSources.TryGetValue(key, out AudioSource source) && source)
        {
            source.Stop();
            source.loop = false;
        }
    }
    public void StopAllSounds()
    {
        foreach (var pair in audioSources)
        {
            pair.Value.Stop();
        }
    }// Stops every sound found inside "audioSources" dictionary

    private float GetDistanceVolume(Vector3 soundPosition)
    {
        float distance =
            GameController.Instance.GetDistanceToPlayer(soundPosition);

        float normalizedDistance = Mathf.Clamp01(
            distance / maxAudibleDistance
        );

        float curveValue = Mathf.Clamp01(
            distanceVolumeCurve.Evaluate(normalizedDistance)
        );

        float result = Mathf.Lerp(
            minimumVolume,
            1f,
            curveValue
        );
        
        return result;
    }
    
    public void PlayEffect(
        AudioClip clip,
        Transform emitter,
        float volume = 1f,
        float minPitch = 1f,
        float maxPitch = 1f)
    {
        if (!clip || !isSfxOn || !emitter)
            return;

        AudioSource source = GetOrCreateAudioSource(clip, emitter);

        baseEffectVolumes[source] = volume;
        
        float distanceVolume =
            GetDistanceVolume(emitter.position);

        source.loop = false;
        source.volume =
            masterVolume *
            effectVolume *
            volume *
            distanceVolume;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip);
    }

    public void PlayLoopEffect(
        AudioClip clip,
        Transform emitter,
        float volume = 1f,
        float minPitch = 1f,
        float maxPitch = 1f)
    {
        if (!clip || !isSfxOn || !emitter)
            return;

        AudioSource source = GetOrCreateAudioSource(clip, emitter);

        source.clip = clip;
        source.loop = true;
        source.pitch = Random.Range(minPitch, maxPitch);

        float distanceVolume =
            GetDistanceVolume(emitter.position);

        source.volume =
            masterVolume *
            effectVolume *
            volume *
            distanceVolume;
        
        baseEffectVolumes[source] = volume;

        if (!source.isPlaying)
            source.Play();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private AudioSource GetOrCreateAudioSource(AudioClip clip, Transform emitter)
    {
        string key = $"{emitter.GetInstanceID()}_{clip.name}";

        if (audioSources.TryGetValue(
                key,
                out AudioSource existingSource) &&
            existingSource)
        {
            return existingSource;
        }

        AudioSource newSource =
            emitter.gameObject.AddComponent<AudioSource>();

        newSource.clip = clip;
        newSource.playOnAwake = false;
        newSource.spatialBlend = 0f;

        audioSources[key] = newSource;

        return newSource;
    }
    
    public void PlayMusic(AudioClip newClip, float volume = 1f, float fadeDuration = -1f)
    {
        activeMusicBaseVolume = volume;
        
        if (!newClip)
            return;

        if (fadeDuration < 0f)
            fadeDuration = defaultMusicFadeDuration;

        if (activeMusicSource &&
            activeMusicSource.clip == newClip &&
            activeMusicSource.isPlaying)
        {
            return;
        }

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicFadeCoroutine = StartCoroutine(
            CrossFadeMusic(newClip, volume, fadeDuration)
        );
    }
    
    public void ToggleSfx()
    {
        isSfxOn = !isSfxOn;

        foreach (AudioSource source in audioSources.Values)
        {
            if (source)
                source.mute = !isSfxOn;
        }
    }
    
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;

        if (musicSourceA)
            musicSourceA.mute = !isMusicOn;

        if (musicSourceB)
            musicSourceB.mute = !isMusicOn;
    }
    
    private void Update()
    {
        List<AudioSource> finishedSources = null;

        foreach ((AudioSource source, float baseVolume) in baseEffectVolumes)
        {
            if (!source || !source.isPlaying)
            {
                finishedSources ??= new List<AudioSource>();
                finishedSources.Add(source);
                continue;
            }

            float distanceVolume =
                GetDistanceVolume(source.transform.position);

            source.volume =
                masterVolume *
                effectVolume *
                baseVolume *
                distanceVolume;
        }

        if (finishedSources == null)
            return;

        foreach (AudioSource source in finishedSources)
            baseEffectVolumes.Remove(source);
    }
    
    public void UpdateSoundVolumes(float newMasterVolume, float newEffectVolume, float newMusicVolume)
    {
        masterVolume = newMasterVolume;
        effectVolume = newEffectVolume;
        musicVolume = newMusicVolume;

        if (musicFadeCoroutine == null &&
            activeMusicSource &&
            activeMusicSource.isPlaying)
        {
            activeMusicSource.volume =
                masterVolume *
                musicVolume *
                activeMusicBaseVolume;
        }
    }
    
    private IEnumerator CrossFadeMusic(
        AudioClip newClip,
        float targetVolume,
        float duration)
    {
        AudioSource oldSource = GetLoudestMusicSource();

        AudioSource newSource =
            oldSource == musicSourceA
                ? musicSourceB
                : musicSourceA;

        // La fuente secundaria podría seguir reproduciendo un fade anterior.
        newSource.Stop();
        newSource.clip = newClip;
        newSource.loop = true;
        newSource.volume = 0f;
        newSource.mute = !isMusicOn;
        newSource.Play();

        float oldStartVolume =
            oldSource && oldSource.isPlaying
                ? oldSource.volume
                : 0f;

        float finalVolume =
            masterVolume *
            musicVolume *
            targetVolume;

        if (duration <= 0f)
        {
            StopAndClearMusicSource(oldSource);

            newSource.volume = finalVolume;
            activeMusicSource = newSource;
            musicFadeCoroutine = null;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            if (oldSource)
            {
                oldSource.volume = Mathf.Lerp(
                    oldStartVolume,
                    0f,
                    progress
                );
            }

            newSource.volume = Mathf.Lerp(
                0f,
                finalVolume,
                progress
            );

            yield return null;
        }

        StopAndClearMusicSource(oldSource);

        newSource.volume = finalVolume;
        activeMusicSource = newSource;
        musicFadeCoroutine = null;
    }
    
    private AudioSource GetLoudestMusicSource()
    {
        bool aIsPlaying = musicSourceA && musicSourceA.isPlaying;
        bool bIsPlaying = musicSourceB && musicSourceB.isPlaying;

        if (aIsPlaying && bIsPlaying)
        {
            return musicSourceA.volume >= musicSourceB.volume
                ? musicSourceA
                : musicSourceB;
        }

        if (aIsPlaying)
            return musicSourceA;

        if (bIsPlaying)
            return musicSourceB;

        return activeMusicSource
            ? activeMusicSource
            : musicSourceA;
    }

    private static void StopAndClearMusicSource(AudioSource source)
    {
        if (!source)
            return;

        source.Stop();
        source.clip = null;
        source.volume = 0f;
    }
}
