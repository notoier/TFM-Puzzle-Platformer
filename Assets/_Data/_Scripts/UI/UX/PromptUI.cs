using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PromptUI : UIPanel
{
    public static PromptUI Instance;

    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI keyText;
    [SerializeField]
    private TextMeshProUGUI description;
    [SerializeField]
    private Camera cam;

    private Transform target;
    private float timer;
    

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void LateUpdate()
    {
        if (!target) return;
        timer += Time.deltaTime;
        Vector3 pos = target.position + Vector3.up * 2f;
        pos.y += Mathf.Sin(timer * 3f) * 0.1f;
        transform.position = cam.WorldToScreenPoint(pos);
    }
    public void Show(Sprite sprite, string keyboardName ,string actionDescription, Transform follow) 
    {
        bool gamepad = Gamepad.current != null;


        icon.gameObject.SetActive(gamepad);
        keyText.gameObject.SetActive(!gamepad);

        if (gamepad)
            icon.sprite = sprite;
        else
            keyText.text = keyboardName;

        description.text = actionDescription;

        target = follow;
        gameObject.SetActive(true);

    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false);
    }


}
