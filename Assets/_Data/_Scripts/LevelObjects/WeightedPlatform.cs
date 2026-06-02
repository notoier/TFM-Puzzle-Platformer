using UnityEngine;
using System.Collections;

public class WeightedPlatform : MonoBehaviour, IDetectsWeight
{
    private enum MovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private enum ControlMode
    {
        Independent,
        External
    }

    public float CurrentWeight { get; set; }

    public Vector3 StartPosition => _startPosition;
    public float MaxDistance => config.maxDistance;

    [Header("Config")] 
    [SerializeField] private WeightedPlatformConfig config;

    [Header("Behaviour")] [SerializeField] private MovementDirection movementDirection = MovementDirection.Down;
    [SerializeField] private ControlMode controlMode = ControlMode.Independent;

    [Header("Scale Settings")]
    [Tooltip("Desplazamiento máximo relativo en modo externo. Normalmente 1.")]
    [SerializeField] private float maxExternalOffset = 1f;

    [Header("Colliders")]
    [SerializeField] private BoxCollider2D solidCollider;
    [SerializeField] private BoxCollider2D weightTriggerCollider;
    
    [Header("Audio")]
    [SerializeField] private AudioClip movingSound;
    [SerializeField] private float movingSoundVolume;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private float lockedSoundVolume;
    private bool _isMovingSoundPlaying;
    
    
    private Vector3 _startPosition;
    private Vector3 _currentTargetPosition;
    private Coroutine _movementCoroutine;

    private bool _hasTarget;
    private bool _isReturning;
    private bool _hasCompletedForwardMove;
    private bool _isWaitingAtStart;
    
    /// <summary>
    /// Stores the platform's initial position and initializes its first target.
    /// </summary>
    private void Awake()
    {
        _startPosition = transform.position;
        _currentTargetPosition = _startPosition;
        _hasTarget = true;
    }

    /// <summary>
    /// Handles automatic movement when the platform is working independently.
    /// In external mode, movement is controlled by another script such as a ScaleController.
    /// </summary>
    private void Update()
    {
        if (controlMode != ControlMode.Independent || _isReturning || _isWaitingAtStart)
            return;

        float normalizedOffset = CalculateNormalizedOffsetFromWeight();

        if (!config.recovers && normalizedOffset <= 0f && CurrentWeight < config.requiredWeight)
            return;

        SetNormalizedOffset(normalizedOffset);
    }

    /// <summary>
    /// Moves the platform using a normalized offset between 0 and 1.
    /// This is mainly used by independent platforms.
    /// </summary>
    /// <param name="normalizedOffset">
    /// Value between 0 and 1. 
    /// 0 means the start position and 1 means the maximum movement distance.
    /// </param>
    public void SetNormalizedOffset(float normalizedOffset)
    {
        if (controlMode == ControlMode.Independent && _isReturning)
            return;

        normalizedOffset = Mathf.Clamp01(normalizedOffset);

        float currentNormalizedOffset = GetCurrentNormalizedOffset();

        if (!config.recovers && normalizedOffset < currentNormalizedOffset)
            return;

        Vector3 targetPosition =
            _startPosition + GetMovementDirectionVector() * (config.maxDistance * normalizedOffset);

        ApplyMovement(targetPosition, normalizedOffset >= 1f);
    }
    
    private float GetCurrentNormalizedOffset()
    {
        if (config.maxDistance <= 0f)
            return 0f;

        Vector3 direction = GetMovementDirectionVector();
        Vector3 fromStart = transform.position - _startPosition;

        float currentDistance = Vector3.Dot(fromStart, direction);

        return Mathf.Clamp01(currentDistance / config.maxDistance);
    }

    /// <summary>
    /// Moves the platform using a signed offset between -maxExternalOffset and maxExternalOffset.
    /// Positive values move the platform along its configured direction,
    /// while negative values move it in the opposite direction.
    /// </summary>
    /// <param name="signedOffset">Signed movement offset used by external controllers.</param>
    public void SetSignedOffset(float signedOffset)
    {
        signedOffset = Mathf.Clamp(signedOffset, -maxExternalOffset, maxExternalOffset);

        Vector3 targetPosition =
            _startPosition + GetMovementDirectionVector() * (config.maxDistance * signedOffset);

        ApplyMovement(targetPosition, false);
    }

