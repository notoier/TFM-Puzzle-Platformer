using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & interactableLayer) == 0)
            return;

        currentInteractable = other.GetComponent<IInteractable>();

        if (currentInteractable == null)
            return;

        Debug.Log($"Interactable detected: {other.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & interactableLayer) == 0)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null && interactable == currentInteractable)
            currentInteractable = null;
    }

    public void TryInteract(GameObject interactor)
    {
        currentInteractable?.Interact(interactor);
    }
}