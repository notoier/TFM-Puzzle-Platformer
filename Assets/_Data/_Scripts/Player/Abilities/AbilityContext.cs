using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityContext 
{
    public GameObject actor;
    public MonoBehaviour coroutineRunner;

    public bool success = true;
    public bool cancelled;
    public bool keepActive;
    public bool finished;
    private Action<AbilityContext> finishCallback;

    Dictionary<string, Vector3> vectors = new Dictionary<string, Vector3>();
    Dictionary<string, float> floats = new Dictionary<string, float>();
    Dictionary<string, int> ints = new Dictionary<string, int>();
    Dictionary<string, bool> bools = new Dictionary<string, bool>();
    Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

    public Vector3 direction;
    public RaycastHit hit;

    public void Complete()
    {
        success = true;
    }

    public void Fail()
    {
        success = false;
    }

    public void Cancel()
    {
        cancelled = true;
    }

    public void KeepActive()
    {
        keepActive = true;
    }

    public void Finish()
    {
        if (finished)
            return;

        finished = true;
        keepActive = false;
        finishCallback?.Invoke(this);
    }

    public void SetFinishCallback(Action<AbilityContext> callback)
    {
        finishCallback = callback;
    }

    public void SetVector(string key, Vector3 value)
    {
        if (string.IsNullOrEmpty(key)) return;
        vectors[key] = value;
    }

    public bool TryGetVector(string key, out Vector3 value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        return vectors.TryGetValue(key, out value);
    }

    public void SetFloat(string key, float value)
    {
        if (string.IsNullOrEmpty(key)) return;
        floats[key] = value;
    }

    public bool TryGetFloat(string key, out float value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        return floats.TryGetValue(key, out value);
    }

    public void SetInt(string key, int value)
    {
        if (string.IsNullOrEmpty(key)) return;
        ints[key] = value;
    }

    public bool TryGetInt(string key, out int value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        return ints.TryGetValue(key, out value);
    }

    public void SetBool(string key, bool value)
    {
        if (string.IsNullOrEmpty(key)) return;
        bools[key] = value;
    }

    public bool TryGetBool(string key, out bool value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        return bools.TryGetValue(key, out value);
    }

    public void SetGameObject(string key, GameObject value)
    {
        if (string.IsNullOrEmpty(key)) return;
        gameObjects[key] = value;
    }

    public bool TryGetGameObject(string key, out GameObject value)
    {
        if (string.IsNullOrEmpty(key))
        {
            value = default;
            return false;
        }

        return gameObjects.TryGetValue(key, out value);
    }
}
