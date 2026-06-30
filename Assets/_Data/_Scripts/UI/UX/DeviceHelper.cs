using UnityEngine;
using UnityEngine.InputSystem;

public static class DeviceHelper 
{
    public static bool UsingGamepad()
    {
        return Gamepad.current != null;
    }
}
