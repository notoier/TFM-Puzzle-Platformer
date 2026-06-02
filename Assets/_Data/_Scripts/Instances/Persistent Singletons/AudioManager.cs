using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class AudioManager : PersistentSingleton<AudioManager>
{
    public AudioSource musicSource, effectSource;
    public static bool IsMusicLoop, IsLoopActive;
    private readonly Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();
    public bool isSfxOn, isMusicOn;
    
    private float _masterVolume, _effectVolume, _musicVolume;
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void StopSound(string soundName)
    {
        if (_audioSources.TryGetValue(soundName, out AudioSource source))
        {
            if (source)
            {
                source.Stop();
            }
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }// Stops sounds found inside "audioSources" dictionary when called
    public void StopAllSounds()
    {
        foreach (var pair in _audioSources)
        {
            pair.Value.Stop();
        }
    }// Stops every sound found inside "audioSources" dictionary

    public void PlayEffect(
        AudioClip clip,
        Vector3 soundPosition = default,
        float volume = 1.0f,
        float minPitch = 1f,
        float maxPitch = 1f)
    {
        if (!clip || !isSfxOn)
            return;

        AudioSource source = GetOrCreateAudioSource(clip, soundPosition);

        source.loop = false;
        source.transform.position = soundPosition;
        source.volume = _masterVolume * _effectVolume * volume;
        source.pitch = Random.Range(minPitch, maxPitch);

        source.PlayOneShot(clip);
    }

    public void PlayLoopEffect(
        AudioClip clip,
        Vector3 soundPosition = default,
        float volume = 1.0f,
        float minPitch = 1f,
        float maxPitch = 1f)
    {
        if (!clip || !isSfxOn)
            return;

        AudioSource source = GetOrCreateAudioSource(clip, soundPosition);

        source.clip = clip;
        source.loop = true;
        source.transform.position = soundPosition;
        source.volume = _masterVolume * _effectVolume * volume;
        source.pitch = Random.Range(minPitch, maxPitch);

        if (!source.isPlaying)
            source.Play();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private AudioSource GetOrCreateAudioSource(AudioClip clip, Vector3 soundPosition)
    {
        string soundName = clip.name;

        if (_audioSources.TryGetValue(soundName, out AudioSource existingSource) && existingSource)
            return existingSource;

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.transform.position = soundPosition;
        newSource.playOnAwake = false;

        _audioSources[soundName] = newSource;

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
        //if (Input.GetKeyDown(KeyCode.F8)) StopAllSounds();
    }
    
    public void UpdateSoundVolumes(float masterVolume, float effectVolume, float musicVolume)
    {
        _masterVolume = masterVolume;
        _effectVolume = effectVolume;
        _musicVolume = musicVolume;
    }
}
