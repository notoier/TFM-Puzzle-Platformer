using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class MultiNode : FlowNode
{
    [SerializeField]
    private MultiNodeExecutionMode executionMode;

    [SerializeReference]
    public List <AbilityNode> nodes = new List<AbilityNode>();

    public override void Execute(AbilityContext context)
    {
        if (executionMode == MultiNodeExecutionMode.Simultaneous)
        {
            ExecuteSimultaneously(context);
            return;
        }

        ExecuteSequentially(context);
    }

    private void ExecuteSequentially(AbilityContext context)
    {
        ExecuteSequentially(context, 0);
    }

    private void ExecuteSequentially(AbilityContext context, int startIndex)
    {
        if (nodes == null)
        {
            Fail(context);
            return;
        }

        for (int i = startIndex; i < nodes.Count; i++)
        {
            AbilityNode node = nodes[i];
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
            {
                int nextIndex = i + 1;
                context.SetResumeCallback(_ => ExecuteSequentially(context, nextIndex));
                break;
            }
        }
    }

    private void ExecuteSimultaneously(AbilityContext context)
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

public enum MultiNodeExecutionMode
{
    Sequential,
    Simultaneous
}
