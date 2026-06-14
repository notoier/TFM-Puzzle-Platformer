using System;
using UnityEngine;

[Serializable]
public class SpawnNode : ActionNode
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Vector3 position;


    public override void Execute(AbilityContext context)
    {
        if (prefab == null)
        {
            Fail(context);
            return;
        }

        GameObject spawnedObject = GameObject.Instantiate(prefab, position, Quaternion.identity);
        if (spawnedObject != null)
            Complete(context);
        else
            Fail(context);
    }

    public override AbilityValidationResult Validate()
    {
        if (prefab == null)
            return AbilityValidationResult.Incomplete("Spawn node needs a prefab.");

        return AbilityValidationResult.Complete();
    }
}
