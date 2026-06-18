using System;
using UnityEngine;

[Serializable]
public class VisibilityNode : ActionNode    
{
    [SerializeField]
    private bool isVisible;

    [SerializeField]
    private VisibilityValueSource valueSource;

    [SerializeReference]
    private ParameterNode visibilityParameter;

    public override void Execute(AbilityContext context)
    {
        if (context.actor == null)
        {
            Fail(context);
            return;
        }

      
        bool resolvedVisibility = GetVisibility(context);
        Renderer[] renderers = context.actor.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = resolvedVisibility;
        }
        
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        if (valueSource == VisibilityValueSource.ParameterNode)
        {
            if (visibilityParameter == null)
                return AbilityValidationResult.Incomplete("Visibility node needs a visibility parameter.");

            return visibilityParameter.ValidateValue(context, AbilityValueType.Bool);
        }

        return AbilityValidationResult.Complete();
    }

    private bool GetVisibility(AbilityContext context)
    {
        return valueSource == VisibilityValueSource.ParameterNode && visibilityParameter != null
            ? visibilityParameter.GetValue<bool>(context)
            : isVisible;
    }
}

public enum VisibilityValueSource
{
    LocalValue,
    ParameterNode
}
