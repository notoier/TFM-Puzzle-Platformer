using UnityEngine;

public interface IMovable
{
    Transform Transform { get; }
    Rigidbody2D Rigidbody2D { get; }
    float MoveSpeed { get; set; }
    float WeightRequired { get; set; }

    public void Move(Vector2 direction, Vector2? origin = null)
    {
        Vector2 position = origin ?? Transform.position;
        // RaycastHit2D hit = Physics2D.Raycast(position, direction, 10f);
        //
        // Debug.DrawRay(position, direction, Color.red);
        //
        // if (hit.collider.name != this.Transform.gameObject.name) return; 

        Vector2 velocity = Rigidbody2D.linearVelocity;

        velocity.x = direction.normalized.x * MoveSpeed;

        Rigidbody2D.linearVelocity = velocity;
    }
}