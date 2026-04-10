using UnityEngine;

[RequireComponent( typeof(BoxCollider2D) )]
public class Door : MonoBehaviour, IActivable
{
    public bool IsActive { get; private set;  }
    
    [SerializeField]
    private BoxCollider2D doorCollider;
    
    /* IActivable */
    public void Activate()
    {
        doorCollider.enabled = false;
        IsActive = false;
        print("Door is OPEN");
    }

    public void Deactivate()
    {
        doorCollider.enabled = true;
        IsActive = true;
        print("Door is CLOSED");
    }
}
