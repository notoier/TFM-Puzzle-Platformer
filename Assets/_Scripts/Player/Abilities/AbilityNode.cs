using System;
using UnityEngine;

[Serializable]
public abstract class AbilityNode
{
    public virtual AbilityNodeCategory Category => AbilityNodeCategory.Misc;
    public abstract void Execute(AbilityContext context);
}

public enum AbilityNodeCategory
{
    Action,
    Flow,
    Logic,
    Data,
    Misc
}