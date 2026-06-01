using System;
using UnityEngine;
using System.Collections;

public class ScaleController : MonoBehaviour
{
    [SerializeField] private WeightedPlatform leftPlatform;
    [SerializeField] private WeightedPlatform rightPlatform;
    [SerializeField] private Transform centerPoint;

    [Header("Scale Settings")]
    [Tooltip("Diferencia de peso necesaria para llegar al desplazamiento máximo. " +
             "Si es 0, cualquier diferencia inclina al máximo.")]
    
    [SerializeField] private float requiredWeightDifference = 1f;

    [Tooltip("Distancia máxima permitida respecto al centro en el eje de movimiento")] [SerializeField]
    private float maxDistanceFromCenter = 3f;

    [Header("Delay Settings")]
    [Tooltip("Tiempo que espera la balanza antes de ajustarse cuando se coloca peso encima")]
    
    [SerializeField] private float loadDelay = 0.2f;

    [Tooltip("Tiempo que espera la balanza antes de restaurarse cuando se quita todo el peso")] [SerializeField]
    private float unloadDelay = 0.75f;

    private bool _wasLoaded;
    private bool _isWaitingAfterLoad;
    private bool _isWaitingAfterUnload;

    private Coroutine _loadDelayCoroutine;
    private Coroutine _unloadDelayCoroutine;
    
    [Header("Audio")]
    [SerializeField] private AudioClip tickingSound;
    [SerializeField] private float tickingSoundVolume;
    [SerializeField] private AudioClip timerEndSound;
    [SerializeField] private float timerEndSoundVolume;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private float lockedSoundVolume;
    [SerializeField] private AudioClip movingSound;
    [SerializeField] private float movingSoundVolume;

    [Header("Editor Setup")] 
    [SerializeField] private Grid grid;
    
    [SerializeField] private bool autoPlacePlatforms = true;
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private bool snapManualMovementToGrid = true;
    
    [OddRange(3, 15)]
    [SerializeField] private int horizontalSeparation = 3;

    [OddRange(-15, 15)]
    [SerializeField] private int verticalSeparation = 3;
    
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

        float normalized = CalculateTotalNormalizedTilt(leftWeight, rightWeight);

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

    private float CalculateTotalNormalizedTilt(float leftWeight, float rightWeight)
    {
        float difference = leftWeight - rightWeight;
        float absDiff = Math.Abs(difference);

        if (absDiff < requiredWeightDifference)
        {
            return 0f;
        }

        return Math.Sign(difference);
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

        if (unloadDelay >= 5 && tickingSound) AudioManager.Instance.StopSound(tickingSound.name);
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

        if (unloadDelay >= 5f && tickingSound) AudioManager.Instance.PlayEffect(tickingSound, centerPoint.position, tickingSoundVolume); 
        if (unloadDelay > 0f) yield return new WaitForSeconds(unloadDelay);
        AudioManager.Instance.StopSound(tickingSound.name);
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
    private Vector2 GetAllowedMovementRange(WeightedPlatform platform, Vector3 direction)
    {
        Vector3 centerToStart = platform.StartPosition - centerPoint.position;

        float startDistanceFromCenter = Vector3.Dot(centerToStart, direction);

        float minAllowedMovement = -maxDistanceFromCenter - startDistanceFromCenter;
        float maxAllowedMovement = maxDistanceFromCenter - startDistanceFromCenter;

        return new Vector2(minAllowedMovement, maxAllowedMovement);
    }
    
    
/* #################### *\    
    IN EDITOR SETTINGS     
\* #################### */


#if UNITY_EDITOR
    
    private Vector3 _lastLeftPosition;
    private Vector3 _lastRightPosition;
    private Vector3 _lastCenterPosition;

    private double _lastManualMoveTime;
    private bool _snapPending;

    [SerializeField] private float snapAfterMoveDelay = 0.25f;

    /// <summary>
    /// Detects manual movement in the editor and schedules a grid snap after movement stops.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            return;

        if (!snapManualMovementToGrid || !snapToGrid)
            return;

        if (!leftPlatform || !rightPlatform || !centerPoint)
            return;

        DetectManualMovement();
        TrySnapAfterManualMovement();
    }

    /// <summary>
    /// Checks if any scale part has been moved manually in the editor.
    /// </summary>
    private void DetectManualMovement()
    {
        if (leftPlatform.transform.position == _lastLeftPosition &&
            rightPlatform.transform.position == _lastRightPosition &&
            centerPoint.position == _lastCenterPosition) return;
        
        
        _lastManualMoveTime = UnityEditor.EditorApplication.timeSinceStartup;
        _snapPending = true;

        _lastLeftPosition = leftPlatform.transform.position;
        _lastRightPosition = rightPlatform.transform.position;
        _lastCenterPosition = centerPoint.position;
    }

