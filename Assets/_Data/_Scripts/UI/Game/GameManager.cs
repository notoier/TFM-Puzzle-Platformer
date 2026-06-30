using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<GameState> OnGameStateChanged;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if(Instance ==null)
            Instance = this;
        else
            Destroy(gameObject);

        SetState(GameState.Playing);
    }

    public void TogglePause()
    {
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

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
