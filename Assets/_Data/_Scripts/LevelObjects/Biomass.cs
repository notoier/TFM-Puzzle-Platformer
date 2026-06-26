using UnityEngine;

public class Biomass : MonoBehaviour
{
    [SerializeField] private float mass = 1f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other?.GetComponent<IProvidesWeight>()?.AddWeight(mass);
        Rigidbody2D d = other?.GetComponent<Rigidbody2D>();
        if (d) d.mass += mass * 20;
        this.gameObject.SetActive(false);
    }
}
