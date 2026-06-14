using System;
using UnityEngine;

[Serializable]
public class CancelNode : MiscNode
{
    
    public override void Execute(AbilityContext context)
    {
        Cancel(context);
    }
}
