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
        SceneManager.LoadScene("GameMap");
    }
    public void Quit()
    {
        Application.Quit();
    }

}
