using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class FocusManager : MonoBehaviour
{
    public static FocusManager Instance { get; private set; }

    private UIPanel currentPanel;
    private Coroutine focusRoutine;

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        GameEvents.OnDeviceChanged += HandleDeviceChanged;
    }
    private void OnDisable()
    {
        GameEvents.OnDeviceChanged -= HandleDeviceChanged;
    }


    public void FocusPanel(UIPanel panel)
    {
        if (panel == null || panel.DefaultSelected == null) 
            return;

        currentPanel = panel;
        if (focusRoutine != null)
            StopCoroutine(focusRoutine);
        focusRoutine=StartCoroutine(SetFocus(panel.DefaultSelected));
        
    }
    private void HandleDeviceChanged(InputDeviceType device)
    {
        if(device != InputDeviceType.GamePad)
        {
            ClearFocus();
        }
        else if (currentPanel!= null){
            FocusPanel(currentPanel);
        }
    }
    private IEnumerator SetFocus(GameObject target)
    {
        yield return null;
        if (EventSystem.current == null)
            yield break;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);

        focusRoutine = null;
    }

    public void ClearFocus()
    {
        if (EventSystem.current == null)
            return;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
