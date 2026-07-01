using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RespawnTrigger : MonoBehaviour
{
    private RespawnPoints owner;
    private Collider2D ownCollider;

    public void Initialize(RespawnPoints owner, Collider2D ownCollider)
    {
        this.owner = owner;
        this.ownCollider = ownCollider;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null || ownCollider == null)
            return;

        owner.OnTriggerDetected(ownCollider, other);
    }
}