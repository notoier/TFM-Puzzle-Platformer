using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class WeightNode : ActionNode
{
    [SerializeField]
    private TargetSource targetSource;

    [SerializeField]
    private WeightNodeType nodeType;

    [SerializeField]
    private TargetSelectionMode targetSelectionMode;

    [SerializeField]
    private string targetTag;

    [SerializeField]
    private string targetName;

    [SerializeField]
    private string contextTargetKey;

    [SerializeField]
    private WeightValueSource valueSource;

    [SerializeField]
    private int weightValue = 1;

    [SerializeReference]
    private ParameterNode weightParameter;

    public override void Execute(AbilityContext context)
    {
        if (!TryGetWeightTarget(context, out IProvidesWeight weightTarget))
        {
            Debug.LogError("IProvidesWeight component not found on weight target.");
            Fail(context);
            return;
        }

        int resolvedWeight = GetWeightValue(context);
        if (nodeType == WeightNodeType.Divide && resolvedWeight == 0)
        {
            Debug.LogError("Weight node cannot divide by zero.");
            Fail(context);
            return;
        }

        switch (nodeType)
        {
            case WeightNodeType.Sum:
                weightTarget.AddWeight(resolvedWeight, true);
                break;
            case WeightNodeType.Rest:
                weightTarget.AddWeight(-resolvedWeight, true);
                break;
            case WeightNodeType.Set:
                weightTarget.Weight = resolvedWeight;
                weightTarget.AdaptWeight(false);
                break;
            case WeightNodeType.Divide:
                weightTarget.Weight /= resolvedWeight;
                weightTarget.AdaptWeight(false);
                break;
            case WeightNodeType.Multiply:
                weightTarget.Weight *= resolvedWeight;
                weightTarget.AdaptWeight(false);
                break;
        }
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        AbilityValidationResult targetValidation = ValidateTargetSource();
        if (targetValidation.BlocksUse)
            return targetValidation;

        if (valueSource == WeightValueSource.LocalValue)
        {
            if (nodeType == WeightNodeType.Divide && weightValue == 0)
                return AbilityValidationResult.Invalid("Weight node cannot divide by zero.");

            return AbilityValidationResult.Complete();
        }

        if (weightParameter == null)
            return AbilityValidationResult.Incomplete("Weight node needs a weight parameter.");

        AbilityValidationResult validation = weightParameter.ValidateValue(context, AbilityValueType.Int);
        if (validation.BlocksUse)
            return validation;

        if (nodeType == WeightNodeType.Divide && weightParameter.GetValue<int>() == 0)
            return AbilityValidationResult.Invalid("Weight node cannot divide by zero.");

        return AbilityValidationResult.Complete();
    }

    private int GetWeightValue(AbilityContext context)
    {
        return valueSource == WeightValueSource.LocalValue
            ? weightValue
            : weightParameter.GetValue<int>(context);
    }

    private bool TryGetWeightTarget(AbilityContext context, out IProvidesWeight weightTarget)
    {
        weightTarget = null;

        if (!TryGetTargetObject(context, out GameObject targetObject) || targetObject == null)
            return false;

        weightTarget = targetObject.GetComponent<IProvidesWeight>();
        return weightTarget != null;
    }

    private bool TryGetTargetObject(AbilityContext context, out GameObject targetObject)
    {
        targetObject = null;

        if (targetSource == TargetSource.Self)
        {
            targetObject = context.actor;
            return targetObject != null;
        }

        if (targetSource == TargetSource.ContextTarget)
        {
            return context.TryGetGameObject(contextTargetKey, out targetObject) && targetObject != null;
        }

        List<GameObject> candidates = GetTargetCandidates();
        targetObject = SelectTarget(candidates, context.actor != null ? context.actor.transform.position : Vector3.zero);
        return targetObject != null;
    }

    private List<GameObject> GetTargetCandidates()
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
            float distance = Vector3.SqrMagnitude(candidate.transform.position - origin);
            if (farthest ? distance > selectedDistance : distance < selectedDistance)
            {
                selected = candidate;
                selectedDistance = distance;
            }
        }

        return selected;
    }

    private AbilityValidationResult ValidateTargetSource()
    {
#if UNITY_EDITOR
        if (targetSource == TargetSource.Tag)
        {
            if (string.IsNullOrWhiteSpace(targetTag))
                return AbilityValidationResult.Incomplete("Weight node needs a target tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Weight node needs a target name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Weight node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Weight target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }
}

public enum WeightNodeType
{
    Sum,
    Rest,
    Set,
    Divide,
    Multiply
}

public enum WeightValueSource
{
    ParameterNode,
    LocalValue
}
