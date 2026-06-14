using System;
using UnityEngine;

[Serializable]
public class WeightNode : ActionNode
{
    [SerializeField]
    private WeightNodeType nodeType;

    [SerializeReference]
    private ParameterNode weightParameter;

    public override void Execute(AbilityContext context)
    {
        if (context.actor == null)
        {
            Fail(context);
            return;
        }

        IProvidesWeight actor = context.actor.GetComponent<IProvidesWeight>();
        if (actor == null)
        {
            Debug.LogError("IProvidesWeight component not found on actor.");
            Fail(context);
            return;
        }

        int weightValue = weightParameter.GetValue<int>(context);

        switch (nodeType)
        {
            case WeightNodeType.Sum:
               actor.AddWeight(weightValue);
                break;
            case WeightNodeType.Rest:
                actor.AddWeight(-weightValue);
                break;
            case WeightNodeType.Set:
                actor.Weight = weightValue;
                break;
        }
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        if (weightParameter == null)
            return AbilityValidationResult.Incomplete("Weight node needs a weight parameter.");

        return weightParameter.ValidateValue(context, AbilityValueType.Int);
    }
}

public enum WeightNodeType
{
    Sum,
    Rest,
    Set
}
