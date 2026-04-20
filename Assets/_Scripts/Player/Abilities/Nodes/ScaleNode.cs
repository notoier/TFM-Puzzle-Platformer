using System;
using UnityEngine;

[System.Serializable]
public class ScaleNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Action;

    public float scale;
    public override void Execute(AbilityContext context)
    {

    }
}
