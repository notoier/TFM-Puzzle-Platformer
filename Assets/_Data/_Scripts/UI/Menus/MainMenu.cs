using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : UIPanel
{

    private void Start()
    {
        FocusManager.Instance.FocusPanel(this);
    }
    public void Play()
    {
        GameManager.Instance.LoadGame();
    }
    public void Quit()
    {
        Application.Quit();
    }

}
