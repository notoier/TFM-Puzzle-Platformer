using System;
using UnityEngine;

[Serializable]
public class LerpNode : FlowNode    
{

    [SerializeReference]
    public DataNode dataNodeA, dataNodeB;

    [SerializeReference]
    public TimerNode timer;

    

    public override void Execute(AbilityContext context)
    {

    }
}
