using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HorizontalSlideLimiter : MonoBehaviour
{
    [SerializeField]
    private float deceleration = 12f;

    [SerializeField]
    private float stopThreshold = 0.05f;

    private Rigidbody2D body;
    private CharacterMovement characterMovement;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        characterMovement = GetComponent<CharacterMovement>();
    }

    private void FixedUpdate()
    {
        if (characterMovement != null && Mathf.Abs(characterMovement.GetMovementDirection().x) > 0.01f)
            return;

        Vector2 velocity = body.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, 0f, deceleration * Time.fixedDeltaTime);

        if (Mathf.Abs(velocity.x) < stopThreshold)
            velocity.x = 0f;

        body.linearVelocity = velocity;
    }
}
