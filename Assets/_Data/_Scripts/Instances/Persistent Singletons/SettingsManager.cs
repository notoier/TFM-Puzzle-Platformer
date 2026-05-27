using UnityEngine;

public class SettingsManager : PersistentSingleton<SettingsManager>
{
    [Header ("Sound")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float effectsVolume = 1f;
    [SerializeField] private float musicVolume = 1f;
    
    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
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
    
}