    /// <summary>
    /// Moves the platform directly towards a specific world position.
    /// This is useful for external controllers that calculate their own target position.
    /// </summary>
    /// <param name="targetPosition">World position the platform should move towards.</param>
    public void SetTargetPosition(Vector3 targetPosition)
    {
        ApplyMovement(targetPosition, false);
    }

    /// <summary>
    /// Resets the platform movement state and moves it back to its initial position.
    /// </summary>
    public void ResetToStart()
    {
        _hasCompletedForwardMove = false;
        _isReturning = false;
        _isWaitingAtStart = false;

        _currentTargetPosition = _startPosition;
        _hasTarget = true;

        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        
        StopMovingSound();

        if (config.instantMovement)
        {
            transform.position = _startPosition;
            return;
        }

        StartSmoothMovement(_startPosition, false, false);
    }

    /// <summary>
    /// Converts the current weight on the platform into a normalized movement offset.
    /// </summary>
    /// <returns>
    /// A value between 0 and 1, where 0 means no movement and 1 means full movement.
    /// </returns>
    public float CalculateNormalizedOffsetFromWeight()
    {
        if (config.requiredWeight <= 0f)
            return 1f;

        return Mathf.Clamp01(CurrentWeight / config.requiredWeight);
    }

    /// <summary>
    /// Applies either instant or smooth movement towards the given target position.
    /// </summary>
    /// <param name="targetPosition">Target world position.</param>
    /// <param name="reachingEnd">
    /// True if this movement reaches the platform's maximum offset.
    /// Used to trigger automatic return behavior.
    /// </param>
    private void ApplyMovement(Vector3 targetPosition, bool reachingEnd)
    {
        if (config.instantMovement)
        {
            transform.position = targetPosition;
            _currentTargetPosition = targetPosition;
            _hasTarget = true;

            if (reachingEnd)
                OnReachedEnd();

            return;
        }

        bool alreadyAtTarget = Vector3.Distance(transform.position, targetPosition) < 0.01f;

        if (alreadyAtTarget)
        {
            _currentTargetPosition = targetPosition;
            _hasTarget = true;
            StopMovingSound();

            if (reachingEnd)
                OnReachedEnd();

            return;
        }

        bool sameTarget =
            _hasTarget && Vector3.Distance(_currentTargetPosition, targetPosition) < 0.01f;

        if (sameTarget && _movementCoroutine != null)
            return;

        _currentTargetPosition = targetPosition;
        _hasTarget = true;

        bool useDelay = controlMode == ControlMode.Independent;

        StartSmoothMovement(targetPosition, reachingEnd, useDelay);
    }

