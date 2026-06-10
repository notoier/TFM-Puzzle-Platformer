using System;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour, IProvidesWeight
{
    private static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Speed1 = Animator.StringToHash("Speed");

    [SerializeField]
    private Animator animator;

    [Header("Movement Configuration")]
    [SerializeField]
    private float speed;
    
    [Header("Jump Configuration")]
    [SerializeField]
    private float jumpForce =14;
    [SerializeField]
    private float coyoteTime = 0.1f;
    [SerializeField]
    private float jumpBufferTime = 0.1f;
    


    [Header("Check Ground")]
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private float groundRadius = 0.15f;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Gravedad")]
    [SerializeField]
    private float fallMultiplier = 2.5f;
    [SerializeField]
    private float lowJumpMultiplier = 2f;

    [Header("Debug")] 
    [SerializeField] private float weightDebug = 1f;

    private Vector3 characterMovementDirection;
    private Rigidbody2D characterRigidbody;
    private bool isGrounded;

    //Timers salto
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    //private bool jumpHeld;
    private bool hasJumped;
    private bool jumpPressed;

    void Awake()
    {
        characterRigidbody = GetComponent<Rigidbody2D>();
        Weight = weightDebug;
    }

    private void Update()
    {

        //Separar por funciones!!!!!!!!! TO-DO

        //Animations
        animator.SetFloat(Speed1, Mathf.Abs(characterRigidbody.linearVelocity.x));
        animator.SetBool(IsGrounded, isGrounded);

        //Le damos la vuelta
        if (characterMovementDirection.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(characterMovementDirection.x), 1, 1);
        }

        //Estamos en el suelo
        Debug.DrawRay(groundCheck.position, Vector2.down * groundRadius, isGrounded ? Color.green : Color.red);

        //CoyoteTime
        if (isGrounded)
        {
            coyoteTimeCounter=coyoteTime;
        }
        else
        {
            coyoteTimeCounter-=Time.deltaTime;
        }

        //Buffer para detectar si el jugador ha saltado antes de timepo
        
        jumpBufferCounter -= Time.deltaTime;


    }

    private void FixedUpdate()
    {
        characterRigidbody.linearVelocity = new Vector2(characterMovementDirection.x * speed, characterRigidbody.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (isGrounded)
        {
            hasJumped = false;
        }

        //Buffering jump and coyote time
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !hasJumped)
        {
          
            characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, jumpForce);
            animator.SetTrigger(JumpTrigger);
            animator.SetBool(IsGrounded, false);

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            //No Doble Salto
            hasJumped = true;

        }

        switch (characterRigidbody.linearVelocity.y)
        {
            //Mejorar la gravedad y la relaci�n con el salto
            case < 0:
                characterRigidbody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
                break;
            case > 0 when !jumpPressed:
                characterRigidbody.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
                break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 vectorInput = context.ReadValue<Vector2>();
        characterMovementDirection = new Vector3(vectorInput.x, 0f, 0f);
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressed = true;
        }
        else if (context.canceled)
        {
            jumpPressed = false;

            //Corta el salto
            if(characterRigidbody.linearVelocity.y >0)
            {
                characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, characterRigidbody.linearVelocity.y* 0.5f);
            }
        }
    }

    public Vector2 GetMovementDirection()
    {
        return characterMovementDirection;
    }
    
    public float Weight { get; set; } = 2;

    public void AddWeight(float mass)
    {
        Weight += mass;
        weightDebug += mass;
    }
}

