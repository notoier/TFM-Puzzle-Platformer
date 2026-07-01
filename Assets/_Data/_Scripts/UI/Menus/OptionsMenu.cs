using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : UIPanel
{
    [Header("Sliders")]
    [SerializeField]
    private Slider masterSlider;
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;

    private SettingsManager settingsManager;

    private void Start()
    {
        settingsManager = SettingsManager.Instance;

        masterSlider.value = settingsManager.masterVolume;
        musicSlider.value = settingsManager.musicVolume;
        sfxSlider.value = settingsManager.effectsVolume;

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    private void OnMasterChanged(float value)
    {
        settingsManager.SetMasterVolume(value);
    }
    private void OnMusicChanged(float value)
    {
        settingsManager.SetMusicVolume(value);
    }
    private void OnSfxChanged(float value)
    {
        settingsManager.SetEffectsVolume(value);
    }

}
