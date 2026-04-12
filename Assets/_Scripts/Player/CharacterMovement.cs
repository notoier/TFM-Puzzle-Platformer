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

    [SerializeField]
    private Animator animator;

    [Header("Movement Configuration")]
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float JumpForce;

    [Header("Check Ground")]
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private float groundRadius = 0.15f;
    [SerializeField]
    private LayerMask groundLayer;


    private Vector3 characterMovementDirection;
    private Rigidbody2D characterRigidbody;
    private bool isGrounded;

    void Start()
    {
        characterRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Animations
        animator.SetFloat("Speed", Mathf.Abs(characterRigidbody.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);

        //Le damos la vuelta
        if (characterMovementDirection.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(-characterMovementDirection.x), 1, 1);
        }

        //Estamos en el suelo
        Debug.DrawRay(groundCheck.position, Vector2.down * groundRadius, isGrounded ? Color.green : Color.red);
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        characterRigidbody.linearVelocity = new Vector2(characterMovementDirection.x * Speed, characterRigidbody.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    public void OnMove(InputValue Value)
    {
        Vector2 vectorInput = Value.Get<Vector2>();
        characterMovementDirection = new Vector3(vectorInput.x, 0f, 0f);
        characterMovementDirection = characterMovementDirection.normalized;
    }

    public void OnJump(InputValue Value)
    {
        if (!Value.isPressed) return;
        if (!isGrounded) return;

        characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, JumpForce);
        animator.SetTrigger("JumpTrigger");
    }
}

