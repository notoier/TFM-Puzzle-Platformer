using System;
using UnityEngine;

[Serializable]
public class RotateNode : ActionNode
{
    public int degrees;

    public bool clockwise;


    public override void Execute(AbilityContext context)
    {
        context.actor.transform.Rotate(0, clockwise ? degrees : -degrees, 0);
    }
}
