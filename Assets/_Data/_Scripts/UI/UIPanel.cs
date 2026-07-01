using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{

    [SerializeField]
    private GameObject defaultSelected;

    public GameObject DefaultSelected => defaultSelected;

    public bool IsOpen { get; private set; }
    public virtual void Open()
    {
        if (IsOpen)
            return;
        IsOpen = true;
        gameObject.SetActive(true);

        OnOpened();
    }

    public virtual void Close()
    {
        if(!IsOpen)
            return;
        IsOpen=false;
        OnClosed();
        gameObject.SetActive(false);
    }

    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}
