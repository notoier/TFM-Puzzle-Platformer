using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour, IProvidesWeight
{
    private static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    private static readonly int IsGroundedParameter = Animator.StringToHash("IsGrounded");
    private static readonly int IsWalledParameter = Animator.StringToHash("IsWalled");
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int IsTryingToMoveTrigger = Animator.StringToHash("IsTryingToMove");

    [SerializeField] private Animator animator;

    [Header("Movement Configuration")]
    [SerializeField] private float speed;
    
    [Header("Jump Configuration")]
    [SerializeField] private float jumpForce =14;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private float jumpVolume;

    [Header("Particle Configuration")]
    [SerializeField] private GameObject splashParticles;
    [SerializeField] private Transform particleSpawnPosition1;
    [SerializeField] private Transform particleSpawnPosition2;
    
    [Header("Check Wall")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallRadius = 0.15f;

    [Header("Check Ramp")]
    [SerializeField] private Transform slimeTexture;
    [SerializeField] private SpriteRenderer slimeRender;
    [SerializeField] private Transform leftPivot;
    [SerializeField] private Transform rightPivot;
    [SerializeField] private float slopeRayDistance = 1f;
    [SerializeField] private float maxSlopeOffset = 0.15f;
    [SerializeField] private float slopeRotationSpeed = 10f;
    
    [Header("Gravedad")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Water")] 
    [SerializeField] private float jumpDebuffOnWater = 0.2f;

    [Header("Slope Movement")] 
    [SerializeField] private float groundStickSpeed;
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private float groundDetectionDisableTime = 0.1f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distancia utilizada para buscar una superficie debajo.")]
    [SerializeField] private float groundProbeDistance = 0.3f;

    [Tooltip("Distancia máxima para considerar que los pies están tocando el suelo.")]
    [SerializeField] private float groundedContactDistance = 0.05f;
    
    [Header("Debug")] 
    [SerializeField] private float weightDebug = 1f;
    
    private Vector2 groundN = Vector2.up;
    private Vector3 characterMovementDirection;
    private Rigidbody2D characterRigidbody;
    
    private bool jumpStarted;
    public  bool isTryingToMove;
    private bool isGrounded;
    public  bool isWalled;
    private bool isInsideWater;
    private bool hasJumped;
    private bool jumpPressed;
    
    //Timers salto
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float groundDetectionDisableCounter;
    
    void Awake()
    {
        characterRigidbody = GetComponent<Rigidbody2D>();
        Weight = weightDebug;
    }

    private void Update()
    {
        UpdateAnimationParameters();
        UpdateFacingDirection();
        UpdateJumpBuffer();
        HandleWallPushAnimation();
        DrawDebugChecks();
    }

    private void FixedUpdate()
    {
        UpdateGroundDetectionTimer();
        DetectWall();
        DetectGround();

        UpdateCoyoteTime();
        HandleJump();

        ApplyHorizontalMovement();
        ApplyGravity();

        UpdateSlopeRotation();
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat(
            SpeedParameter,
            Mathf.Abs(characterRigidbody.linearVelocity.x)
        );

        animator.SetBool(
            IsGroundedParameter,
            isGrounded
        );

        animator.SetBool(
            IsWalledParameter,
            isWalled
        );
    }
    
    private void UpdateFacingDirection()
    {
        if (Mathf.Approximately(characterMovementDirection.x, 0f))
            return;

        Vector3 currentScale = slimeTexture.localScale;

        currentScale.x = Mathf.Sign(characterMovementDirection.x);

        slimeTexture.localScale = currentScale;
    }
    
    private void UpdateJumpBuffer()
    {
        jumpBufferCounter = Mathf.Max(
            0f,
            jumpBufferCounter - Time.deltaTime
        );
    }
    
    private void HandleWallPushAnimation()
    {
        bool pressingAgainstWall =
            isWalled &&
            (Input.GetKeyDown(KeyCode.D) ||
             Input.GetKeyDown(KeyCode.A));

        if (pressingAgainstWall)
        {
            animator.SetTrigger(IsTryingToMoveTrigger);
        }
    }
    
    private void UpdateGroundDetectionTimer()
    {
        groundDetectionDisableCounter = Mathf.Max(
            0f,
            groundDetectionDisableCounter - Time.fixedDeltaTime
        );
    }
    
    private void DetectWall()
    {
        isWalled = Physics2D.OverlapCircle(
            wallCheck.position,
            wallRadius,
            groundLayer
            
        );
    }
    
    private void DetectGround()
    {
        if (groundDetectionDisableCounter > 0f)
        {
            SetAirborne();
            return;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            groundCheck.position,
            groundRadius,
            Vector2.down,
            groundProbeDistance,
            groundLayer
        );

        RaycastHit2D groundHit = FindValidGroundHit(hits);

        if (!groundHit.collider)
        {
            SetAirborne();
            return;
        }

        groundN = groundHit.normal;

        isGrounded = groundHit.distance <= groundedContactDistance;

        if (isGrounded)
        {
            hasJumped = false;
        }
    }
    
    private RaycastHit2D FindValidGroundHit(RaycastHit2D[] hits)
    {
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider)
                continue;

            // Los triggers se ignoran, pero el raycast sigue buscando
            // el collider sólido de la plataforma.
            if (hit.collider.isTrigger && hit.transform.gameObject.layer == LayerMask.NameToLayer("LevelObject"))
                continue;

            return hit;
        }

        return default;
    }
    
    private void SetAirborne()
    {
        isGrounded = false;
        groundN = Vector2.up;
    }
    
    private void UpdateCoyoteTime()
    {
        if (isGrounded && !hasJumped)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter = Math.Max(0f, coyoteTimeCounter - Time.fixedDeltaTime);
        }
    }
    
    private void HandleJump()
    {
        bool hasBufferedJump =
            jumpBufferCounter > 0f;

        bool canUseCoyoteTime =
            coyoteTimeCounter > 0f;

        if (!hasBufferedJump ||
            !canUseCoyoteTime ||
            hasJumped)
        {
            return;
        }

        float finalJumpForce = isInsideWater
            ? jumpForce * jumpDebuffOnWater
            : jumpForce;

        characterRigidbody.linearVelocity = new Vector2(
            characterRigidbody.linearVelocity.x,
            finalJumpForce
        );

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        hasJumped = true;
        isGrounded = false;
        groundN = Vector2.up;

        groundDetectionDisableCounter =
            groundDetectionDisableTime;

        AudioManager.Instance.PlayEffect(jumpSFX, transform, jumpVolume);
        animator.SetTrigger(JumpTrigger);
        animator.SetBool(IsGroundedParameter, false);
    }
    
    private void ApplyHorizontalMovement()
    {
        float horizontalInput =
            characterMovementDirection.x;

        if (!CanUseSlopeMovement())
        {
            ApplyAirHorizontalMovement(horizontalInput);
            return;
        }

        ApplySlopeMovement(horizontalInput);
    }
    
    private bool CanUseSlopeMovement()
    {
        if (!isGrounded)
            return false;

        if (hasJumped)
            return false;

        if (isInsideWater)
            return false;

        float slopeAngle =
            Vector2.Angle(groundN, Vector2.up);

        return slopeAngle <= maxSlopeAngle;
    }
    
    private void ApplyAirHorizontalMovement(float horizontalInput)
    {
        characterRigidbody.linearVelocity = new Vector2(
            horizontalInput * speed,
            characterRigidbody.linearVelocity.y
        );
    }
    
    private void ApplySlopeMovement(float horizontalInput)
    {
        Vector2 slopeDirection = new Vector2(
            groundN.y,
            -groundN.x
        ).normalized;

        if (slopeDirection.x < 0f)
        {
            slopeDirection = -slopeDirection;
        }

        Vector2 targetVelocity =
            slopeDirection * (horizontalInput * speed);

        characterRigidbody.linearVelocity = targetVelocity;
    }
    
    private void ApplyGravity()
    {
        if (isInsideWater)
            return;

        float verticalVelocity =
            characterRigidbody.linearVelocity.y;

        if (verticalVelocity < 0f)
        {
            characterRigidbody.linearVelocity +=
                Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime);

            return;
        }

        if (verticalVelocity > 0f && !jumpPressed)
        {
            characterRigidbody.linearVelocity +=
                Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }

    private void UpdateSlopeRotation()
    {
        RaycastHit2D leftHit = GetSlopeHit(leftPivot);
        RaycastHit2D rightHit = GetSlopeHit(rightPivot);

        if (!leftHit.collider || !rightHit.collider)
        {
            RotateSlimeTowards(Quaternion.identity);
            return;
        }

        Vector2 slopeDirection =
            rightHit.point - leftHit.point;

        float angle = Mathf.Atan2(
            slopeDirection.y,
            slopeDirection.x
        ) * Mathf.Rad2Deg;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, angle);

        RotateSlimeTowards(targetRotation);
    }
    
    private RaycastHit2D GetSlopeHit(Transform pivot)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            pivot.position,
            Vector2.down,
            slopeRayDistance,
            groundLayer
        );

        return FindValidGroundHit(hits);
    }
    
    private void RotateSlimeTowards(Quaternion targetRotation)
    {
        slimeTexture.localRotation = Quaternion.Lerp(
            slimeTexture.localRotation,
            targetRotation,
            slopeRotationSpeed * Time.fixedDeltaTime
        );
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
            return;
        }

        if (!context.canceled)
            return;

        jumpPressed = false;

        if (characterRigidbody.linearVelocity.y <= 0f)
            return;

        characterRigidbody.linearVelocity = new Vector2(
            characterRigidbody.linearVelocity.x,
            characterRigidbody.linearVelocity.y * 0.5f
        );
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
    
    public void SpawnParticles()
    {
        if (!isGrounded)
            return;

        Instantiate(splashParticles, particleSpawnPosition1.position, Quaternion.identity);
        Instantiate(splashParticles, particleSpawnPosition2.position, Quaternion.identity);
    }
    
    private void DrawDebugChecks()
    {
        Debug.DrawRay(
            groundCheck.position,
            Vector2.down * groundProbeDistance,
            isGrounded ? Color.green : Color.red
        );

        Debug.DrawRay(
            leftPivot.position,
            Vector2.down * slopeRayDistance,
            Color.red
        );

        Debug.DrawRay(
            rightPivot.position,
            Vector2.down * slopeRayDistance,
            Color.blue
        );
    }
    
}

