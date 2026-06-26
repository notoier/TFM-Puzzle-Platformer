using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class CancelNode : MiscNode
{
    [SerializeField]
    private CancelMode cancelMode;

    [SerializeField]
    private bool negateCondition;

    [SerializeField]
    private TargetSource targetSource;

    [SerializeField]
    private TargetSelectionMode targetSelectionMode;

    [SerializeField]
    private string targetTag;

    [SerializeField]
    private string targetName;

    [SerializeField]
    private string contextTargetKey;

    [SerializeField]
    private AbilityValueType contextVariableType;

    [SerializeField]
    private string contextVariableKey;

    [SerializeField]
    private TargetDistanceComparison distanceComparison;

    [SerializeField]
    private float distance = 1f;

    public override void Execute(AbilityContext context)
    {
        if (ShouldCancel(context))
            Cancel(context);
        else
            Complete(context);
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        if (cancelMode == CancelMode.Always)
            return AbilityValidationResult.Complete();

        if (cancelMode == CancelMode.IfContextVariableExists)
            return ValidateContextVariable(context);

        if (cancelMode != CancelMode.IfTargetExists && cancelMode != CancelMode.IfTargetDistance)
            return AbilityValidationResult.Incomplete($"Cancel mode '{cancelMode}' is not implemented yet.");

        if (cancelMode == CancelMode.IfTargetDistance && distance < 0f)
            return AbilityValidationResult.Invalid("Cancel node distance cannot be negative.");

        return ValidateTargetSource();
    }

    private AbilityValidationResult ValidateTargetSource()
    {
#if UNITY_EDITOR
        if (targetSource == TargetSource.Tag)
        {
            if (string.IsNullOrWhiteSpace(targetTag))
                return AbilityValidationResult.Incomplete("Cancel node needs a target tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Cancel node needs a target name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Cancel node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Cancel target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }

    private bool ShouldCancel(AbilityContext context)
    {
        if (cancelMode == CancelMode.Always)
            return true;

        bool conditionResult = cancelMode switch
        {
            CancelMode.IfTargetExists => TargetExists(context),
            CancelMode.IfContextVariableExists => ContextVariableExists(context),
            CancelMode.IfTargetDistance => TargetMatchesDistance(context),
            _ => false
        };

        return negateCondition ? !conditionResult : conditionResult;
    }

    private bool ContextVariableExists(AbilityContext context)
    {
        if (context == null || string.IsNullOrWhiteSpace(contextVariableKey))
            return false;

        return contextVariableType switch
        {
            AbilityValueType.Float => context.TryGetFloat(contextVariableKey, out _),
            AbilityValueType.Int => context.TryGetInt(contextVariableKey, out _),
            AbilityValueType.Bool => context.TryGetBool(contextVariableKey, out _),
            AbilityValueType.Vector3 => context.TryGetVector(contextVariableKey, out _),
            AbilityValueType.GameObject => context.TryGetGameObject(contextVariableKey, out GameObject contextObject) && contextObject != null,
            _ => false
        };
    }

    private AbilityValidationResult ValidateContextVariable(AbilityValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(contextVariableKey))
            return AbilityValidationResult.Incomplete("Cancel node needs a context variable key.");

        if (context != null)
        {
            if (!context.TryGetVariable(contextVariableKey, out AbilityVariableDefinition variable))
                return AbilityValidationResult.Invalid($"Cancel node uses undeclared variable '{contextVariableKey}'.");

            if (variable.type != contextVariableType)
                return AbilityValidationResult.Invalid($"Variable '{contextVariableKey}' is {variable.type}, but Cancel node checks {contextVariableType}.");
        }

        return AbilityValidationResult.Complete();
    }

    private bool TargetMatchesDistance(AbilityContext context)
    {
        if (context?.actor == null)
            return false;

        if (!TryGetTarget(context, out GameObject target) || target == null)
            return false;

        float sqrDistance = Vector3.SqrMagnitude(target.transform.position - context.actor.transform.position);
        float sqrThreshold = distance * distance;

        return distanceComparison switch
        {
            TargetDistanceComparison.CloserThan => sqrDistance < sqrThreshold,
            TargetDistanceComparison.FurtherThan => sqrDistance > sqrThreshold,
            _ => false
        };
    }

    private bool TargetExists(AbilityContext context)
    {
        if (targetSource == TargetSource.Self)
            return context.actor != null;

        if (targetSource == TargetSource.ContextTarget)
            return context.TryGetGameObject(contextTargetKey, out GameObject contextTarget) && contextTarget != null;

        List<GameObject> candidates = GetCandidates();
        return candidates.Count > 0;
    }

    private bool TryGetTarget(AbilityContext context, out GameObject target)
    {
        target = null;

        if (targetSource == TargetSource.Self)
        {
            target = context.actor;
            return target != null;
        }

        if (targetSource == TargetSource.ContextTarget)
            return context.TryGetGameObject(contextTargetKey, out target) && target != null;

        target = SelectTarget(GetCandidates(), context.actor.transform.position);
        return target != null;
    }

    private GameObject SelectTarget(List<GameObject> candidates, Vector3 origin)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        switch (targetSelectionMode)
        {
            case TargetSelectionMode.Farthest:
                return SelectTargetByDistance(candidates, origin, true);
            case TargetSelectionMode.Random:
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            case TargetSelectionMode.Last:
                return candidates[^1];
            case TargetSelectionMode.Closest:
                return SelectTargetByDistance(candidates, origin, false);
            default:
                return candidates[0];
        }
    }

    private GameObject SelectTargetByDistance(List<GameObject> candidates, Vector3 origin, bool farthest)
    {
        GameObject selected = null;
        float selectedDistance = farthest ? float.MinValue : float.MaxValue;

        foreach (GameObject candidate in candidates)
        {
            if (candidate == null)
                continue;

            float candidateDistance = Vector3.SqrMagnitude(candidate.transform.position - origin);
            if (farthest ? candidateDistance > selectedDistance : candidateDistance < selectedDistance)
            {
                selected = candidate;
                selectedDistance = candidateDistance;
            }
        }

        return selected;
    }

    private List<GameObject> GetCandidates()
    {
        List<GameObject> candidates = new List<GameObject>();

        switch (targetSource)
        {
            case TargetSource.Tag:
                if (!string.IsNullOrWhiteSpace(targetTag))
                    candidates.AddRange(GameObject.FindGameObjectsWithTag(targetTag));
                break;

            case TargetSource.Name:
                if (string.IsNullOrWhiteSpace(targetName))
                    break;

                Transform[] sceneTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                foreach (Transform sceneTransform in sceneTransforms)
                {
                    string objectName = sceneTransform.gameObject.name;
                    if (objectName == targetName || objectName == $"{targetName}(Clone)")
                        candidates.Add(sceneTransform.gameObject);
                }
                break;
        }

        return candidates;
    }
}

public enum CancelMode
{
    Always,
    IfTargetExists,
    IfContextVariableExists,
    IfTargetDistance
}

public enum TargetDistanceComparison
{
    CloserThan,
    FurtherThan
}
