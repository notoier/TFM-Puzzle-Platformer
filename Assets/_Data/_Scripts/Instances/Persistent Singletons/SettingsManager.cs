using System;
using UnityEngine;

public class SettingsManager : PersistentSingleton<SettingsManager>
{
    [Header ("Sound")]
    [SerializeField] public float masterVolume = 1f;
    [SerializeField] public float effectsVolume = 1f;
    [SerializeField] public float musicVolume = 1f;

    [Header("Resolution")]
    private Resolution[] resolutions;
    private int resolutionIndex;
    private int fullscreenIndex;


    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
        InitScreenData();
    }

    private void Start()
    {
        ApplySoundSettings();
        
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
        
        ApplySoundSettings();
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
        PlayerPrefs.Save();
        
        ApplySoundSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        
        ApplySoundSettings();
    }

    private void LoadSettings()
    {
        /* Sound Volume */
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", masterVolume);
        effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", effectsVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolume);

        /*Resolution ScreeMode*/
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        fullscreenIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
    }

    private void ApplySoundSettings()
    {
        AudioManager.Instance.UpdateSoundVolumes(masterVolume, effectsVolume, musicVolume);
    }

    /* ####################### *\
                DEBUG 
    \* ####################### */
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        effectsVolume = Mathf.Clamp01(effectsVolume);
        musicVolume = Mathf.Clamp01(musicVolume);

        if (!Application.isPlaying)
            return;

        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.UpdateSoundVolumes(masterVolume, effectsVolume, musicVolume);
    }
#endif

    //Resolutions

    private void SaveInt(string key, int value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public void InitScreenData()
    {
        resolutions = Screen.resolutions;
    }

    public Resolution[] GetResolutions()
    {
        return resolutions;
    }

    public int GetResolutionIndex()
    {
        return resolutionIndex;
    }

    public int GetFullscreenIndex()
    {
        return fullscreenIndex;
    }

    public void SetResolution(int index)
    {
        resolutionIndex=Math.Clamp(index,0, resolutions.Length-1);
        SaveInt("ResolutionIndex", resolutionIndex);

        ApplyScreenSettings();
    }

    public void SetFullscreen(int index)
    {
        fullscreenIndex = index;
        SaveInt("FullscreenIndex", fullscreenIndex);

        ApplyScreenSettings();
    }
    private void ApplyScreenSettings()
    {
        if (resolutions == null || resolutions.Length == 0)
            resolutions = Screen.resolutions;
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, GetFullScreenMode());
    }
    private FullScreenMode GetFullScreenMode()
    {
        return fullscreenIndex switch {
            0 => FullScreenMode.Windowed,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.FullScreenWindow
            };
    }
}
