using System;
using UnityEngine;

[Serializable]
public class ConditionalNode : FlowNode
{
    [SerializeReference]
    public DataNode element1, element2;
    

    public ComparisonType comparison;
    public override void Execute(AbilityContext context)
    {
        context.success = Evaluate(element1.value, element2.value, comparison);
    }
    private bool Evaluate(float a, float b, ComparisonType comp)
    {
        switch (comp)
        {
            case ComparisonType.Equal:
                return a == b;

            case ComparisonType.NotEqual:
                return a != b;

            case ComparisonType.GreaterThan:
                return a > b;

            case ComparisonType.LessThan:
                return a < b;

            case ComparisonType.GreaterThanOrEqual:
                return a >= b;

            case ComparisonType.LessThanOrEqual:
                return a <= b;

            default:
                return false;
        }
    }
}
public enum ComparisonType
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}
