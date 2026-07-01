
using System;
using System.Collections.Generic;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header ("BasePanels")]
    [SerializeField]
    private UIPanel pauseMenu;

    [Header("Modals")]
    [SerializeField]
    private UIPanel optionsMenu;

    private readonly Stack<UIPanel> baseStack = new();
    private UIPanel currentModal;

    public UIPanel CurrentBasePanel => baseStack.Count > 0 ? baseStack.Peek() : null;
    public UIPanel CurrentVisiblePanel => currentModal != null ? currentModal : CurrentBasePanel;
    public bool HasModalOpen => currentModal != null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    private void OnEnable()
    {
    
        GameEvents.OnGameStateChanged += HandleGameState;
       
    
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameState;
        
    }

    private void HandleGameState(GameState state)
    {
        Debug.Log("State: " + state);
        switch (state)
        {
            case GameState.Playing:
                GoBackBase();
                break;
            case GameState.Paused:
                OpenPauseMenu();
                break;
        }
 
    }

    public void OpenBase(UIPanel panel)
    {
        if (CurrentBasePanel == panel || panel ==null)
            return;

        if (CurrentBasePanel != null)
            CurrentBasePanel.Close();


        baseStack.Push(panel);
        panel.Open();
        FocusManager.Instance.FocusPanel(panel);
    }

    public void GoBackBase()
    {
        if (baseStack.Count == 0 || currentModal != null)
            return;

        
        var top = baseStack.Pop();
        top.Close();

        if (baseStack.Count > 0)
        {
            var panel = baseStack.Peek();
            panel.Open();
            FocusManager.Instance.FocusPanel(panel);
        }
        
    }

    public void OpenModal(UIPanel modalPanel)
    {
        if (currentModal != null || modalPanel==null) return;

        currentModal = modalPanel;
        currentModal.Open();
        FocusManager.Instance.FocusPanel(modalPanel);
    }

    public void CloseModal()
    {
        if (currentModal == null) return;

        currentModal.Close();
        currentModal = null;

        FocusManager.Instance.FocusPanel(CurrentBasePanel);
    }


    public void OpenPauseMenu()
    {
        OpenBase(pauseMenu);
    }
         
}
