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

    public void PlayEffect(AudioClip clip, Vector3 soundPosition = default, float volume = 1.0f, bool stop = false, float minPitch = 1f, float manPitch = 1f)
    {
        if (!clip) return;
        
        
        // Use the clip name as the key in the dictionary
        string soundName = clip.name;

        // Check if the sound is already in the dictionary
        if (!_audioSources.ContainsKey(soundName))
        {
            // Create a new AudioSource, assign the clip, and add it to the dictionary
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.transform.position = soundPosition;
            _audioSources.Add(soundName, newSource);
        }

        // Get the AudioSource from the dictionary
        AudioSource source = _audioSources[soundName];

        source.volume = _masterVolume * _effectVolume * volume;
        source.pitch = Random.Range(minPitch, manPitch);

        if (stop)
        {
            source.Play();
        }
        else
        {
            // Play the clip as a one-shot
            source.PlayOneShot(clip);
        }
    }// Plays a sound through "effectSource" audio source

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
        if (Input.GetKeyDown(KeyCode.F8)) StopAllSounds();
    }
    
    public void UpdateSoundVolumes(float masterVolume, float effectVolume, float musicVolume)
    {
        _masterVolume = masterVolume;
        _effectVolume = effectVolume;
        _musicVolume = musicVolume;
    }
}
