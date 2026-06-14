using System;
using UnityEngine;

[Serializable]
public class DeathNode : ActionNode
{
    public override void Execute(AbilityContext context)
    {
        //Destroy(context.actor);
    }

    public override AbilityValidationResult Validate()
    {
        return AbilityValidationResult.Incomplete("Death node execution is not implemented yet.");
    }
}
