using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class AudioManager : PersistentSingleton<AudioManager>
{
    public AudioSource _musicSource, _effectSource;
    public static bool isMusicLoop, isLoopActive;
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    public bool isSfxOn, isMusicOn;

    public void StopSound(string soundName)
    {
        if (audioSources.ContainsKey(soundName))
        {
            AudioSource source = audioSources[soundName];
            if (source != null)
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
        foreach (var pair in audioSources)
        {
            pair.Value.Stop();
        }
    }// Stops every sound found inside "audioSources" dictionary

    public void PlayEffect(AudioClip clip, Vector3 soundPosition = default, float volume = 1.0f, bool stop = false, float minPitch = 1f, float manPitch = 1f)
    {
        if (clip != null)
        {
            // Use the clip name as the key in the dictionary
            string soundName = clip.name;

            // Check if the sound is already in the dictionary
            if (!audioSources.ContainsKey(soundName))
            {
                // Create a new AudioSource, assign the clip, and add it to the dictionary
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                newSource.clip = clip;
                newSource.transform.position = soundPosition;
                audioSources.Add(soundName, newSource);
            }

            // Get the AudioSource from the dictionary
            AudioSource source = audioSources[soundName];

            source.volume = volume;

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
        }
    }// Plays a sound through "effectSource" audio source

    public void PlayMusic(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            // Use the clip name as the key in the dictionary
            string soundName = clip.name;

            // Check if the sound is already in the dictionary
            if (!audioSources.ContainsKey(soundName))
            {
                // Create a new AudioSource, assign the clip, and add it to the dictionary
                AudioSource newSource = gameObject.AddComponent<AudioSource>();
                if (isMusicLoop)
                {
                    newSource.loop = true;
                    isLoopActive = true;
                }
                newSource.clip = clip;
                audioSources.Add(soundName, newSource);
            }

            // Play the sound
            audioSources[soundName].Play();
            audioSources[soundName].volume = volume;
            if (!audioSources[soundName].isPlaying) isLoopActive = false;
        }
    }// Plays a sound through "musicSource" audio source

    public void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
    }// Changes master volume

    public void ToggleSFX()
    {
        _effectSource.mute = !_effectSource.mute;
        isSfxOn = !isSfxOn;
    }// Activates/deactivates effectSource

    public void ToggleMusic()
    {
        _musicSource.mute = !_musicSource.mute;
        isMusicOn = !isMusicOn;
    }// Activates/deactivates musicSource
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8)) StopAllSounds();
    }
}
