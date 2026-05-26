using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButtonConfig", menuName = "Game/Interactable/Button Config")]
public class ButtonConfig : ScriptableObject
{
    [Header("Weight Settings")]
    [Tooltip("Peso necesario para que el botón se active")]
    public float requiredWeight = 1f;

    [Tooltip("Si es true, el botón se mantiene activo mientras haya suficiente peso")]
    public bool holdToActivate = true;

    [Tooltip("Si es false, el botón solo se activa una vez")]
    public bool canDeactivate = true;

    [Header("Activation Settings")]
    [Tooltip("Tiempo que tarda en activarse (opcional)")]
    public float activationDelay = 0f;

    [Tooltip("Tiempo que tarda en desactivarse (opcional)")]
    public float deactivationDelay = 0f;
}