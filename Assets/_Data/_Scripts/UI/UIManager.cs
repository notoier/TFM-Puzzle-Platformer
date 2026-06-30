using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static UnityEngine.Rendering.DebugUI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField]
    private PauseMenu pauseMenu;
    /*[SerializeField]
    private OptionMenu optionsMenu;
    
    [SerializeField]
    private PromptUI prompt;
    */

    private readonly Stack<UIPanel> panelStack = new();
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameState;
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameState;
        
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= HandleGameState;
    }

    private void HandleGameState(GameState state)
    {
        Debug.Log("State: " + state);
        switch (state)
        {
            case GameState.Playing:
                CloseTopPanel();
                break;
            case GameState.Paused:
                OpenPanel(pauseMenu);
                break;
        }
 
    }
    public void CloseTopPanel()
    {
        if (panelStack.Count == 0) return;

        UIPanel panel = panelStack.Pop();
        panel.Close();
    }

    public void OpenPanel(UIPanel panel)
    {
        if (panelStack == null) return;

        panel.Open();
        panelStack.Push(panel);
    }



        
}
