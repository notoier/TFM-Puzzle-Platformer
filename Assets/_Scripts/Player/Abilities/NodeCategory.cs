using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class NodeCategory : PropertyAttribute
{
    public AbilityNodeCategory[] validCategories;

    public NodeCategory(params AbilityNodeCategory[] categories)
    {
        validCategories = categories;
    }
}
