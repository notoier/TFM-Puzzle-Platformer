using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour, IActivable
{
    public bool IsActive { get; private set;  }
    
    [SerializeField] private GameObject doorClosed;
    [SerializeField] private GameObject doorOpen;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
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
        doorClosed.SetActive(true);
        doorOpen.SetActive(false);
        
        if (closeSound) AudioManager.Instance.PlayEffect(closeSound, this.transform.position);
    }

    private void Open()
    {
        doorClosed.SetActive(false);
        doorOpen.SetActive(true);
        
        if (openSound) AudioManager.Instance.PlayEffect(openSound, this.transform.position);
    }

    public void Deactivate()
    {
        Toggle();
    }
}
