using System;
using TMPro.EditorUtilities;
using UnityEngine;

[Serializable]
public class ConditionalNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Logic;

    public AbilityNode element1, element2;

    public override void Execute(AbilityContext context)
    {

    }
}
