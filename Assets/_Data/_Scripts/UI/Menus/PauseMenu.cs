using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : UIPanel
{

    public void Resume()
    {
        GameManager.Instance.ResumeGame();
    }
    public void Options()
    {
        Debug.Log("Options");
    }

    public void ReturnMainMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }
    public void Quit()
    {
        Application.Quit();
    }
}

