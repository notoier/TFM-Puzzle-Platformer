using System;
using UnityEngine;

[Serializable]
public class SpawnNode : ActionNode
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private SpawnPositionSource positionSource;

    [SerializeField]
    private Vector3 position;

    [SerializeField]
    private string outputObjectKey = "spawnedObject";

    public override void Execute(AbilityContext context)
    {
        if (prefab == null)
        {
            Fail(context);
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition(context);
        GameObject spawnedObject = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);
        if (spawnedObject != null)
        {
            context.SetGameObject(outputObjectKey, spawnedObject);
            Complete(context);
        }
        else
        {
            Fail(context);
        }
    }

    public override AbilityValidationResult Validate()
    {
        if (prefab == null)
            return AbilityValidationResult.Incomplete("Spawn node needs a prefab.");

        return AbilityValidationResult.Complete();
    }

    private Vector3 GetSpawnPosition(AbilityContext context)
    {
        if (positionSource == SpawnPositionSource.ActorPosition && context.actor != null)
            return context.actor.transform.position;

        return position;
    }
}

public enum SpawnPositionSource
{
    LocalPosition,
    ActorPosition
}
