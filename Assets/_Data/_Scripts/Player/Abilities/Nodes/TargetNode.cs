using System;
using UnityEngine;

[Serializable]
public class TargetNode : DataNode
{
    
    public TargetType targetType;

    public override void Execute(AbilityContext context)
    {
        context.direction = GetTargetPosition(context);
    }

    private Vector3 GetTargetPosition(AbilityContext context)
    {
        GameObject target = GameObject.FindWithTag(targetType.ToString());
        if(target != null)
        {
            return target.transform.position;
        }
        else
        {
            context.success = false;
            return Vector3.zero;
        }
    }
}
public enum TargetType
{
    Object,
    Enemy,
    Self
}
