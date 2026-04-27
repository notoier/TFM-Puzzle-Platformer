using System;
using UnityEngine;

[Serializable]
public class TargetNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Data;

    public enum TargetType
    {
        
    }
    
    public override void Execute(AbilityContext context)
    {

    }
}
