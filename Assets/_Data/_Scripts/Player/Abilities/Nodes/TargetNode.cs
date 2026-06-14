using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

[Serializable]
public class TargetNode : DataNode
{
    
    public TargetType targetType;

    public override void Execute(AbilityContext context)
    {
        Vector3 targetPosition = GetTargetPosition(context);
        context.SetVector("targetPosition", targetPosition);

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

    private Vector3 GetTargetPosition(AbilityContext context)
    {
        if (targetType == TargetType.Self)
        {
            return context.actor != null ? context.actor.transform.position : Vector3.zero;
        }

        GameObject target = GameObject.FindWithTag(targetType.ToString());
        if(target != null)
        {
            return target.transform.position;
        }
        else
        {
            Fail(context);
            return Vector3.zero;
        }
    }

    public override AbilityValidationResult Validate()
    {
#if UNITY_EDITOR
        if (targetType != TargetType.Self)
        {
            string tagName = targetType.ToString();
            bool tagExists = Array.Exists(InternalEditorUtility.tags, tag => tag == tagName);

            if (!tagExists)
                return AbilityValidationResult.Invalid($"Tag '{tagName}' does not exist.");
        }
#endif

        return AbilityValidationResult.Complete();
    }
}
public enum TargetType
{
    Object,
    Enemy,
    Self
}
