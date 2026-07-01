using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;

public class ScreenSettingsUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown resolutionDropDown;
    [SerializeField]
    private TMP_Dropdown fullscreenDropDown;

    private SettingsManager settingsManager;

    private void Start()
    {
        settingsManager = SettingsManager.Instance;
        SetupResolutionDropdown();
        SetupFullscreenDropdown();

    }

    private void SetupFullscreenDropdown()
    {
        fullscreenDropDown.onValueChanged.AddListener(settingsManager.SetFullscreen);
        fullscreenDropDown.value = settingsManager.GetFullscreenIndex();
        fullscreenDropDown.RefreshShownValue();
    }

    private void SetupResolutionDropdown()
    {
        var resolutions = settingsManager.GetResolutions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + "x" + resolutions[i].height);
            resolutionDropDown.ClearOptions();
            resolutionDropDown.AddOptions(options);
            resolutionDropDown.value = settingsManager.GetResolutionIndex();
            resolutionDropDown.RefreshShownValue();

            resolutionDropDown.onValueChanged.AddListener(settingsManager.SetResolution);
        }
    }


}
