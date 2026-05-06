using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class MultiNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Flow;

    public List <AbilityNode> nodes = new List<AbilityNode>();
    public override void Execute(AbilityContext context)
    {

    }
}
