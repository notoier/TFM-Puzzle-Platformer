using System;
using UnityEngine;

[Serializable]
public class RotateNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Action;

    public int degrees;

    public bool clockwise;


    public override void Execute(AbilityContext context)
    {

    }
}
