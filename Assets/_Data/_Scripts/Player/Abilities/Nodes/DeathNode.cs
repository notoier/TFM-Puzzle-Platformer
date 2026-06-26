using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class DeathNode : ActionNode
{
    [SerializeField]
    private DeathAction deathAction;

    [SerializeField]
    private TargetSource targetSource;

    [SerializeField]
    private TargetSelectionMode targetSelectionMode;

    [SerializeField]
    private bool affectAllTargets;

    [SerializeField]
    private string targetTag;

    [SerializeField]
    private string targetName;

    [SerializeField]
    private string contextTargetKey;

    public override void Execute(AbilityContext context)
    {
        List<GameObject> targets = GetTargets(context);
        if (targets.Count == 0)
        {
            Fail(context);
            return;
        }

        foreach (GameObject target in targets)
        {
            if (target == null)
                continue;

            ApplyDeathAction(target);
        }

        Complete(context);
    }

    public override AbilityValidationResult Validate()
    {
#if UNITY_EDITOR
        if (targetSource == TargetSource.Tag)
        {
            if (string.IsNullOrWhiteSpace(targetTag))
                return AbilityValidationResult.Incomplete("Death node needs a target tag.");

            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == targetTag);
            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{targetTag}' does not exist.");
        }
#endif

        if (targetSource == TargetSource.Name && string.IsNullOrWhiteSpace(targetName))
            return AbilityValidationResult.Incomplete("Death node needs a target name.");

        if (targetSource == TargetSource.ContextTarget && string.IsNullOrWhiteSpace(contextTargetKey))
            return AbilityValidationResult.Incomplete("Death node needs a context target key.");

        if (targetSource != TargetSource.Self
            && targetSource != TargetSource.Tag
            && targetSource != TargetSource.Name
            && targetSource != TargetSource.ContextTarget)
            return AbilityValidationResult.Incomplete($"Death target source '{targetSource}' is not implemented yet.");

        return AbilityValidationResult.Complete();
    }

    private List<GameObject> GetTargets(AbilityContext context)
    {
        List<GameObject> candidates = GetCandidates(context);
        if (affectAllTargets || !UsesSelection())
            return candidates;

        GameObject selected = SelectTarget(candidates, context?.actor != null ? context.actor.transform.position : Vector3.zero);
        return selected != null ? new List<GameObject> { selected } : new List<GameObject>();
    }

    private List<GameObject> GetCandidates(AbilityContext context)
    {
        List<GameObject> candidates = new List<GameObject>();

        switch (targetSource)
        {
            case TargetSource.Self:
                if (context?.actor != null)
                    candidates.Add(context.actor);
                break;

            case TargetSource.ContextTarget:
                if (context != null
                    && context.TryGetGameObject(contextTargetKey, out GameObject contextTarget)
                    && contextTarget != null)
                {
                    candidates.Add(contextTarget);
                }
                break;

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
            if (candidate == null)
                continue;

            float distance = Vector3.SqrMagnitude(candidate.transform.position - origin);
            if (farthest ? distance > selectedDistance : distance < selectedDistance)
            {
                selected = candidate;
                selectedDistance = distance;
            }
        }

        return selected;
    }

    private void ApplyDeathAction(GameObject target)
    {
        if (deathAction == DeathAction.ReturnToPool)
        {
            if (target.TryGetComponent(out IPoolable poolable))
            {
                poolable.ReturnToPool();
                return;
            }

            target.SetActive(false);
            return;
        }

        UnityEngine.Object.Destroy(target);
    }

    private bool UsesSelection()
    {
        return targetSource == TargetSource.Tag || targetSource == TargetSource.Name;
    }
}

public enum DeathAction
{
    Destroy,
    ReturnToPool
}

public interface IPoolable
{
    void ReturnToPool();
}
