using System;
using UnityEngine;

[System.Serializable]
public class SpawnNode : AbilityNode
{
    public override AbilityNodeCategory Category => AbilityNodeCategory.Action;

    public GameObject spawnable;
    public override void Execute(AbilityContext context)
    {

    }
}
