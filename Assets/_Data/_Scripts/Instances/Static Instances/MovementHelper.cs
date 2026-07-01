using System.Collections;
using UnityEngine;

public static class MovementHelper
{
    public static IEnumerator MoveTowards(
        GameObject interactor,
        Transform target,
        float moveSpeed = 3f,
        float stopDistance = 0.08f
    )
    {
        if (!interactor || !target) yield break;

        Rigidbody2D rb = interactor.GetComponent<Rigidbody2D>();
        if (!rb) yield break;

        while (Mathf.Abs(target.position.x - rb.position.x) > stopDistance)
        {
            float directionX = Mathf.Sign(target.position.x - rb.position.x);

            Vector2 movement = new Vector2(
                directionX * moveSpeed * Time.fixedDeltaTime,
                0f
            );

            rb.MovePosition(rb.position + movement);

            yield return new WaitForFixedUpdate();
        }
    }
}