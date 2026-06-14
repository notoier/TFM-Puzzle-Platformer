using System;
using UnityEngine;

[System.Serializable]
public class TimerNode : FlowNode
{
    public float time;

    public override void Execute(AbilityContext context)
    {

    }

    public override AbilityValidationResult Validate()
    {
        return AbilityValidationResult.Incomplete("Timer node execution is not implemented yet.");
    }
}
