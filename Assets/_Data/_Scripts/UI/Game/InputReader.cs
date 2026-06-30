using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance;
    private PlayerInputActions actions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
        actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        actions.Enable();
        actions.UI.Pause.performed += OnPause;

    }

    private void OnDisable()
    {
        actions.UI.Pause.performed -= OnPause;
        actions.Disable();
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        GameManager.Instance.TogglePause();
    }



}
