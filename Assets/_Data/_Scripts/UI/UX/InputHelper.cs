using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper 
{
    public static string GetDisplayName(InputAction action)
    {
        return action.GetBindingDisplayString();
    }
}
