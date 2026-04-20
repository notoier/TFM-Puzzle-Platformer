using System;
using UnityEngine;

[System.Serializable]
public class LerpNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Flow;


    public TimerNode time;

    public AbilityNode node;

    public override void Execute(AbilityContext context)
    {

    }
}
