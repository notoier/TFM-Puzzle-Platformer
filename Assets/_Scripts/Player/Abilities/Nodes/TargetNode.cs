using System;
using UnityEngine;

[System.Serializable]
public class TargetNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Data;

    public enum TargetType
    {
        
    }
    public Vector3 targetPosition;
    public override void Execute(AbilityContext context)
    {

    }
}
