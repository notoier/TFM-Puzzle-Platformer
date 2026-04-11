using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WAPlatformConfig", menuName = "Game/Interactable/WA Platform Config")]
public class WAPlatformConfig : ScriptableObject
{
    [Header("Weight Settings")]
    [Tooltip("Peso necesario para que la plataforma se active")]
    public float requiredWeight = 1f;

    [Tooltip("Si es true, la plataforma vuelve a la posición original al quitar el peso de encima")]
    public bool recovers = true;

    [Header("Activation Settings")]
    [Tooltip("Tiempo que tarda en empezar a moverse (opcional)")]
    public float movementDelay = 0f;
    
    [Header("Behaviour Settings")]
    [Tooltip("Distancia maxima a la que se puede mover")]
    public float maxDistance = 4f;
    
    [Tooltip("Velocidad a la que se puede mover")]
    public float movementSpeed = 1f;

    [Tooltip("Si es true, la traslacion se realizara de manera inmediata (la plataforma se teletransporta)")]
    public bool instantMovement = false;
}
