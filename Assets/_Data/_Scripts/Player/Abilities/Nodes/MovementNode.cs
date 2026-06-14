using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class MovementNode : ActionNode
{
    public float distance;
    public bool instant = true;
    [Min(0f)]
    public float duration = 0.2f;

    [SerializeField]
    private MovementDirectionSource directionSource;

    [SerializeField]
    private Vector3 direction;

    public override void Execute(AbilityContext context)
    {
        if (context.cancelled) return;

        if (context.actor == null)
        {
            Fail(context);
            return;
        }

        Vector3 movementDirection = GetDirection(context);
        if (movementDirection == Vector3.zero)
        {
            Fail(context);
            return;
        }

        Vector3 movement = movementDirection.normalized * distance;
        if (instant)
        {
            context.actor.transform.position += movement;
            Complete(context);
            return;
        }

        if (context.coroutineRunner == null)
        {
            Fail(context);
            Debug.LogWarning("Movement node needs a coroutine runner for non-instant movement.");
            return;
        }

        KeepAbilityActive(context);
        Complete(context);
        context.coroutineRunner.StartCoroutine(MoveOverTime(context, context.actor.transform, movement));
    }

    public override AbilityValidationResult Validate()
    {
        if (distance == 0)
            return AbilityValidationResult.Incomplete("Movement node needs a distance.");

        if (directionSource == MovementDirectionSource.LocalDirection && direction == Vector3.zero)
            return AbilityValidationResult.Incomplete("Movement node needs a direction.");

        if (!instant && duration <= 0f)
            return AbilityValidationResult.Incomplete("Non-instant movement needs a duration greater than 0.");

        return AbilityValidationResult.Complete();
    }

    private IEnumerator MoveOverTime(AbilityContext context, Transform target, Vector3 movement)
    {
        Vector3 startPosition = target.position;
        Vector3 endPosition = startPosition + movement;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            yield return null;
        }

        target.position = endPosition;
        FinishAbility(context);
    }

    private Vector3 GetDirection(AbilityContext context)
    {
        return directionSource switch
        {
            MovementDirectionSource.ContextDirection => context.direction,
            _ => direction
        };
    }
}

public enum MovementDirectionSource
{
    LocalDirection,
    ContextDirection
}
