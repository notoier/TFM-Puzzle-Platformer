using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour, IProvidesWeight
{
    private static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsWalled = Animator.StringToHash("IsWalled");
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

    [Header("Particle Configuration")]
    [SerializeField]
    private GameObject SplashParticles;
    [SerializeField]
    private Transform particleSpawnPosition1;
    [SerializeField]
    private Transform particleSpawnPosition2;


    [Header("Check Ground")]
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private float groundRadius = 0.15f;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Check Wall")]
    [SerializeField]
    private Transform wallCheck;
    [SerializeField]
    private float wallRadius = 0.15f;

    [Header("Check Ramp")]
    [SerializeField]
    private Transform slimeTexture;
    [SerializeField]
    private SpriteRenderer slimeRender;
    [SerializeField]
    private Transform leftPivot;
    [SerializeField]
    private Transform rightPivot;
    [SerializeField]
    private float slopeRayDistance = 1f;
    [SerializeField]
    private float maxSlopeOffset = 0.15f;
    [SerializeField]
    private float slopeRotationSpeed = 10f;
    

    [Header("Gravedad")]
    [SerializeField]
    private float fallMultiplier = 2.5f;
    [SerializeField]
    private float lowJumpMultiplier = 2f;

    [Header("Status")] 
    [SerializeField] private float jumpDebuffOnWater = 0.2f;
    
    [Header("Debug")] 
    [SerializeField] private float weightDebug = 1f;
    public bool isTryingToMove;

    private Vector3 characterMovementDirection;
    private Rigidbody2D characterRigidbody;
    private bool isGrounded;
    public bool isWalled = false;
    private bool isInsideWater;

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
        animator.SetBool(IsWalled, isWalled);

        //Le damos la vuelta
        /*if (characterMovementDirection.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(characterMovementDirection.x), 1, 1);
        }*/

        if (characterMovementDirection.x < 0)
        {
            slimeRender.flipX=false;
            slimeTexture.localScale = new Vector3(Mathf.Sign(characterMovementDirection.x), 1, 1);

        }
        else if (characterMovementDirection.x > 0)
        {
            slimeRender.flipX = true;
            slimeTexture.localScale = new Vector3(Mathf.Sign(-characterMovementDirection.x), 1, 1);
        }
        


        //Estamos en el suelo
        Debug.DrawRay(groundCheck.position, Vector2.down * groundRadius, isGrounded ? Color.green : Color.red);
        //Estamos contra pared
        Debug.DrawRay(wallCheck.position, Vector2.down * wallRadius, isWalled ? Color.blue : Color.red);

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

        if (isWalled && Input.GetKeyDown(KeyCode.D) || isWalled && Input.GetKeyDown(KeyCode.A))
            animator.SetTrigger("IsTryingToMove");

    }

    private void FixedUpdate()
    {
        characterRigidbody.linearVelocity = new Vector2(characterMovementDirection.x * speed, characterRigidbody.linearVelocity.y);

        isWalled = Physics2D.OverlapCircle(wallCheck.position, wallRadius, groundLayer);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        if (isGrounded)
        {
            hasJumped = false;
        }

        //Buffering jump and coyote time
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !hasJumped)
        {
          
            characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, 
                                                            isInsideWater ? jumpForce * jumpDebuffOnWater 
                                                                          : jumpForce);
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

        UpdateRotationRamp();
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
            //spawnParticles();
        }
        else if (context.canceled)
        {
            jumpPressed = false;

            //Corta el salto
            if(characterRigidbody.linearVelocity.y > 0)
            {
                characterRigidbody.linearVelocity = new Vector2(characterRigidbody.linearVelocity.x, characterRigidbody.linearVelocity.y * 0.5f);
            }
        }
    }

    public Vector2 GetMovementDirection()
    {
        return characterMovementDirection;
    }

    public void WaterEntered()
    {
        isInsideWater = true;
    }

    public void WaterExited()
    {
        isInsideWater = false;
    }

    public float Weight { get; set; } = 2;

    public void AddWeight(float mass)
    {
        Weight += mass;
        weightDebug += mass;
    }

    private void UpdateRotationRamp()
    {
        RaycastHit2D lHit = Physics2D.Raycast(leftPivot.position, Vector2.down, slopeRayDistance, groundLayer);
        RaycastHit2D rHit = Physics2D.Raycast(rightPivot.position, Vector2.down, slopeRayDistance, groundLayer);

        Debug.DrawRay(leftPivot.position, Vector2.down * slopeRayDistance, Color.red);
        Debug.DrawRay(rightPivot.position, Vector2.down * slopeRayDistance, Color.blue);
        if (!lHit || !rHit)
        {
            
            slimeTexture.localRotation = Quaternion.Lerp(slimeTexture.localRotation, Quaternion.identity, slopeRotationSpeed * Time.deltaTime);
            return;
        }

        Vector2 slopeDirection = rHit.point - lHit.point;
        float angle = Mathf.Atan2(slopeDirection.y, slopeDirection.x) *Mathf.Rad2Deg;
        

        slimeTexture.localRotation = Quaternion.Lerp(slimeTexture.localRotation, Quaternion.Euler(0f,0f,angle), slopeRotationSpeed * Time.deltaTime);

    }

    public void spawnParticles()
    {
        if (!isGrounded)
            return;

        Instantiate(SplashParticles, particleSpawnPosition1.position, Quaternion.identity);
        Instantiate(SplashParticles, particleSpawnPosition2.position, Quaternion.identity);
    }


}

