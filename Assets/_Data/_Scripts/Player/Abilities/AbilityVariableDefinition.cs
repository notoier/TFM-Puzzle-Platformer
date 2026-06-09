using System;

[Serializable]
public class AbilityVariableDefinition
{
    public string key;
    public AbilityValueType type;
}

public enum AbilityValueType
{
    Float,
    Int,
    Bool,
    Vector3,
    GameObject
}
