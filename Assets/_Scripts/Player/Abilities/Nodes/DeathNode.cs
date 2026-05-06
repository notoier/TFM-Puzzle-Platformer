using System;
using UnityEngine;

[Serializable]
public class DeathNode : ActionNode
{
    public override void Execute(AbilityContext context)
    {
        //Destroy(context.actor);
    }
}
