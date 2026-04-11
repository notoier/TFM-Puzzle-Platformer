using UnityEngine;
using System.Collections;

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

    private Vector3 _startPosition;
    private Coroutine _movementCoroutine;
    private float _currentNormalizedOffset;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        if (controlMode != ControlMode.Independent)
            return;

        float normalizedOffset = CalculateNormalizedOffsetFromWeight();

        if (!config.recovers && normalizedOffset <= 0f && CurrentWeight < config.requiredWeight)
            return;

        SetNormalizedOffset(normalizedOffset);
    }

    public void SetNormalizedOffset(float normalizedOffset)
    {
        normalizedOffset = Mathf.Clamp01(normalizedOffset);
        _currentNormalizedOffset = normalizedOffset;

        Vector3 targetPosition = _startPosition + GetDirectionVector() * (config.maxDistance * normalizedOffset);

        if (config.instantMovement)
        {
            transform.position = targetPosition;
            return;
        }

        StartSmoothMovement(targetPosition);
    }

    public void ResetToStart()
    {
        SetNormalizedOffset(0f);
    }

    public float CalculateNormalizedOffsetFromWeight()
    {
        if (config.requiredWeight <= 0f)
            return 1f;

        return Mathf.Clamp01(CurrentWeight / config.requiredWeight);
    }

    private void StartSmoothMovement(Vector3 targetPosition)
    {
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(MoveToPosition(targetPosition));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (config.movementDelay > 0f)
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
}