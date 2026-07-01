using System;
using UnityEngine;

public static class GameEvents 
{
    public static Action OnPausedPressed;
    public static Action OnResumePressed;
    public static Action OnTogglePause;

    public static Action<GameState> OnGameStateChanged;
    public static Action<InputDeviceType> OnDeviceChanged;
}
