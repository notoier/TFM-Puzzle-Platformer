using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Anim : MonoBehaviour
{
    [SerializeField]
    private Animator slimeAnimator;

    public void Hover()
    {
        Debug.Log("Hover");
        slimeAnimator.SetBool("Hover", true);
    }
    public void UnHover()
    {
        Debug.Log("Hover");
        slimeAnimator.SetBool("Hover", false);
    }
}
