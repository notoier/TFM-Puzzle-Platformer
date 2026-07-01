using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if(Instance ==null)
            Instance = this;
        else
            Destroy(gameObject);  

        SetState(GameState.MainMenu);
    }

    private void OnEnable()
    {
        GameEvents.OnPausedPressed += TogglePause;
    }
    private void OnDisable()
    {
        GameEvents.OnPausedPressed -= TogglePause;
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.MainMenu) return;

            if (CurrentState == GameState.Paused)
            ResumeGame();
        else if (CurrentState == GameState.Playing)
            PauseGame();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
            return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;

    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
    }

    private void SetState(GameState newState)
    {
        if (CurrentState==newState)
            return;
        CurrentState = newState;
        GameEvents.OnGameStateChanged?.Invoke(newState);
    }

    public void LoadMainMenu()
    {
        SetState(GameState.MainMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadGame()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameMap");
    }
}
