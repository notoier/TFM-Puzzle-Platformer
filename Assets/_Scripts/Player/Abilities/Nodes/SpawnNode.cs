using System;
using UnityEngine;

[Serializable]
public class SpawnNode : ActionNode
{
    [SerializeReference]
    public ParameterNode spawnable, positon;


    public override void Execute(AbilityContext context)
    {
        if(positon.parameterType != ParameterType.Vector3) 
        {
            Debug.LogError("SpawnNode requires a Vector3 parameter for the spawn position.");
            context.success = false;
            return;
        }

        if(spawnable.parameterType != ParameterType.GameObject) 
        {
            Debug.LogError("SpawnNode requires a GameObject parameter for the spawnable object.");
            context.success = false;
            return;
        }

        GameObject prefab = spawnable.GetValue<GameObject>();   
        Vector3 pos = positon.GetValue<Vector3>();         

        GameObject spawnedObject = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        context.success = spawnedObject != null;
    }
}
