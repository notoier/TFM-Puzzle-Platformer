using System;
using UnityEngine;

[System.Serializable]
public class TimerNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Flow;

    public float time;

    public override void Execute(AbilityContext context)
    {

    }
}
