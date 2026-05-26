using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    [SerializeField] private WAPlatform leftPlatform;
    [SerializeField] private WAPlatform rightPlatform;
    [SerializeField] private Transform centerPoint;

    [Header("Scale Settings")]
    [Tooltip("Diferencia de peso necesaria para llegar al desplazamiento máximo. Si es 0, cualquier diferencia inclina al máximo.")]
    [SerializeField] private float requiredWeightDifference = 0f;

    [Tooltip("Distancia máxima permitida respecto al centro en el eje de movimiento")]
    [SerializeField] private float maxDistanceFromCenter = 3f;

    [Header("Delay Settings")]
    [Tooltip("Tiempo que espera la balanza antes de ajustarse cuando se coloca peso encima")]
    [SerializeField] private float loadDelay = 0.2f;

    [Tooltip("Tiempo que espera la balanza antes de restaurarse cuando se quita todo el peso")]
    [SerializeField] private float unloadDelay = 0.75f;

    private bool _wasLoaded;
    private bool _isWaitingAfterLoad;
    private bool _isWaitingAfterUnload;

    private Coroutine _loadDelayCoroutine;
    private Coroutine _unloadDelayCoroutine;

    /// <summary>
    /// Updates the scale behavior every frame by reading the weight on both platforms,
    /// applying load/unload delays when needed, and moving both platforms using a shared balanced offset.
    /// </summary>
    private void Update()
    {
        if (!leftPlatform || !rightPlatform || !centerPoint)
            return;

        float leftWeight = leftPlatform.CurrentWeight;
        float rightWeight = rightPlatform.CurrentWeight;
        float totalWeight = leftWeight + rightWeight;

        bool isLoaded = totalWeight > 0f;

        if (!_wasLoaded && isLoaded)
        {
            CancelUnloadDelay();
            StartLoadDelay();
        }

        if (_wasLoaded && !isLoaded)
        {
            CancelLoadDelay();
            StartUnloadDelay();
        }

        _wasLoaded = isLoaded;

        if (_isWaitingAfterLoad || _isWaitingAfterUnload)
            return;

        float normalized = CalculateNormalizedTilt(leftWeight, rightWeight);

        ApplyBalancedOffset(normalized);
    }

    /// <summary>
    /// Calculates the normalized tilt of the scale based on the weight difference
    /// between the left and right platforms.
    /// </summary>
    /// <param name="leftWeight">Current weight on the left platform.</param>
    /// <param name="rightWeight">Current weight on the right platform.</param>
    /// <returns>
    /// A value between -1 and 1. Positive values tilt towards the left platform,
    /// negative values tilt towards the right platform.
    /// </returns>
    private float CalculateNormalizedTilt(float leftWeight, float rightWeight)
    {
        float difference = leftWeight - rightWeight;

        if (Mathf.Approximately(requiredWeightDifference, 0f))
        {
            return Mathf.Approximately(difference, 0f)
                ? 0f
                : Mathf.Sign(difference);
        }

        return Mathf.Clamp(difference / requiredWeightDifference, -1f, 1f);
    }

    /// <summary>
    /// Starts the delay used when weight is placed on the scale.
    /// </summary>
    private void StartLoadDelay()
    {
        if (_loadDelayCoroutine != null)
            StopCoroutine(_loadDelayCoroutine);

        _loadDelayCoroutine = StartCoroutine(LoadDelayRoutine());
    }

    /// <summary>
    /// Starts the delay used when all weight is removed from the scale.
    /// </summary>
    private void StartUnloadDelay()
    {
        if (_unloadDelayCoroutine != null)
            StopCoroutine(_unloadDelayCoroutine);

        _unloadDelayCoroutine = StartCoroutine(UnloadDelayRoutine());
    }

    /// <summary>
    /// Cancels the load delay if it is currently running.
    /// </summary>
    private void CancelLoadDelay()
    {
        if (_loadDelayCoroutine == null)
            return;

        StopCoroutine(_loadDelayCoroutine);
        _loadDelayCoroutine = null;
        _isWaitingAfterLoad = false;
    }

    /// <summary>
    /// Cancels the unload delay if it is currently running.
    /// </summary>
    private void CancelUnloadDelay()
    {
        if (_unloadDelayCoroutine == null)
            return;

        StopCoroutine(_unloadDelayCoroutine);
        _unloadDelayCoroutine = null;
        _isWaitingAfterUnload = false;
    }

    /// <summary>
    /// Waits before allowing the scale to react after weight is placed on it.
    /// </summary>
    private IEnumerator LoadDelayRoutine()
    {
        _isWaitingAfterLoad = true;

        if (loadDelay > 0f)
            yield return new WaitForSeconds(loadDelay);

        _isWaitingAfterLoad = false;
        _loadDelayCoroutine = null;
    }

    /// <summary>
    /// Waits before allowing the scale to restore itself after all weight is removed.
    /// </summary>
    private IEnumerator UnloadDelayRoutine()
    {
        _isWaitingAfterUnload = true;

        if (unloadDelay > 0f)
            yield return new WaitForSeconds(unloadDelay);

        _isWaitingAfterUnload = false;
        _unloadDelayCoroutine = null;
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