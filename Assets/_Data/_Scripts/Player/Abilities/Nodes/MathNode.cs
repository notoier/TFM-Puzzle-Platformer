using UnityEngine;

public class MathNode : LogicNode
{
    public override AbilityValidationResult Validate()
    {
        return AbilityValidationResult.Incomplete("Math node has no operation configured yet.");
    }
}
