using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private TextMeshProUGUI text;

    private Coroutine currentRoutine;


    public void Show(string message)
    {
        if (currentRoutine != null) 
            StopCoroutine(currentRoutine);

        currentRoutine=StartCoroutine(ShowRoutine(message));

    }

    private IEnumerator ShowRoutine (string message)
    {
        panel.SetActive(true);
        text.text = message;
        yield return new WaitForSeconds(3f);
        panel.SetActive(false);

    }
}
