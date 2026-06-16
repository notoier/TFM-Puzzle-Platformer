using System;
using UnityEngine;

public class Door : MonoBehaviour, IActivable
{
    public bool IsActive { get; private set;  }
    [Header("References")]
    [SerializeField] private GameObject doorClosed;
    [SerializeField] private GameObject doorOpen;

    [Header("Status")]
    [SerializeField] private bool startsOpen;
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private float openSoundVolume;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float closeSoundVolume;

    private void Awake()
    {
        if (!startsOpen) return;
        
        doorClosed.SetActive(false);
        doorOpen.SetActive(true);
        IsActive = true;
    }

    /* IActivable */
    public void Activate()
    {
        Toggle();
    }

    private void Toggle()
    {
        if (IsActive) Close();
        else Open();
        IsActive = !IsActive;
    }

    private void Close()
    {
        doorClosed?.SetActive(true);
        doorOpen?.SetActive(false);
        
        if (closeSound) AudioManager.Instance?.PlayEffect(closeSound, transform, closeSoundVolume);
    }

    private void Open()
    {
        doorClosed?.SetActive(false);
        doorOpen?.SetActive(true);
        
        if (openSound) AudioManager.Instance?.PlayEffect(openSound, transform, openSoundVolume);
    }

    public void Deactivate()
    {
        Toggle();
    }
}
