using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class MovementNode : ActionNode
{
    [SerializeField]
    private MovementMode movementMode;

    [SerializeField]
    private TargetSource actorSource;

    [SerializeField]
    private TargetSelectionMode actorSelectionMode;

    [SerializeField]
    private string actorTag;

    [SerializeField]
    private string actorName;

    [SerializeField]
    private string actorKey;

    public float distance;
    public bool instant = true;
    [Min(0f)]
    public float duration = 0.2f;

    [SerializeField]
    private MovementDirectionSource directionSource;

    [SerializeField]
    private MovementPositionSource positionSource;

    [SerializeField]
    private string positionKey;

    [SerializeReference]
    private ParameterNode localDirection;

    [SerializeReference]
    private TargetNode targetNode;

    public override void Execute(AbilityContext context)
    {
        if (context.cancelled) return;

        if (!TryGetMovementTarget(context, out Transform target))
        {
            Fail(context);
            return;
        }

        if (!TryGetMovement(context, target, out Vector3 movement))
        {
            Fail(context);
            return;
        }

        if (instant)
        {
            target.position += movement;
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
        context.coroutineRunner.StartCoroutine(MoveOverTime(context, target, movement));
    }

    public override AbilityValidationResult Validate()
    {
        AbilityValidationResult actorValidation = ValidateActorSource();
        if (actorValidation.BlocksUse)
            return actorValidation;

        if (movementMode == MovementMode.Position)
        {
            if (positionSource == MovementPositionSource.ContextVector && string.IsNullOrWhiteSpace(positionKey))
                return AbilityValidationResult.Incomplete("Movement node needs a context vector key.");

            if (positionSource == MovementPositionSource.TargetNode)
            {
                if (targetNode == null)
                    return AbilityValidationResult.Incomplete("Movement node needs a target node.");

                AbilityValidationResult validation = targetNode.Validate();
                if (validation.BlocksUse)
                    return validation;
            }

            if (positionSource == MovementPositionSource.LocalDirection)
            {
                if (localDirection == null)
                    return AbilityValidationResult.Incomplete("Movement node needs a local direction parameter.");

                AbilityValidationResult validation = localDirection.ValidateValue(null, AbilityValueType.Vector3);
                if (validation.BlocksUse)
                    return validation;
            }
        }

        if (movementMode == MovementMode.Distance && distance == 0)
            return AbilityValidationResult.Incomplete("Movement node needs a distance.");

        if (movementMode == MovementMode.Distance && directionSource == MovementDirectionSource.LocalDirection)
        {
            if (localDirection == null)
                return AbilityValidationResult.Incomplete("Movement node needs a local direction parameter.");

            AbilityValidationResult validation = localDirection.ValidateValue(null, AbilityValueType.Vector3);
            if (validation.BlocksUse)
                return validation;
        }

        if (movementMode == MovementMode.Distance && directionSource == MovementDirectionSource.TargetNode)
        {
            if (targetNode == null)
                return AbilityValidationResult.Incomplete("Movement node needs a target node.");

            AbilityValidationResult validation = targetNode.Validate();
            if (validation.BlocksUse)
                return validation;
        }

        if (!instant && duration <= 0f)
            return AbilityValidationResult.Incomplete("Non-instant movement needs a duration greater than 0.");

        return AbilityValidationResult.Complete();
    }

    private bool TryGetMovementTarget(AbilityContext context, out Transform target)
    {
        target = null;

        if (!TryGetActorObject(context, out GameObject actorObject) || actorObject == null)
        {
            return false;
        }

        target = actorObject.transform;
        return true;
    }

    private bool TryGetActorObject(AbilityContext context, out GameObject actorObject)
    {
        actorObject = null;

        if (context.actor == null)
            return false;

        if (actorSource == TargetSource.Self)
        {
            actorObject = context.actor;
            return true;
        }

        if (actorSource == TargetSource.ContextTarget)
        {
            return context.TryGetGameObject(actorKey, out actorObject) && actorObject != null;
        }

        List<GameObject> candidates = GetActorCandidates();
        actorObject = SelectActor(candidates, context.actor.transform.position);
        return actorObject != null;
    }

    private List<GameObject> GetActorCandidates()
    {
        List<GameObject> candidates = new List<GameObject>();

        switch (actorSource)
        {
            case TargetSource.Tag:
                if (string.IsNullOrWhiteSpace(actorTag))
                    return candidates;

                candidates.AddRange(GameObject.FindGameObjectsWithTag(actorTag));
                break;

            case TargetSource.Name:
                if (string.IsNullOrWhiteSpace(actorName))
                    return candidates;

                Transform[] sceneTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                foreach (Transform sceneTransform in sceneTransforms)
                {
                    if (sceneTransform.gameObject.name == actorName)
                        candidates.Add(sceneTransform.gameObject);
                }
                break;
        }

        return candidates;
    }

    private GameObject SelectActor(List<GameObject> candidates, Vector3 origin)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        switch (actorSelectionMode)
        {
            case TargetSelectionMode.Farthest:
                return SelectActorByDistance(candidates, origin, true);
            case TargetSelectionMode.Random:
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            case TargetSelectionMode.Last:
                return candidates[^1];
            case TargetSelectionMode.Closest:
                return SelectActorByDistance(candidates, origin, false);
            default:
                return candidates[0];
        }
    }

    private GameObject SelectActorByDistance(List<GameObject> candidates, Vector3 origin, bool farthest)
    {
        GameObject selected = null;
        float selectedDistance = farthest ? float.MinValue : float.MaxValue;

        foreach (GameObject candidate in candidates)
        {
            float distance = Vector3.SqrMagnitude(candidate.transform.position - origin);
            if (farthest ? distance > selectedDistance : distance < selectedDistance)
            {
                selected = candidate;
                selectedDistance = distance;
            }
        }

        return selected;
    }

    private AbilityValidationResult ValidateActorSource()
    {
#if UNITY_EDITOR
        if (actorSource == TargetSource.Tag)
        {
            if (string.IsNullOrWhiteSpace(actorTag))
                return AbilityValidationResult.Incomplete("Movement node needs an actor tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == actorTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{actorTag}' does not exist.");
        }
#endif

        if (actorSource == TargetSource.Name && string.IsNullOrWhiteSpace(actorName))
            return AbilityValidationResult.Incomplete("Movement node needs an actor name.");

        if (actorSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(actorKey))
            return AbilityValidationResult.Incomplete("Movement node needs a context actor key.");

        if (actorSource != TargetSource.Self
            && actorSource != TargetSource.Tag
            && actorSource != TargetSource.Name
            && actorSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Movement actor source '{actorSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }

    private bool TryGetMovement(AbilityContext context, Transform target, out Vector3 movement)
    {
        movement = Vector3.zero;

        if (movementMode == MovementMode.Position)
        {
            return TryGetPositionMovement(context, target, out movement);
        }

        Vector3 movementDirection = GetDirection(context);
        if (movementDirection == Vector3.zero)
            return false;

        movement = movementDirection.normalized * distance;
        return true;
    }

    private bool TryGetPositionMovement(AbilityContext context, Transform target, out Vector3 movement)
    {
        movement = Vector3.zero;

        if (positionSource == MovementPositionSource.TargetNode)
        {
            targetNode.Execute(context);
            if (!context.success || context.actor == null)
                return false;

            Vector3 targetPosition = context.actor.transform.position + context.direction;
            movement = targetPosition - target.position;
            return true;
        }

        if (positionSource == MovementPositionSource.ContextVector)
        {
            if (!context.TryGetVector(positionKey, out Vector3 contextPosition))
                return false;

            movement = contextPosition - target.position;
            return true;
        }

        movement = positionSource switch
        {
            MovementPositionSource.LocalDirection => localDirection != null ? localDirection.GetValue<Vector3>(context) : Vector3.zero,
            MovementPositionSource.ContextDirection => context.direction,
            _ => Vector3.zero
        };

        return positionSource == MovementPositionSource.LocalDirection && localDirection != null
               || positionSource == MovementPositionSource.ContextDirection;
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
        if (directionSource == MovementDirectionSource.TargetNode)
        {
            targetNode.Execute(context);
            return context.success ? context.direction : Vector3.zero;
        }

        return directionSource switch
        {
            MovementDirectionSource.ContextDirection => context.direction,
            MovementDirectionSource.ActorFacing => GetActorFacingDirection(context),
            _ => localDirection != null ? localDirection.GetValue<Vector3>(context) : Vector3.zero
        };
    }

    private Vector3 GetActorFacingDirection(AbilityContext context)
    {
        if (context.actor == null)
            return Vector3.zero;

        float xSign = Mathf.Sign(context.actor.transform.localScale.x);
        if (Mathf.Approximately(xSign, 0f))
            xSign = 1f;

        return new Vector3(xSign, 0f, 0f);
    }
}

public enum MovementDirectionSource
{
    LocalDirection,
    ContextDirection,
    TargetNode,
    ActorFacing
}

public enum MovementMode
{
    Distance,
    Position
}

public enum MovementPositionSource
{
    TargetNode,
    LocalDirection,
    ContextDirection,
    ContextVector
}
