using System;
using UnityEngine;

[Serializable]
public class RotateNode : ActionNode
{
    public int degrees;

    public bool clockwise;


    public override void Execute(AbilityContext context)
    {
        if (context.actor == null)
        {
            Fail(context);
            return;
        }

        context.actor.transform.Rotate(0, 0, clockwise ? -degrees : degrees);
    }

    public override AbilityValidationResult Validate()
    {
        if (degrees == 0)
            return AbilityValidationResult.Incomplete("Rotate node needs degrees.");

        return AbilityValidationResult.Complete();
    }
}
