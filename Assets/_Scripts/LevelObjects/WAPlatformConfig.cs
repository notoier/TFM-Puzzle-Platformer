using UnityEngine;

[CreateAssetMenu(fileName = "WAPlatformConfig", menuName = "Game/Interactable/WA Platform Config")]
public class WAPlatformConfig : ScriptableObject
{
    [Header("Weight Settings")]
    [Tooltip("Peso necesario para alcanzar el desplazamiento máximo")]
    public float requiredWeight = 1f;

    [Tooltip("Si es true, vuelve a la posición inicial al dejar de recibir peso")]
    public bool recovers = true;

    [Header("Movement Settings")]
    [Tooltip("Tiempo que tarda en empezar a moverse")]
    public float movementDelay = 0f;

    [Tooltip("Distancia máxima que puede recorrer")]
    public float maxDistance = 4f;

    [Tooltip("Velocidad de movimiento")]
    public float movementSpeed = 1f;

    [Tooltip("Si es true, se mueve instantáneamente")]
    public bool instantMovement = false;

    [Header("Return Settings")]
    [Tooltip("Si es true, al llegar al final volverá automáticamente al origen")]
    public bool returnAfterReachingEnd = false;

    [Tooltip("Tiempo de espera antes de volver al origen")]
    public float returnDelay = 0f;
}