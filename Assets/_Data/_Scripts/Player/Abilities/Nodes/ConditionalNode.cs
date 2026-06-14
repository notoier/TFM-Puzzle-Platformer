using System;
using UnityEngine;

[Serializable]
public class ConditionalNode : LogicNode
{
    [SerializeField]
    private float valueA, valueB;   

    public ComparisonType comparison;

    public override void Execute(AbilityContext context)
    {
        if (Evaluate(valueA, valueB, comparison))
            Complete(context);
        else
            Fail(context);
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
