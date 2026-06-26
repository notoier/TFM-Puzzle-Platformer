using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityTrigger2D : MonoBehaviour
{
    [SerializeField] private int abilityIndex;
    [SerializeField] private bool onlyOnce = true;

    private bool used;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyOnce && used)
            return;

        AbilityManager abilityManager = other.GetComponentInParent<AbilityManager>();
        if (abilityManager == null)
            return;

        abilityManager.ActivateAbility(abilityIndex);
        used = true;
    }
}