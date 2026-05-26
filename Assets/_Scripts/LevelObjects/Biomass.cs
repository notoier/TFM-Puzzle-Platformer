using UnityEngine;

public class Biomass : MonoBehaviour
{
    [SerializeField] private float mass = 1f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other?.GetComponent<IProvidesWeight>()?.AddWeight(mass);
        this.gameObject.SetActive(false);
    }
}
