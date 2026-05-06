using System;
using UnityEngine;

[Serializable]
public class MovementNode : ActionNode
{
    public float distance;

    [SerializeReference]
    public TargetNode targetNode;

    public override void Execute(AbilityContext context)
    {
        if (context.cancelled) return;

        context.actor.transform.position += context.direction.normalized * distance;
    }
}
