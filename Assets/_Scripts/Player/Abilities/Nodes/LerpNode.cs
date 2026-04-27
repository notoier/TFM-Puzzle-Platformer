using System;
using UnityEngine;

[Serializable]
public class LerpNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Flow;

    [SerializeReference]
    [NodeCategory(AbilityNodeCategory.Data)]
    public AbilityNode dataNodeA;

    [SerializeReference]
    [NodeCategory(AbilityNodeCategory.Data)]
    public TimerNode dataNodeB;

    [SerializeReference]
    [NodeCategory(AbilityNodeCategory.Flow)]
    public TimerNode timer;

    

    public override void Execute(AbilityContext context)
    {

    }
}
