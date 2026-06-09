using System.Collections.Generic;
using UnityEngine;

public class AbilityContext 
{
    public GameObject actor;

    public bool success;
    public bool cancelled;

    /* ??? */
    public bool keepActive;
    public Vector3 direction;
    
    Dictionary<string, Vector3> vectors = new Dictionary<string, Vector3>();
    Dictionary<string, float> floats = new Dictionary<string, float>();
    Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

    public RaycastHit hit;  
}