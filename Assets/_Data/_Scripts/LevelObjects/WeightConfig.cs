using UnityEngine;

[CreateAssetMenu(fileName = "WeightConfig", menuName = "Game/Status/Weight Config")]
public class WeightConfig : ScriptableObject
{
    [Tooltip("State name")]
    public Sizes size;
    
    [Tooltip("Current weight needed for this state")]
    public float neededWeight;

    [Tooltip("Object scale")] 
    public float scale;

    [Tooltip("RigidBody2D mass")] 
    public float mass;
    
    [Tooltip("Speed Modifier")]
    public float speed;
    
    [Tooltip("Jump modifier")]
    public float jump;
}

public enum Sizes
{
    Big,
    Medium,
    Small
}