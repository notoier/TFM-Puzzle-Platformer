using UnityEngine;

public class ChainSounds : MonoBehaviour
{
    [SerializeField] private AudioClip chainHitSound;

    private void Awake()
    {
        foreach (Rigidbody2D childBody in
                 GetComponentsInChildren<Rigidbody2D>(true))
        {
            ChainCollisionRelay relay =
                childBody.GetComponent<ChainCollisionRelay>();

            if (relay == null)
                relay = childBody.gameObject.AddComponent<ChainCollisionRelay>();

            relay.Initialize(this);
        }
    }

    public void PlayHitSound(Transform emitter)
    {
        if (chainHitSound == null)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEffect(
                chainHitSound, emitter, 1f, 0.9f, 1.1f);
        else
            AudioSource.PlayClipAtPoint(chainHitSound, emitter.position);
    }
}

public class ChainCollisionRelay : MonoBehaviour
{
    private ChainSounds chainSounds;

    public void Initialize(ChainSounds owner)
    {
        chainSounds = owner;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        chainSounds.PlayHitSound(transform);
    }
}