    /// <summary>
    /// Starts a smooth movement coroutine, stopping any previous movement first.
    /// </summary>
    /// <param name="targetPosition">Target world position.</param>
    /// <param name="reachingEnd">Whether the target is the end of the platform's path.</param>
    /// <param name="useDelay">Whether the configured movement delay should be applied.</param>
    private void StartSmoothMovement(Vector3 targetPosition, bool reachingEnd, bool useDelay)
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
        }
        
        _movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, reachingEnd, useDelay));
    }

    /// <summary>
    /// Smoothly moves the platform towards the target position.
    /// </summary>
    /// <param name="targetPosition">Target world position.</param>
    /// <param name="reachingEnd">Whether the movement reaches the end of the path.</param>
    /// <param name="useDelay">Whether to wait before starting the movement.</param>
    private IEnumerator MoveToPosition(Vector3 targetPosition, bool reachingEnd, bool useDelay)
    {
        if (useDelay && config.movementDelay > 0f)
            yield return new WaitForSeconds(config.movementDelay);

        if (Vector3.Distance(transform.position, targetPosition) <= 0.01f)
        {
            transform.position = targetPosition;
            _movementCoroutine = null;
            _currentTargetPosition = targetPosition;
            _hasTarget = true;

            if (reachingEnd)
                OnReachedEnd();

            yield break;
        }

        StartMovingSound();

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                config.movementSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;

        StopMovingSound();
        
        _movementCoroutine = null;
        _currentTargetPosition = targetPosition;
        _hasTarget = true;
        
        if (reachingEnd)
            OnReachedEnd();
    }

    /// <summary>
    /// Handles the logic executed when the platform reaches the end of its movement.
    /// If configured, the platform starts returning to its original position.
    /// </summary>
    private void OnReachedEnd()
    {
        if (_hasCompletedForwardMove)
            return;

        _hasCompletedForwardMove = true;
        
        
        PlayLockSound();

        if (config.returnAfterReachingEnd && controlMode == ControlMode.Independent)
        {
            _movementCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private void PlayLockSound()
    {
        if (lockedSound)
            AudioManager.Instance?.PlayEffect(lockedSound, transform.position, lockedSoundVolume);
    }

    /// <summary>
    /// Waits for the configured return delay, moves the platform back to its start position,
    /// and optionally waits again before allowing a new activation.
    /// </summary>
    private IEnumerator ReturnAfterDelay()
    {
        _isReturning = true;

        if (config.returnDelay > 0f)
            yield return new WaitForSeconds(config.returnDelay);

        StartMovingSound();
        
        while (Vector3.Distance(transform.position, _startPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _startPosition,
                config.movementSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = _startPosition;

        StopMovingSound();
        
        PlayLockSound();
        
        _isReturning = false;
        _hasCompletedForwardMove = false;
        _currentTargetPosition = _startPosition;
        _hasTarget = true;

        if (config.movementDelay > 0f)
        {
            _isWaitingAtStart = true;
            yield return new WaitForSeconds(config.movementDelay);
            _isWaitingAtStart = false;
        }

        _movementCoroutine = null;
    }

    /// <summary>
    /// Returns the world-space direction vector based on the selected movement direction.
    /// </summary>
    /// <returns>The movement direction as a Vector3.</returns>
    public Vector3 GetMovementDirectionVector()
    {
        return movementDirection switch
        {
            MovementDirection.Up => Vector3.up,
            MovementDirection.Down => Vector3.down,
            MovementDirection.Left => Vector3.left,
            MovementDirection.Right => Vector3.right,
            _ => Vector3.zero
        };
    }

    /// <summary>
    /// Registers the weight of an object currently affecting the platform.
    /// </summary>
    /// <param name="weightProvider">Object that provides weight.</param>
    public void RegisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight += weightProvider.Weight;
    }

    /// <summary>
    /// Registers a raw weight value on the platform.
    /// </summary>
    /// <param name="weight">Weight value to add.</param>
    public void RegisterWeight(float weight)
    {
        CurrentWeight += weight;
    }

    /// <summary>
    /// Removes the weight of an object that is no longer affecting the platform.
    /// </summary>
    /// <param name="weightProvider">Object that provides weight.</param>
    public void UnregisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight -= weightProvider.Weight;
    }

    /// <summary>
    /// Removes a raw weight value from the platform.
    /// </summary>
    /// <param name="weight">Weight value to remove.</param>
    public void UnregisterWeight(float weight)
    {
        CurrentWeight -= weight;
    }

    /// <summary>
    /// Checks whether the platform currently has enough weight to be fully activated.
    /// </summary>
    /// <returns>True if the current weight is greater than or equal to the required weight.</returns>
    public bool HasEnoughWeight() => CurrentWeight >= config.requiredWeight;

    /// <summary>
    /// Detects weighted objects entering the platform trigger area and registers their weight.
    /// </summary>
    /// <param name="other">Collider entering the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();

        if (weightProvider != null)
            RegisterWeight(weightProvider);
    }

    /// <summary>
    /// Detects weighted objects leaving the platform trigger area and unregisters their weight.
    /// </summary>
    /// <param name="other">Collider exiting the trigger.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();

        if (weightProvider != null)
            UnregisterWeight(weightProvider);
    }
    
    /// <summary>
    /// Updates the platform colliders to match the current visual size.
    /// </summary>
    /// <param name="platformSize">Final platform size in local units.</param>
    public void UpdateColliders(Vector2 platformSize)
    {
        if (solidCollider)
        {
            solidCollider.size = platformSize;
            solidCollider.offset = Vector2.zero;
        }

        if (!weightTriggerCollider) return;
        
        const float triggerHeight = 0.25f;

        weightTriggerCollider.size = new Vector2(platformSize.x, triggerHeight);
        weightTriggerCollider.offset = new Vector2(
            0f,
            platformSize.y * 0.5f + triggerHeight * 0.5f
        );
    }
    
    private void StartMovingSound()
    {
        if (_isMovingSoundPlaying || !movingSound)
            return;

        AudioManager.Instance?.PlayLoopEffect(movingSound, transform.position, movingSoundVolume);
        _isMovingSoundPlaying = true;
    }

    private void StopMovingSound()
    {
        if (!_isMovingSoundPlaying || !movingSound)
            return;

        AudioManager.Instance?.StopSound(movingSound.name);
        _isMovingSoundPlaying = false;
    }
}