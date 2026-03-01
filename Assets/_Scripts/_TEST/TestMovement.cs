using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TestMovement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed=1f;

    [SerializeField] 
    private InputAction input;

    private Rigidbody2D rb;
    private float moveValue;
    private void OnEnable()
    {
        input?.Enable();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveValue = input.ReadValue<float>();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveValue* movementSpeed, rb.linearVelocityY);
    }

    private void OnDisable()
    {
        input?.Disable();
    }
}
