using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Box : MonoBehaviour, IMovable, IProvidesWeight
{
    public Transform Transform => transform;
    public Rigidbody2D Rigidbody2D => GetComponent<Rigidbody2D>();
    public float MoveSpeed { get; set; }= 2f;
    public float WeightRequired { get; set; } = 2f;
    public float Weight { get; set; } = 2f;
    
    private Rigidbody2D _rb;
    
    [Header("Debug")]
    [SerializeField] private float debugWeight;
    [SerializeField] private float debugWeightRequired;
    [SerializeField] private float debugMoveSpeed;

    private void Awake()
    {
        MoveSpeed = debugMoveSpeed;
        WeightRequired = debugWeightRequired;
        Weight = debugWeight;

        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        LockHorizontalMovement();
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        IProvidesWeight weightProvider = other.gameObject.GetComponent<IProvidesWeight>();
        if (weightProvider == null) return;

        if (weightProvider.Weight < WeightRequired)
        {
            LockHorizontalMovement();
            return;
        }

        if (other.transform.position.y > this.transform.position.y * transform.lossyScale.y) return;
        UnlockHorizontalMovement();

        Vector2 dir = other.gameObject.GetComponent<CharacterMovement>()?.GetMovementDirection() ?? Vector2.zero;
        ((IMovable)this).Move(dir);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<IProvidesWeight>() == null) return;

        LockHorizontalMovement();
    }

    private void LockHorizontalMovement()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void UnlockHorizontalMovement()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
