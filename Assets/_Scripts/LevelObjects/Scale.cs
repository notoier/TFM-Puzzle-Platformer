using UnityEngine;

public class ScaleController : MonoBehaviour
{
    [SerializeField] private WAPlatform leftPlatform;
    [SerializeField] private WAPlatform rightPlatform;

    [Header("Scale Settings")]
    [Tooltip("Diferencia de peso necesaria para llegar al desplazamiento mÃ¡ximo")]
    [SerializeField] private float requiredWeightDifference = 1f;

    private void Update()
    {
        if (!leftPlatform || !rightPlatform)
            return;

        float leftWeight = leftPlatform.CurrentWeight;
        float rightWeight = rightPlatform.CurrentWeight;

        float difference = leftWeight - rightWeight;

        float safeRequiredDifference = Mathf.Approximately(requiredWeightDifference, 0f)
            ? 0.01f
            : requiredWeightDifference;

        float normalized = Mathf.Clamp(difference / safeRequiredDifference, -1f, 1f);

        leftPlatform.SetSignedOffset(normalized);
        rightPlatform.SetSignedOffset(-normalized);
    }
}