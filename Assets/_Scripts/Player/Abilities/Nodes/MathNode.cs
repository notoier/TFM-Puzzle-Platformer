using System;
using UnityEngine;

[Serializable]

public class MathNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Data;

    public float valueA;
    public float valueB;

    public override void Execute(AbilityContext context)
    {

    }
}
