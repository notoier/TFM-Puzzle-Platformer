using System;
using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    [SerializeField] private WAPlatform leftPlatform;
    [SerializeField] private WAPlatform rightPlatform;
    [SerializeField] private Transform centerPoint;

    [Header("Scale Settings")]
    [Tooltip("Diferencia de peso necesaria para llegar al desplazamiento máximo. Si es 0, cualquier diferencia inclina al máximo.")]
    [SerializeField] private float requiredWeightDifference = 1f;

    [Tooltip("Distancia máxima permitida respecto al centro en el eje de movimiento")]
    [SerializeField] private float maxDistanceFromCenter = 2f;

    [Header("Delay Settings")]
    [Tooltip("Tiempo que espera la balanza antes de ajustarse al quitar todo el peso")]
    [SerializeField] private float unloadDelay = 0.15f;

    private bool _wasLoaded;
    private bool _isWaitingAfterUnload;
    private Coroutine _unloadDelayCoroutine;

    
    /// <summary>
    /// Updates the scale behavior every frame by reading the weight on both platforms,
    /// calculating the resulting tilt, applying unload delay when needed,
    /// and moving both platforms using a shared balanced offset.
    /// </summary>
    private void Update()
    {
        if (!leftPlatform || !rightPlatform || !centerPoint)
            return;

        float leftWeight = leftPlatform.CurrentWeight;
        float rightWeight = rightPlatform.CurrentWeight;
        float totalWeight = leftWeight + rightWeight;

        bool isLoaded = totalWeight > 0f;

        if (_wasLoaded && !isLoaded)
        {
            StartUnloadDelay();
        }

        _wasLoaded = isLoaded;

        if (_isWaitingAfterUnload)
            return;

        float difference = leftWeight - rightWeight;

        float normalized;

        if (Mathf.Approximately(requiredWeightDifference, 0f))
        {
            normalized = Mathf.Approximately(difference, 0f)
                ? 0f
                : Mathf.Sign(difference);
        }
        else
        {
            normalized = Mathf.Clamp(difference / requiredWeightDifference, -1f, 1f);
        }

        ApplyBalancedOffset(normalized);
    }

    
        
    
    /// <summary>
    /// Starts the delay coroutine for when there's a weight change
    ///</summary>
    private void StartUnloadDelay()
    {
        if (_unloadDelayCoroutine != null)
            StopCoroutine(_unloadDelayCoroutine);

        _unloadDelayCoroutine = StartCoroutine(UnloadDelayRoutine());
    }

    
    
     /// <summary>
     /// Sets a delay for when there's a weight change in the scale
     ///</summary>
    private IEnumerator UnloadDelayRoutine()
    {
        _isWaitingAfterUnload = true;

        if (unloadDelay > 0f)
            yield return new WaitForSeconds(unloadDelay);

        _isWaitingAfterUnload = false;
        _unloadDelayCoroutine = null;
    }

    
    /// <summary>
    /// Applies a signed movement offset to the given platform while ensuring that
    /// the resulting target position does not exceed the allowed distance from the
    /// scale center point along the platform's movement axis.
    /// </summary>
    ///
    /// <param name="platform">
    /// The platform that will receive the calculated target position.</param>
    ///
    /// <param name="signedOffset">
    /// Normalized signed offset used to move the platform.
    /// Positive values move it along its configured movement direction,
    /// negative values move it in the opposite direction.
    /// </param>

    [Obsolete]
    private void ApplyLimitedOffset(WAPlatform platform, float signedOffset)
    {
        Vector3 direction = platform.GetMovementDirectionVector();

        Vector3 centerToStart = platform.StartPosition - centerPoint.position;

        float startDistanceFromCenter = Vector3.Dot(centerToStart, direction);

        float desiredMovementFromStart = platform.MaxDistance * signedOffset;

        float minAllowedMovement = -maxDistanceFromCenter - startDistanceFromCenter;
        float maxAllowedMovement = maxDistanceFromCenter - startDistanceFromCenter;

        float limitedMovementFromStart = Mathf.Clamp(
            desiredMovementFromStart,
            minAllowedMovement,
            maxAllowedMovement
        );

        Vector3 targetPosition =
            platform.StartPosition + direction * limitedMovementFromStart;

        platform.SetTargetPosition(targetPosition);
    }
    
    /// <summary>
    /// Applies a balanced movement offset to both scale platforms.
    /// The movement is limited using the allowed range of both platforms,
    /// so if one platform reaches its limit, the other one also stops moving.
    /// </summary>
    /// <param name="normalizedOffset">
    /// Normalized scale offset between -1 and 1.
    /// Positive values move the left platform along its movement direction
    /// and the right platform in the opposite direction.
    /// </param>
    private void ApplyBalancedOffset(float normalizedOffset)
    {
        Vector3 direction = leftPlatform.GetMovementDirectionVector();

        float desiredMovement = leftPlatform.MaxDistance * normalizedOffset;

        Vector2 leftAllowedRange = GetAllowedMovementRange(leftPlatform, direction);
        Vector2 rightAllowedRange = GetAllowedMovementRange(rightPlatform, direction);

        float minSharedMovement = Mathf.Max(leftAllowedRange.x, -rightAllowedRange.y);
        float maxSharedMovement = Mathf.Min(leftAllowedRange.y, -rightAllowedRange.x);

        float limitedMovement = Mathf.Clamp(
            desiredMovement,
            minSharedMovement,
            maxSharedMovement
        );

        leftPlatform.SetTargetPosition(
            leftPlatform.StartPosition + direction * limitedMovement
        );

        rightPlatform.SetTargetPosition(
            rightPlatform.StartPosition - direction * limitedMovement
        );
    }
    
    
    
    /// <summary>
    /// Calculates how far the given platform is allowed to move from its start position
    /// without exceeding the maximum distance from the scale center point along the
    /// selected movement direction.
    /// </summary>
    /// <param name="platform">Platform whose movement range will be calculated.</param>
    /// <param name="direction">Movement direction used as the scale axis.</param>
    /// <returns>
    /// A Vector2 where x is the minimum allowed movement and y is the maximum allowed movement.
    /// </returns>
    private Vector2 GetAllowedMovementRange(WAPlatform platform, Vector3 direction)
    {
        Vector3 centerToStart = platform.StartPosition - centerPoint.position;

        float startDistanceFromCenter = Vector3.Dot(centerToStart, direction);

        float minAllowedMovement = -maxDistanceFromCenter - startDistanceFromCenter;
        float maxAllowedMovement = maxDistanceFromCenter - startDistanceFromCenter;

        return new Vector2(minAllowedMovement, maxAllowedMovement);
    }
}