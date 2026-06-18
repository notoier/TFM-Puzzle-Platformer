using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class TargetNode : DataNode
{
    [SerializeField]
    private TargetSource targetSource;

    [SerializeField]
    private TargetSelectionMode targetSelectionMode;

    [SerializeField]
    private string outputVectorKey = "targetPosition";

    [SerializeField]
    private string targetTag;

    [SerializeField]
    private string targetName;

    [SerializeField]
    private string contextTargetKey;

    public override void Execute(AbilityContext context)
    {
        if (!TryGetTargetPosition(context, out Vector3 targetPosition))
        {
            Fail(context);
            return;
        }

        context.SetVector(outputVectorKey, targetPosition);

        if (!context.success)
            return;

        if (context.actor == null)
        {
            Fail(context);
            return;
        }

        context.direction = targetPosition - context.actor.transform.position;
        Complete(context);
    }

    private bool TryGetTargetPosition(AbilityContext context, out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        if (context.actor == null)
            return false;

        if (targetSource == TargetSource.Self)
        {
            targetPosition = context.actor.transform.position;
            return true;
        }

        if (targetSource == TargetSource.ContextTarget)
        {
            if (!context.TryGetGameObject(contextTargetKey, out GameObject contextTarget) || contextTarget == null)
                return false;

            targetPosition = contextTarget.transform.position;
            return true;
        }

        List<GameObject> candidates = GetCandidates();
        GameObject target = SelectTarget(candidates, context.actor.transform.position);
        if (target == null)
            return false;

        targetPosition = target.transform.position;
        return true;
    }

    private List<GameObject> GetCandidates()
    {
        List<GameObject> candidates = new List<GameObject>();

        switch (targetSource)
        {
            case TargetSource.Tag:
                if (string.IsNullOrWhiteSpace(targetTag))
                    return candidates;

                candidates.AddRange(GameObject.FindGameObjectsWithTag(targetTag));
                break;

            case TargetSource.Name:
                if (string.IsNullOrWhiteSpace(targetName))
                    return candidates;

                Transform[] sceneTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                foreach (Transform sceneTransform in sceneTransforms)
                {
                    if (sceneTransform.gameObject.name == targetName)
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
                return SelectByDistance(candidates, origin, true);
            case TargetSelectionMode.Random:
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];
            case TargetSelectionMode.Last:
                return candidates[^1];
            case TargetSelectionMode.Closest:
                return SelectByDistance(candidates, origin, false);
            default:
                return candidates[0];
        }
    }

    private GameObject SelectByDistance(List<GameObject> candidates, Vector3 origin, bool farthest)
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

    public override AbilityValidationResult Validate()
    {
#if UNITY_EDITOR
        if (targetSource == TargetSource.Tag)
        {
            if (string.IsNullOrWhiteSpace(targetTag))
                return AbilityValidationResult.Incomplete("Target node needs a tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);

            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Target node needs a name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Target node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }
}

//ONLY SELF, TAG & NAME IMPLEMENTED
public enum TargetSource
{
    Self,
    Object,
    Tag,
    Name,
    Layer,
    Childrens,
    Siblings,
    Parent,
    PreviousTarget,
    ContextTarget,
    MousePosition,
    Input
}

//NOT IMPLEMENTED YET
public enum TargetSearchMode
{
    All,
    Radius,
    MinMax,
    Box,    
    Cone,
    Line,
    Raycast,
    Overlap,
    Screen
}

//NOT IMPLEMENTED YET
public enum TargetFilter
{
    MustBeSelf,
    MustBeActive,
    MustBeVisible,
    MustHaveComponent,
    MustHaveTag,
    MustBeInLayer,
    MustBeEnemy,
    MustBeGrounded,
    MustBeInAir,
    MustBeInLineOfSight,
    Distance
}

public enum TargetSelectionMode
{
    Closest,
    Farthest,
    Random,
    First,
    Last,
    HighestHealth,
    LowestHealth,
    HighestLevel,
    LowestLevel,
    Index,
    Health
}

