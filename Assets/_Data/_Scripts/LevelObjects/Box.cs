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
    
    [Header("Sound")]
    [SerializeField] private AudioClip draggingSound;
    [SerializeField] private bool dragging;
    [SerializeField] private float draggingVolume = 1f;
    
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
            StopDraggingSound();
            return;
        }

        CharacterMovement characterMovement = other.gameObject.GetComponent<CharacterMovement>();
        if (characterMovement == null) return;

        Vector2 dir = characterMovement.GetMovementDirection();

        bool isSideContact = false;

        foreach (ContactPoint2D contact in other.contacts)
        {
            // Si la normal tiene bastante componente horizontal, el contacto es lateral
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isSideContact = true;
                break;
            }
        }

        if (!isSideContact)
        {
            LockHorizontalMovement();
            StopDraggingSound();
            return;
        }

        if (Mathf.Abs(dir.x) < 0.01f)
        {
            LockHorizontalMovement();
            StopDraggingSound();
            return;
        }

        UnlockHorizontalMovement();

        if (!dragging)
        {
            dragging = true;
            AudioManager.Instance?.PlayLoopEffect(draggingSound, transform, draggingVolume);
        }

        ((IMovable)this).Move(new Vector2(dir.x, 0f));
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<IProvidesWeight>() == null) return;

        LockHorizontalMovement();

        if (dragging)
        {
            dragging = false;
            AudioManager.Instance?.StopSound(draggingSound, transform);
        }
    }
    
    private void StopDraggingSound()
    {
        if (!dragging) return;

        dragging = false;
        AudioManager.Instance?.StopSound(draggingSound, transform);
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
