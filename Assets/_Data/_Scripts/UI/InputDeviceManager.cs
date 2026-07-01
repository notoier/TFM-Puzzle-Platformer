using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    public InputDeviceType CurrentDevice { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    private void Update()
    {
        if(Gamepad.current !=null && Gamepad.current.wasUpdatedThisFrame)
        {
            SetDevice(InputDeviceType.GamePad);
            return;
        }
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude>0f || Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetDevice(InputDeviceType.MouseKeyboard);
            return;
        }
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SetDevice(InputDeviceType.MouseKeyboard);
        }

    }
     private void SetDevice(InputDeviceType device)
    {
        if(CurrentDevice==device) return;
        CurrentDevice = device;
        GameEvents.OnDeviceChanged?.Invoke(device);
    }

}
