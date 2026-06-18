using UnityEngine;

public class WaterBodyController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        other?.GetComponent<CharacterMovement>()?.WaterEntered();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other?.GetComponent<CharacterMovement>()?.WaterExited();
    }
    
}
