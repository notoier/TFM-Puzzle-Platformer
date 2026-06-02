using UnityEngine;

public class Door : MonoBehaviour, IActivable
{
    public bool IsActive { get; private set;  }
    
    [SerializeField] private GameObject doorClosed;
    [SerializeField] private GameObject doorOpen;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private float openSoundVolume;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float closeSoundVolume;

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
        
        if (closeSound) AudioManager.Instance?.PlayEffect(closeSound, this.transform.position, closeSoundVolume);
    }

    private void Open()
    {
        doorClosed?.SetActive(false);
        doorOpen?.SetActive(true);
        
        if (openSound) AudioManager.Instance?.PlayEffect(openSound, this.transform.position, openSoundVolume);
    }

    public void Deactivate()
    {
        Toggle();
    }
}
