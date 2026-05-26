using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class MultiNode : FlowNode
{
    public List <AbilityNode> nodes = new List<AbilityNode>();
    public override void Execute(AbilityContext context)
    {

    }
}
