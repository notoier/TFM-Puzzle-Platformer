using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 
/// </summary>
public class AudioManager : PersistentSingleton<AudioManager>
{
    public AudioSource musicSource, effectSource;
    public static bool IsMusicLoop, IsLoopActive;
    private readonly Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();
    public bool isSfxOn, isMusicOn;
    
    [Header("Distant Volume")]
    [SerializeField] private AnimationCurve distanceVolumeCurve;
    [SerializeField] private float maxAudibleDistance = 12f;
    [SerializeField, Range(0f, 1f)] private float minimumVolume = 0.1f;
    
    private readonly Dictionary<AudioSource, float> _baseEffectVolumes = new();
    private float _masterVolume, _effectVolume, _musicVolume;
    
    [Header("Debug")]
    [SerializeField] private AudioClip debugSound;
    [SerializeField] private AudioClip debugMusic;
    [SerializeField] private bool playDebugEffect;
    [SerializeField] private bool playDebugMusic;

    private void Start()
    {
        if (playDebugEffect && debugSound) PlayLoopEffect(debugSound, transform);
        if (playDebugMusic &&  debugMusic) PlayMusic(debugMusic);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StopSound(AudioClip clip, Transform emitter)
    {
        if (!clip || !emitter)
            return;

        string key = $"{emitter.GetInstanceID()}_{clip.name}";

        if (_audioSources.TryGetValue(key, out AudioSource source) && source)
        {
            source.Stop();
            source.loop = false;
        }
    }
    public void StopAllSounds()
    {
        foreach (var pair in _audioSources)
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

        _baseEffectVolumes[source] = volume;
        
        float distanceVolume =
            GetDistanceVolume(emitter.position);

        source.loop = false;
        source.volume =
            _masterVolume *
            _effectVolume *
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
            _masterVolume *
            _effectVolume *
            volume *
            distanceVolume;
        
        _baseEffectVolumes[source] = volume;

        if (!source.isPlaying)
            source.Play();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private AudioSource GetOrCreateAudioSource(
        AudioClip clip,
        Transform emitter)
    {
        string key = $"{emitter.GetInstanceID()}_{clip.name}";

        if (_audioSources.TryGetValue(
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

        _audioSources[key] = newSource;

        return newSource;
    }


    public void PlayMusic(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;

        // Use the clip name as the key in the dictionary
        string soundName = clip.name;

        // Check if the sound is already in the dictionary
        if (!_audioSources.ContainsKey(soundName))
        {
            // Create a new AudioSource, assign the clip, and add it to the dictionary
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            if (IsMusicLoop)
            {
                newSource.loop = true;
                IsLoopActive = true;
            }
            newSource.clip = clip;
            _audioSources.Add(soundName, newSource);
        }

        // Play the sound
        _audioSources[soundName].Play();
        _audioSources[soundName].volume = _masterVolume * _musicVolume * volume;
        if (!_audioSources[soundName].isPlaying) IsLoopActive = false;
    }// Plays a sound through "musicSource" audio source
    
    public void ToggleSFX()
    {
        effectSource.mute = !effectSource.mute;
        isSfxOn = !isSfxOn;
    }// Activates/deactivates effectSource

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        isMusicOn = !isMusicOn;
    }// Activates/deactivates musicSource
    private void Update()
    {
        List<AudioSource> finishedSources = null;

        foreach ((AudioSource source, float baseVolume) in _baseEffectVolumes)
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
                _masterVolume *
                _effectVolume *
                baseVolume *
                distanceVolume;
        }

        if (finishedSources == null)
            return;

        foreach (AudioSource source in finishedSources)
            _baseEffectVolumes.Remove(source);
    }
    
    public void UpdateSoundVolumes(float masterVolume, float effectVolume, float musicVolume)
    {
        _masterVolume = masterVolume;
        _effectVolume = effectVolume;
        _musicVolume = musicVolume;
    }
}
