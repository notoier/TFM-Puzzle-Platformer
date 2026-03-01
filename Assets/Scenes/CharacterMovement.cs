using System;
using System.Collections;
using Unity.VersionControl.Git;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Configuration")]
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float JumpForce;


    private Vector3 characterMovementDirection;
    private Rigidbody2D characterRigidbody;


    void Start()
    {
        characterRigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        characterRigidbody.linearVelocity = new Vector2(characterMovementDirection.x * Speed, characterRigidbody.linearVelocity.y);
    }
   
    public void OnMove(InputValue Value)
    {
        Vector2 vectorInput = Value.Get<Vector2>();
        characterMovementDirection = new Vector3(vectorInput.x, 0f, 0f);
        characterMovementDirection = characterMovementDirection.normalized;
    }

    public void OnJump(InputValue Value)
    {
        characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, JumpForce);
    }
}

