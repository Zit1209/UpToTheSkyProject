using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpImpulse = 12f;
    [SerializeField] private float forwardJumpBoost = 1.8f;
    [SerializeField] private float airControlStrength = 0.3f;

    [Header("Gravity Settings")]
    [SerializeField] private float baseGravity = -25f;
    [SerializeField] private float risingGravityMultiplier = 1f;
    [SerializeField] private float fallingGravityMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = -30f;

    [Header("Climbing Settings")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float climbRunSpeed = 5f;
    [SerializeField] private float wallDetectionDistance = 0.6f;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private LayerMask wallLayer = -1;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputAsset;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isRunning;
    private bool isGrounded;
    private bool isTouchingWall;
    private Vector3 wallNormal;
    private bool jumpRequested;
    private bool climbingDisabled = false;
    private float climbDisableTimer = 0f;

    private bool isInKnockback = false;
    private float knockbackTimer = 0f;

    private enum MovementState { Ground, Air, Climb, Knockback }
    private MovementState currentState = MovementState.Ground;

    private int speedHash;
    private int directionHash;
    private int jumpHash;
    private int restHash;
    private int jumpHeightHash;
    private int gravityCeHash;
    
    // Dead zone để tránh drift
    private const float INPUT_DEADZONE = 0.1f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Auto-find camera nếu chưa assign
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform != null)
            {
                Debug.Log($"✅ Auto-found camera: {cameraTransform.name}");
            }
        }

        speedHash = Animator.StringToHash("Speed");
        directionHash = Animator.StringToHash("Direction");
        jumpHash = Animator.StringToHash("Jump");
        restHash = Animator.StringToHash("Rest");
        jumpHeightHash = Animator.StringToHash("JumpHeight");
        gravityCeHash = Animator.StringToHash("GravityCe");
        
        if (inputAsset != null)
        {
            var playerActionMap = inputAsset.FindActionMap("Player");
            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
                jumpAction = playerActionMap.FindAction("Jump");
                runAction = playerActionMap.FindAction("Run");
                
                Debug.Log("✅ Input actions found and assigned!");
            }
            else
            {
                Debug.LogError("❌ Cannot find 'Player' action map!");
            }
        }
        else
        {
            Debug.LogError("❌ InputActionAsset is NULL!");
        }
    }

    private void OnEnable()
    {
        if (moveAction != null) 
        {
            moveAction.Enable();
            Debug.Log("✅ Move action enabled");
        }
        
        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJumpPerformed;
            Debug.Log("✅ Jump action enabled");
        }
        
        if (runAction != null) 
        {
            runAction.Enable();
            Debug.Log("✅ Run action enabled");
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
        if (moveAction != null) moveAction.Disable();
        if (runAction != null) runAction.Disable();
    }

    private void Update()
    {
        UpdateKnockbackTimer();
        UpdateClimbDisableTimer();
        
        if (!isInKnockback)
        {
            ReadInput();
        }
        
        CheckGrounded();
        DetectWall();
        
        ProcessJump();
        
        UpdateState();
        HandleMovement();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        float speedValue = 0f;
        
        if (moveInput.magnitude > INPUT_DEADZONE)
        {
            speedValue = isRunning ? 1f : 0.5f; 
        }
        
        animator.SetFloat(speedHash, speedValue);
        animator.SetFloat(directionHash, moveInput.x);
        
        bool isResting = isGrounded && moveInput.magnitude < INPUT_DEADZONE && !isInKnockback;
        animator.SetBool(restHash, isResting);
        
        float jumpHeight = isGrounded ? 0f : Mathf.Clamp01((velocity.y + 10f) / 20f);
        animator.SetFloat(jumpHeightHash, jumpHeight);

        float gravityValue = isGrounded ? 0f : Mathf.Clamp01(-velocity.y / maxFallSpeed);
        animator.SetFloat(gravityCeHash, gravityValue);
    }

    private void UpdateKnockbackTimer()
    {
        if (isInKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isInKnockback = false;
                currentState = MovementState.Air;
            }
        }
    }

    private void UpdateClimbDisableTimer()
    {
        if (climbingDisabled)
        {
            climbDisableTimer -= Time.deltaTime;
            if (climbDisableTimer <= 0f)
            {
                climbingDisabled = false;
            }
        }
    }

    private void ReadInput()
    {
        Vector2 rawInput = Vector2.zero;
        
        if (moveAction != null)
        {
            rawInput = moveAction.ReadValue<Vector2>();
        }
        
        // Apply deadzone
        if (rawInput.magnitude < INPUT_DEADZONE)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = rawInput;
        }
        
        // Debug log nếu bật
        if (showDebugLogs && moveInput.magnitude > 0f)
        {
            Debug.Log($"📍 MoveInput: {moveInput} (magnitude: {moveInput.magnitude:F3})");
        }
        
        if (runAction != null)
        {
            isRunning = runAction.IsPressed();
        }
    }

    private void CheckGrounded()
    {
        if (isInKnockback)
        {
            isGrounded = false;
            return;
        }
        
        isGrounded = controller.isGrounded;
    }

    private void DetectWall()
    {
        if (climbingDisabled || isInKnockback)
        {
            isTouchingWall = false;
            return;
        }

        isTouchingWall = false;

        Vector3[] directions = {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, wallDetectionDistance, wallLayer))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < 0.1f)
                {
                    isTouchingWall = true;
                    wallNormal = hit.normal;
                    break;
                }
            }
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isInKnockback)
        {
            jumpRequested = true;
        }
    }

    private void ProcessJump()
    {
        if (!jumpRequested || isInKnockback)
            return;

        jumpRequested = false;

        switch (currentState)
        {
            case MovementState.Ground:
                if (isGrounded)
                {
                    PerformGroundJump();
                }
                break;

            case MovementState.Climb:
                PerformWallJump();
                break;
        }
    }

    private void PerformGroundJump()
    {
        velocity.y = jumpImpulse;

        if (isRunning && moveInput.magnitude > INPUT_DEADZONE)
        {
            Vector3 moveDirection = GetCameraRelativeMovement();
            velocity.x += moveDirection.x * runSpeed * (forwardJumpBoost - 1f);
            velocity.z += moveDirection.z * runSpeed * (forwardJumpBoost - 1f);
        }

        if (animator != null)
        {
            animator.SetTrigger(jumpHash);
        }

        currentState = MovementState.Air;
    }

    private void PerformWallJump()
    {
        velocity = wallNormal * wallJumpForce;
        velocity.y = jumpImpulse * 0.9f;

        if (animator != null)
        {
            animator.SetTrigger(jumpHash);
        }

        currentState = MovementState.Air;
    }

    private void UpdateState()
    {
        if (isInKnockback)
            return;

        switch (currentState)
        {
            case MovementState.Ground:
                if (!isGrounded && velocity.y < 0f)
                {
                    currentState = MovementState.Air;
                }
                else if (isTouchingWall && !isGrounded && !climbingDisabled)
                {
                    currentState = MovementState.Climb;
                }
                break;

            case MovementState.Air:
                if (isGrounded && velocity.y <= 0f)
                {
                    currentState = MovementState.Ground;
                }
                else if (isTouchingWall && velocity.y <= 0f && !climbingDisabled)
                {
                    currentState = MovementState.Climb;
                    velocity = Vector3.zero;
                }
                break;

            case MovementState.Climb:
                if (isGrounded || climbingDisabled)
                {
                    currentState = MovementState.Ground;
                }
                else if (!isTouchingWall)
                {
                    currentState = MovementState.Air;
                }
                break;
        }
    }

    private void HandleMovement()
    {
        switch (currentState)
        {
            case MovementState.Ground:
                HandleGroundMovement();
                break;

            case MovementState.Air:
                HandleAirMovement();
                break;

            case MovementState.Climb:
                HandleClimbingMovement();
                break;

            case MovementState.Knockback:
                HandleKnockbackMovement();
                break;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private Vector3 GetCameraRelativeMovement()
    {
        // Nếu không có input, return zero
        if (moveInput.magnitude < INPUT_DEADZONE)
        {
            return Vector3.zero;
        }
        
        if (cameraTransform == null)
        {
            Debug.LogWarning("⚠️ Camera transform is NULL! Using player forward.");
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            return transform.TransformDirection(inputDirection).normalized;
        }
        
        // Lấy camera forward và right, BỎ component Y
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Tính movement direction
        Vector3 direction = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        
        // QUAN TRỌNG: Chỉ normalize nếu magnitude > deadzone
        if (direction.magnitude > INPUT_DEADZONE)
        {
            return direction.normalized;
        }
        
        return Vector3.zero;
    }

    private void HandleGroundMovement()
    {
        // Nếu không có input, DỪNG LẠI
        if (moveInput.magnitude < INPUT_DEADZONE)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
        else
        {
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 moveDirection = GetCameraRelativeMovement();
            
            velocity.x = moveDirection.x * targetSpeed;
            velocity.z = moveDirection.z * targetSpeed;
            
            // Xoay player theo hướng di chuyển
            if (moveDirection.magnitude > INPUT_DEADZONE)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        if (isGrounded && velocity.y <= 0f)
        {
            velocity.y = -2f;
        }
        else
        {
            ApplyGravity();
        }
    }

    private void HandleAirMovement()
    {
        if (moveInput.magnitude > INPUT_DEADZONE)
        {
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 moveDirection = GetCameraRelativeMovement();
            Vector3 airVelocity = moveDirection * targetSpeed * airControlStrength;

            velocity.x += airVelocity.x * Time.deltaTime;
            velocity.z += airVelocity.z * Time.deltaTime;
        }

        ApplyGravity();
    }

    private void HandleClimbingMovement()
    {
        float speed = isRunning ? climbRunSpeed : climbSpeed;

        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(wallNormal, up).normalized;

        Vector3 climbDirection = (up * moveInput.y) + (right * moveInput.x);

        velocity = climbDirection * speed;
    }

    private void HandleKnockbackMovement()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        float gravityMultiplier = velocity.y > 0f ? risingGravityMultiplier : fallingGravityMultiplier;
        
        velocity.y += baseGravity * gravityMultiplier * Time.deltaTime;
        
        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isInKnockback)
            return;

        if (currentState == MovementState.Air)
        {
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.1f)
            {
                isTouchingWall = true;
                wallNormal = hit.normal;
            }
        }
    }

    public void ForceExitClimb()
    {
        if (currentState == MovementState.Climb)
        {
            currentState = MovementState.Air;
        }
    }

    public void DisableClimbForSeconds(float duration)
    {
        climbingDisabled = true;
        climbDisableTimer = duration;
        
        if (currentState == MovementState.Climb)
        {
            currentState = MovementState.Air;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        velocity = direction * force;
        velocity.y = Mathf.Max(velocity.y, jumpImpulse * 0.5f);
    }

    public void EnterKnockbackState(Vector3 direction, float force, float duration)
    {
        isInKnockback = true;
        knockbackTimer = duration;
        currentState = MovementState.Knockback;
        
        climbingDisabled = true;
        climbDisableTimer = duration;
        
        velocity = direction * force;
        
        moveInput = Vector2.zero;
    }

    public bool IsClimbing()
    {
        return currentState == MovementState.Climb;
    }
    
    public void SetCameraTransform(Transform camera)
    {
        cameraTransform = camera;
        Debug.Log($"✅ PlayerMovement: Camera reference set to {camera.name}");
    }
}