using System;
using UnityEngine;

[Serializable]
public class ParameterNode : DataNode
{
    [SerializeField] private string key;

    public ParameterType parameterType;

    [SerializeField] private float floatValue;
    [SerializeField] private int intValue;
    [SerializeField] private bool boolValue;
    [SerializeField] private Vector3 vector3Value;
    [SerializeField] private GameObject gameObjectValue;

    public T GetValue<T>(AbilityContext context = null)
    {
        if (TryGetContextValue(context, out T contextValue))
            return contextValue;

        return parameterType switch
        {
            ParameterType.Float => (T)(object)floatValue,
            ParameterType.Int => (T)(object)intValue,
            ParameterType.Bool => (T)(object)boolValue,
            ParameterType.Vector3 => (T)(object)vector3Value,
            ParameterType.GameObject => (T)(object)gameObjectValue,
            _ => default
        };
    }

    public AbilityValidationResult ValidateValue(AbilityValidationContext context, AbilityValueType expectedType)
    {
        AbilityValueType actualType = GetExpectedType();
        if (actualType != expectedType)
            return AbilityValidationResult.Invalid($"Parameter node provides {actualType}, but {expectedType} is required.");

        if (!string.IsNullOrWhiteSpace(key) && context != null)
        {
            if (!context.TryGetVariable(key, out AbilityVariableDefinition variable))
                return AbilityValidationResult.Invalid($"Parameter node uses undeclared variable '{key}'.");

            if (variable.type != expectedType)
                return AbilityValidationResult.Invalid($"Variable '{key}' is {variable.type}, but {expectedType} is required.");
        }

        if (parameterType == ParameterType.GameObject && gameObjectValue == null)
            return AbilityValidationResult.Incomplete("Parameter node needs a GameObject value.");

        return AbilityValidationResult.Complete();
    }

    public override void Execute(AbilityContext context)
    {
        switch (parameterType)
        {
            case ParameterType.Float:
                context.SetFloat(key, floatValue);
                break;
            case ParameterType.Int:
                context.SetInt(key, intValue);
                break;
            case ParameterType.Bool:
                context.SetBool(key, boolValue);
                break;
            case ParameterType.Vector3:
                context.SetVector(key, vector3Value);
                break;
            case ParameterType.GameObject:
                context.SetGameObject(key, gameObjectValue);
                break;
        }

        Complete(context);
    }

    public override AbilityValidationResult Validate(AbilityValidationContext context)
    {
        return ValidateValue(context, GetExpectedType());
    }

    public override AbilityValidationResult ValidateAsRoot(AbilityValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(key))
            return AbilityValidationResult.Incomplete("Parameter node needs a key.");

        AbilityValidationResult valueValidation = ValidateValue(context, GetExpectedType());
        if (valueValidation.BlocksUse)
            return valueValidation;

        if (context != null)
        {
            if (!context.TryGetVariable(key, out AbilityVariableDefinition variable))
                return AbilityValidationResult.Invalid($"Parameter node uses undeclared variable '{key}'.");

            if (variable.type != GetExpectedType())
                return AbilityValidationResult.Invalid($"Variable '{key}' is {variable.type}, but Parameter node writes {GetExpectedType()}.");
        }

        return AbilityValidationResult.Complete();
    }

    private AbilityValueType GetExpectedType()
    {
        return parameterType switch
        {
            ParameterType.Float => AbilityValueType.Float,
            ParameterType.Int => AbilityValueType.Int,
            ParameterType.Bool => AbilityValueType.Bool,
            ParameterType.Vector3 => AbilityValueType.Vector3,
            ParameterType.GameObject => AbilityValueType.GameObject,
            _ => AbilityValueType.Float
        };
    }

    private bool TryGetContextValue<T>(AbilityContext context, out T value)
    {
        value = default;

        if (context == null || string.IsNullOrWhiteSpace(key))
            return false;

        object result = null;
        bool found = parameterType switch
        {
            ParameterType.Float => context.TryGetFloat(key, out float floatContextValue) && SetResult(floatContextValue, out result),
            ParameterType.Int => context.TryGetInt(key, out int intContextValue) && SetResult(intContextValue, out result),
            ParameterType.Bool => context.TryGetBool(key, out bool boolContextValue) && SetResult(boolContextValue, out result),
            ParameterType.Vector3 => context.TryGetVector(key, out Vector3 vectorContextValue) && SetResult(vectorContextValue, out result),
            ParameterType.GameObject => context.TryGetGameObject(key, out GameObject gameObjectContextValue) && SetResult(gameObjectContextValue, out result),
            _ => false
        };

        if (!found || result is not T typedResult)
            return false;

        value = typedResult;
        return true;
    }

    private static bool SetResult<T>(T input, out object result)
    {
        result = input;
        return true;
    }

}
public enum ParameterType
{
    Float,
    Int,
    Bool,
    Vector3,
    GameObject
}
