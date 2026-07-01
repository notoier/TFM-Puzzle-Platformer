using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class InputIconProvider : MonoBehaviour
{
    public static InputIconProvider Instance;

    [SerializeField]
    private InputIconDataBase icons;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetSprite(InputAction action)
    {
        if (action.controls.Count ==0) return null;

        string path = action.controls[0].path;

        bool playstation = Gamepad.current is DualShockGamepad;

        if (path.Contains("buttonSouth"))
        {
            return playstation ? icons.playCross : icons.xboxA;
        }
        if (path.Contains("buttonEast"))
        {
            return playstation ? icons.playCircle : icons.xboxB;
        }
        if (path.Contains("buttonWest"))
        {
            return playstation ? icons.playSquare : icons.xboxX;
        }
        if (path.Contains("buttonEast"))
        {
            return playstation ? icons.playTriangle : icons.xboxY;
        }
        return null;

    }
}