    /// <summary>
    /// Snaps the scale to the grid once the user has stopped moving it for a short time.
    /// </summary>
    private void TrySnapAfterManualMovement()
    {
        if (!_snapPending)
            return;

        double elapsedTime = UnityEditor.EditorApplication.timeSinceStartup - _lastManualMoveTime;

        if (elapsedTime < snapAfterMoveDelay)
            return;

        SnapScalePartsToGrid();
        _snapPending = false;
    }

    /// <summary>
    /// Snaps the scale to the grid after manual movement.
    /// If auto placement is enabled, platforms are repositioned using the configured separations.
    /// Otherwise, each part is snapped independently.
    /// </summary>
    private void SnapScalePartsToGrid()
    {
        if (autoPlacePlatforms)
        {
            AutoPlacePlatforms();
        }
        else
        {
            leftPlatform.transform.position = SnapPlatformToGrid(
                leftPlatform.transform.position,
                CheckPlatformYCoordDeviation(leftPlatform.transform.position)
            );

            rightPlatform.transform.position = SnapPlatformToGrid(
                rightPlatform.transform.position,
                CheckPlatformYCoordDeviation(rightPlatform.transform.position)
            );

            centerPoint.position = SnapCenterPointToGrid(centerPoint.position);
        }

        _lastLeftPosition = leftPlatform.transform.position;
        _lastRightPosition = rightPlatform.transform.position;
        _lastCenterPosition = centerPoint.position;
    }
    
    /// <summary>
    /// Places both platforms around the center point using the configured separation values.
    /// </summary>
    private void AutoPlacePlatforms()
    {
        Vector3 center = snapToGrid
            ? SnapCenterPointToGrid(centerPoint.position)
            : centerPoint.position;

        centerPoint.position = center;

        Vector3 leftPosition = center + new Vector3(
            -horizontalSeparation * 0.5f,
            verticalSeparation * 0.5f,
            0f
        );

        Vector3 rightPosition = center + new Vector3(
            horizontalSeparation * 0.5f,
            -verticalSeparation * 0.5f,
            0f
        );

        if (snapToGrid)
        {
            leftPosition = SnapPlatformToGrid(leftPosition, CheckPlatformYCoordDeviation(leftPosition));
            rightPosition = SnapPlatformToGrid(rightPosition, CheckPlatformYCoordDeviation(rightPosition));
        }

        leftPlatform.transform.position = leftPosition;
        rightPlatform.transform.position = rightPosition;
    }
    
    /// <summary>
    /// Automatically places both platforms around the center point while editing values in the Inspector,
    /// or snaps them to the grid when they are moved manually.
    /// </summary>
    private void OnValidate()
    {
        AutoAssignGrid();
        
        if (!leftPlatform || !rightPlatform || !centerPoint) return;

        if (autoPlacePlatforms) AutoPlacePlatforms();
    }
    
    /// <summary>
    /// Automatically assigns the world grid to this object
    /// </summary>
    private void AutoAssignGrid()
    {
        if (grid != null)
            return;

    #if UNITY_2023_1_OR_NEWER
        grid = FindFirstObjectByType<Grid>();
    #else
        grid = FindObjectOfType<Grid>();
    #endif
    }

    /// <summary>
    /// Checks whether a platform is above or below the center returning the necessary deviation to align it vertically
    /// </summary>
    /// <param name="platformPosition"> A platforms position to compare </param>
    /// <returns></returns>
    private float CheckPlatformYCoordDeviation(Vector3 platformPosition)
    {
        return platformPosition.y > centerPoint.position.y ? -0.5f : 0.5f ;
    }

    /// <summary>
    /// Snaps a position to the center of the nearest grid cell.
    /// Useful for the scale center point.
    /// </summary>
    private Vector3 SnapCenterPointToGrid(Vector3 position)
    {
        if (!grid)
            return position;

        Vector3 cellSize = grid.cellSize;

        float safeGridX = Mathf.Approximately(cellSize.x, 0f) ? 1f : cellSize.x;
        float safeGridY = Mathf.Approximately(cellSize.y, 0f) ? 1f : cellSize.y;

        position.x = Mathf.Floor(position.x / safeGridX) * safeGridX + safeGridX * 0.5f;
        position.y = Mathf.Floor(position.y / safeGridY) * safeGridY + safeGridY * 0.5f;

        return position;
    }

    /// <summary>
    /// Snaps a platform position to the nearest grid line.
    /// Useful for platforms with even-sized tile widths.
    /// </summary>
    private Vector3 SnapPlatformToGrid(Vector3 position, float deviation)
    {
        if (!grid)
            return position;

        Vector3 cellSize = grid.cellSize;

        float safeGridX = Mathf.Approximately(cellSize.x, 0f) ? 1f : cellSize.x;
        float safeGridY = Mathf.Approximately(cellSize.y, 0f) ? 1f : cellSize.y;

        position.x = Mathf.Round(position.x / safeGridX) * safeGridX;
        position.y = (Mathf.Round(position.y / safeGridY) * safeGridY) + deviation;

        return position;
    }
#endif
    
}