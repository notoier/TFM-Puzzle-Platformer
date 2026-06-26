using UnityEngine;

public class WindController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip windSound;
    [SerializeField] private float windVolume;

    private void Start()
    {
        if (windSound) AudioManager.Instance?.PlayLoopEffect(windSound, transform, windVolume);
    }
}
