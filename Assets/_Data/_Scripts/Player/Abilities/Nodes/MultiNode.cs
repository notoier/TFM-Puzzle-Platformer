using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class MultiNode : FlowNode
{
    [SerializeReference]
    public List <AbilityNode> nodes = new List<AbilityNode>();
    public override void Execute(AbilityContext context)
    {
        if (nodes == null)
        {
            Fail(context);
            return;
        }

        foreach (var node in nodes)
        {
            if (context.cancelled)
                break;

            if (node == null)
            {
                Fail(context);
                return;
            }

            if (!context.success)
            {
                node.DefaultBehavior(context);
                continue;
            }

            node.Execute(context);

            if (context.keepActive)
                break;
        }
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        if (nodes == null || nodes.Count == 0)
            return AbilityValidationResult.Incomplete("Multi node needs child nodes.");

        foreach (var node in nodes)
        {
            if (node == null)
                return AbilityValidationResult.Incomplete("Multi node contains an empty child node.");

            AbilityValidationResult validation = node.Validate(context);
            if (validation.BlocksUse)
                return validation;
        }

        return AbilityValidationResult.Complete();
    }
}
