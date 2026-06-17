using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class ReadWeightNode : DataNode
{
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
    private string outputKey = "weight";

    [SerializeField]
    private ReadWeightOutputType outputType = ReadWeightOutputType.Int;

    [SerializeField]
    private float divideBy = 1f;

    public override void Execute(AbilityContext context)
    {
        if (!TryGetWeightTarget(context, out IProvidesWeight weightTarget))
        {
            Debug.LogError("IProvidesWeight component not found on read weight target.");
            Fail(context);
            return;
        }

        if (Mathf.Approximately(divideBy, 0f))
        {
            Debug.LogError("Read weight node cannot divide by zero.");
            Fail(context);
            return;
        }

        float resolvedWeight = weightTarget.Weight / divideBy;
        if (outputType == ReadWeightOutputType.Float)
            context.SetFloat(outputKey, resolvedWeight);
        else
            context.SetInt(outputKey, Mathf.RoundToInt(resolvedWeight));

        Complete(context);
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        AbilityValidationResult targetValidation = ValidateTargetSource();
        if (targetValidation.BlocksUse)
            return targetValidation;

        if (string.IsNullOrWhiteSpace(outputKey))
            return AbilityValidationResult.Incomplete("Read weight node needs an output key.");

        if (Mathf.Approximately(divideBy, 0f))
            return AbilityValidationResult.Invalid("Read weight node cannot divide by zero.");

        AbilityValueType expectedType = outputType == ReadWeightOutputType.Float
            ? AbilityValueType.Float
            : AbilityValueType.Int;

        if (context != null)
        {
            if (!context.TryGetVariable(outputKey, out AbilityVariableDefinition variable))
                return AbilityValidationResult.Invalid($"Read weight node writes undeclared variable '{outputKey}'.");

            if (variable.type != expectedType)
                return AbilityValidationResult.Invalid($"Variable '{outputKey}' is {variable.type}, but Read weight node writes {expectedType}.");
        }

        return AbilityValidationResult.Complete();
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
                return AbilityValidationResult.Incomplete("Read weight node needs a target tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Read weight node needs a target name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Read weight node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Read weight target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }
}

public enum ReadWeightOutputType
{
    Float,
    Int
}
