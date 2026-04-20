using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WAPlatform : MonoBehaviour, IDetectsWeight
{
    public enum MovementDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum ControlMode
    {
        Independent,
        External
    }

    public float CurrentWeight { get; set; }

    [Header("Config")]
    [SerializeField] private WAPlatformConfig config;

    [Header("Behaviour")]
    [SerializeField] private MovementDirection movementDirection = MovementDirection.Down;
    [SerializeField] private ControlMode controlMode = ControlMode.Independent;

    [Header("Scale Settings")]
    [Tooltip("Desplazamiento máximo respecto a la posición inicial cuando la plataforma está controlada externamente")]
    [SerializeField] private float maxExternalOffset = 1f;

    private Vector3 _startPosition;
    private Vector3 _currentTargetPosition;
    private Coroutine _movementCoroutine;

    private bool _hasTarget;
    private bool _isReturning;
    private bool _hasCompletedForwardMove;
    private bool _isWaitingAtStart;
    
    //private List<IProvidesWeight> _weightProviders = new  List<IProvidesWeight>();
    
    private void Awake()
    {
        _startPosition = transform.position;
        _currentTargetPosition = _startPosition;
        _hasTarget = true;
    }

    private void Update()
    {
        if (controlMode != ControlMode.Independent || _isReturning || _isWaitingAtStart)
            return;

        float normalizedOffset = CalculateNormalizedOffsetFromWeight();

        if (!config.recovers && normalizedOffset <= 0f && CurrentWeight < config.requiredWeight)
            return;

        SetNormalizedOffset(normalizedOffset);
    }

    public void SetNormalizedOffset(float normalizedOffset)
    {
        if (controlMode == ControlMode.Independent && _isReturning)
            return;

        normalizedOffset = Mathf.Clamp01(normalizedOffset);

        Vector3 targetPosition = _startPosition + GetDirectionVector() * (config.maxDistance * normalizedOffset);



    public void SetSignedOffset(float signedOffset)
    {
        signedOffset = Mathf.Clamp(signedOffset, -maxExternalOffset, maxExternalOffset);

        Vector3 targetPosition = _startPosition + GetDirectionVector() * (config.maxDistance * signedOffset);

        ApplyMovement(targetPosition, false);
    }
    
    public void SetSignedOffset(float signedOffset)
    {
        if (controlMode == ControlMode.Independent && _isReturning)
            return;

        signedOffset = Mathf.Clamp(signedOffset, -1f, 1f);

        Vector3 targetPosition = _startPosition + GetDirectionVector() * (config.maxDistance * signedOffset);

        if (config.instantMovement)
        {
            transform.position = targetPosition;
            _currentTargetPosition = targetPosition;
            _hasTarget = true;
            return;
        }

        bool sameTarget = _hasTarget && Vector3.Distance(_currentTargetPosition, targetPosition) < 0.01f;

        if (sameTarget && _movementCoroutine != null)
            return;

        _currentTargetPosition = targetPosition;
        _hasTarget = true;

        StartSmoothMovement(targetPosition, false);
    }

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

        if (config.instantMovement)
        {
            transform.position = _startPosition;
            return;
        }

        StartSmoothMovement(_startPosition, false, false);
    }

    public float CalculateNormalizedOffsetFromWeight()
    {
        if (config.requiredWeight <= 0f)
            return 1f;

        return Mathf.Clamp01(CurrentWeight / config.requiredWeight);
    }

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

        bool sameTarget = _hasTarget && Vector3.Distance(_currentTargetPosition, targetPosition) < 0.01f;

        if (sameTarget && _movementCoroutine != null)
            return;

        _currentTargetPosition = targetPosition;
        _hasTarget = true;

        bool useDelay = controlMode == ControlMode.Independent;
        StartSmoothMovement(targetPosition, reachingEnd, useDelay);
    }

    private void StartSmoothMovement(Vector3 targetPosition, bool reachingEnd, bool useDelay)
    {
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, reachingEnd, useDelay));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, bool reachingEnd, bool useDelay)
    {
        if (useDelay && config.movementDelay > 0f)
            yield return new WaitForSeconds(config.movementDelay);

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
        _movementCoroutine = null;
        _currentTargetPosition = targetPosition;
        _hasTarget = true;

        if (reachingEnd)
            OnReachedEnd();
    }

    private void OnReachedEnd()
    {
        if (_hasCompletedForwardMove)
            return;

        _hasCompletedForwardMove = true;

        if (config.returnAfterReachingEnd && controlMode == ControlMode.Independent)
        {
            if (_movementCoroutine != null)
                StopCoroutine(_movementCoroutine);

            _movementCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    private IEnumerator ReturnAfterDelay()
    {
        _isReturning = true;

        if (config.returnDelay > 0f)
            yield return new WaitForSeconds(config.returnDelay);

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

    private Vector3 GetDirectionVector()
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

    public void RegisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight += weightProvider.Weight;
    }

    public void RegisterWeight(float weight)
    {
        CurrentWeight += weight;
    }

    public void UnregisterWeight(IProvidesWeight weightProvider)
    {
        CurrentWeight -= weightProvider.Weight;
    }

    public void UnregisterWeight(float weight)
    {
        CurrentWeight -= weight;
    }

    public bool HasEnoughWeight() => CurrentWeight >= config.requiredWeight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("webo");
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if ( weightProvider != null)
        {

            //_weightProviders.Add(weightProvider);
            RegisterWeight(weightProvider);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IProvidesWeight weightProvider = other.GetComponent<IProvidesWeight>();
        if ( weightProvider != null)
        {
            //_weightProviders.Add(weightProvider);
            UnregisterWeight(weightProvider);
        }
    }
}