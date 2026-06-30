using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusControllerUI : MonoBehaviour
{
    [Header("Configuration focus")]
    [SerializeField]
    private GameObject initialButton;
    //[SerializeField]
    //private bool isActive = true;

    private Coroutine focusRoutine;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(initialButton.gameObject);
    }
    private void OnEnable()
    {
        
       FocusInitialObject();
        
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

    private void Update()
    {
        if(EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(initialButton);
        }
    }
}
