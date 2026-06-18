using System;
using UnityEngine;

[Serializable]
public class LerpNode : FlowNode    
{

    [SerializeField]
    private float valueA, valueB;

    [SerializeReference]
    public TimerNode timer;

    

    public override void Execute(AbilityContext context)
    {

    }

    public override AbilityValidationResult Validate()
    {
        return AbilityValidationResult.Incomplete("Lerp node execution is not implemented yet.");
    }
}
