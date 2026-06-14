using System;
using UnityEngine;

[Serializable]
public class ScaleNode : ActionNode
{
    public float scale;

    [SerializeField]
    private ScaleValueSource valueSource;

    [SerializeReference]
    private ParameterNode scaleParameter;

    public override void Execute(AbilityContext context)
    {

        if (context.actor == null)
        {
            Fail(context);
            return;
        }

        float resolvedScale = GetScale(context);
        Vector3 currentScale = context.actor.transform.localScale;
        float xSign = Mathf.Sign(currentScale.x);
        if (Mathf.Approximately(xSign, 0f))
            xSign = 1f;

        context.actor.transform.localScale = new Vector3(xSign * resolvedScale, resolvedScale, currentScale.z);
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        if (valueSource == ScaleValueSource.ParameterNode)
        {
            if (scaleParameter == null)
                return AbilityValidationResult.Incomplete("Scale node needs a scale parameter.");

            return scaleParameter.ValidateValue(context, AbilityValueType.Float);
        }

        return AbilityValidationResult.Complete();  
    }

    private float GetScale(AbilityContext context)
    {
        return valueSource == ScaleValueSource.ParameterNode && scaleParameter != null
            ? scaleParameter.GetValue<float>(context)
            : scale;
    }
}

public enum ScaleValueSource
{
    LocalValue,
    ParameterNode
}
