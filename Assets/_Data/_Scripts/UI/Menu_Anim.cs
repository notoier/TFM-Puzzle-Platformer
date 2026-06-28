using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu_Anim : MonoBehaviour
{
    [SerializeField]
    private Animator slimeAnimator;
    [SerializeField]
    private Image slime;

    private Color ogColor;

    private void Awake()
    {
        ogColor = slime.color;

    }
    public void Hover()
    {
        Debug.Log("Hover");
        slimeAnimator.SetBool("Hover", true);
        slime.color = new Color32 (223, 144, 236, 255);
    }
    public void UnHover()
    {
        Debug.Log("Hover");
        slimeAnimator.SetBool("Hover", false);
        slime.color = ogColor;
    }
}
