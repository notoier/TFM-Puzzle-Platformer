using System;
using UnityEngine;

[Serializable]
public class DeathNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Action;

    public bool isDead;

    public override void Execute(AbilityContext context)
    {

    }
}
