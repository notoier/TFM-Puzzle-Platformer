using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusControllerUI : MonoBehaviour
{
    [Header("Configuration focus")]
    [SerializeField]
    private GameObject initialButton;
    [SerializeField]
    private bool isActive = true;

    private Coroutine focusRoutine;

    private void OnEnable()
    {
        if (isActive)
        {
            FocusInitialObject();
        }
    }

    private void OnDisable()
    {
        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
        }
    }

    private void FocusInitialObject()
    {
        if (initialButton == null) return;

        if (focusRoutine != null)
            StopCoroutine(focusRoutine);
        focusRoutine = StartCoroutine(AssingFocusRoutine());
    }
    private IEnumerator AssingFocusRoutine()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        if(initialButton != null && initialButton.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(initialButton);
        }
    }
}
