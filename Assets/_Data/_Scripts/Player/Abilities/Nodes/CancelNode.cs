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
    private TargetSource targetSource;

    [SerializeField]
    private string targetTag;

    [SerializeField]
    private string targetName;

    [SerializeField]
    private string contextTargetKey;

    public override void Execute(AbilityContext context)
    {
        if (ShouldCancel(context))
            Cancel(context);
        else
            Complete(context);
    }

    public override AbilityValidationResult Validate()
    {
        if (cancelMode == CancelMode.Always)
            return AbilityValidationResult.Complete();

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

        return TargetExists(context);
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
    IfTargetExists
}
