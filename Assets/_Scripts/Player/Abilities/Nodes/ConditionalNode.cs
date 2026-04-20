using System;
using TMPro.EditorUtilities;
using UnityEngine;

[System.Serializable]
public class ConditionalNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Logic;

    public AbilityNode element1, element2;

    public DropdownEditor dropdownEditor;

    public override void Execute(AbilityContext context)
    {

    }
}
