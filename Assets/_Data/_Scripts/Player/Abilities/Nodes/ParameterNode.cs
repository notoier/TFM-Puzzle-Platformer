using System;
using UnityEngine;

[Serializable]
public class ParameterNode : DataNode
{
    public ParameterType parameterType;

    [SerializeField] private float floatValue;
    [SerializeField] private int intValue;
    [SerializeField] private bool boolValue;
    [SerializeField] private Vector3 vector3Value;
    [SerializeField] private GameObject gameObjectValue;

    public T GetValue<T>()
    {
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

    public override void Execute(AbilityContext context)
    {
       
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
