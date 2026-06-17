using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class ScaleNode : ActionNode
{
    [SerializeField]
    private ScaleNodeType nodeType;

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

    public float scale;

    [SerializeField]
    private ScaleValueSource valueSource;

    [SerializeReference]
    private ParameterNode scaleParameter;

    public override void Execute(AbilityContext context)
    {
        if (!TryGetScaleTarget(context, out Transform scaleTarget))
        {
            Fail(context);
            return;
        }

        float resolvedScale = GetScale(context);
        Vector3 currentScale = scaleTarget.localScale;
        float xSign = Mathf.Sign(currentScale.x);
        if (Mathf.Approximately(xSign, 0f))
            xSign = 1f;

        float targetScale = nodeType == ScaleNodeType.Multiply
            ? Mathf.Abs(currentScale.y) * resolvedScale
            : resolvedScale;

        scaleTarget.localScale = new Vector3(xSign * targetScale, targetScale, currentScale.z);
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        AbilityValidationResult targetValidation = ValidateTargetSource();
        if (targetValidation.BlocksUse)
            return targetValidation;

        if (valueSource == ScaleValueSource.ParameterNode)
        {
            if (scaleParameter == null)
                return AbilityValidationResult.Incomplete("Scale node needs a scale parameter.");

            return scaleParameter.ValidateValue(context, AbilityValueType.Float);
        }

        return AbilityValidationResult.Complete();  
    }

    private float GetScale(AbilityContext context)
    {
        return valueSource == ScaleValueSource.ParameterNode && scaleParameter != null
            ? scaleParameter.GetValue<float>(context)
            : scale;
    }

    private bool TryGetScaleTarget(AbilityContext context, out Transform scaleTarget)
    {
        scaleTarget = null;

        if (!TryGetTargetObject(context, out GameObject targetObject) || targetObject == null)
            return false;

        scaleTarget = targetObject.transform;
        return true;
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
                return AbilityValidationResult.Incomplete("Scale node needs a target tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Scale node needs a target name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Scale node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Scale target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }
}

public enum ScaleValueSource
{
    LocalValue,
    ParameterNode
}

public enum ScaleNodeType
{
    Set,
    Multiply
}
