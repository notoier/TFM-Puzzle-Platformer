using System;
using UnityEngine;

[Serializable]
public class MovementNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Action;

    public float distance;

    public override void Execute(AbilityContext context)
    {
        if (context.cancelled) return;

        context.actor.transform.position += context.direction.normalized * distance;
    }
}
