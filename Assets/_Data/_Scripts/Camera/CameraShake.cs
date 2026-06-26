using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    public enum ShakeType
    {
        Small,
        Medium,
        Large,
        Massive
    }

    [System.Serializable]
    private struct Shakepreset
    {
        public float amplitude;
        public float frequency;
        public float duration;
    }

    [Header("Presets")]
    [SerializeField]
    private Shakepreset small = new Shakepreset { amplitude = 1f, frequency = 2f, duration = 0.1f };
    [SerializeField]
    private Shakepreset medium = new Shakepreset { amplitude = 2f, frequency = 2.5f, duration = 0.2f };
    [SerializeField]
    private Shakepreset large = new Shakepreset { amplitude = 3.5f, frequency = 3f, duration = 0.35f };
    [SerializeField]
    private Shakepreset massive = new Shakepreset { amplitude = 5f, frequency = 2f, duration = 0.6f };


    [SerializeField] private CinemachineCamera cinemachineCamera;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeRoutine;
    private float currentAmplitude;

    private void Awake()
    {
        Instance = this;
        noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(ShakeType type)
    {
        Shakepreset preset = GetPreset(type);

        if (preset.amplitude < currentAmplitude)
            return;
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(preset));
    }

    private Shakepreset GetPreset(ShakeType type)
    {
        return type switch
        {
            ShakeType.Small => small,
            ShakeType.Medium => medium,
            ShakeType.Large => large,
            ShakeType.Massive => massive,
            _ => small
        };
    }

    private IEnumerator ShakeRoutine(Shakepreset preset)
    {
        currentAmplitude = preset.amplitude;

        noise.AmplitudeGain = preset.amplitude;
        noise.FrequencyGain = preset.frequency;

        yield return new WaitForSeconds(preset.duration);

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        currentAmplitude = 0f;
        shakeRoutine = null;


    }

}
