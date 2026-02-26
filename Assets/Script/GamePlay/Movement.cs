using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player Movement Controller - SIMPLIFIED VERSION
/// - Removed wall climbing
/// - Uses CharacterController's built-in gravity
/// - Cleaner and more maintainable
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float forwardJumpBoost = 1.5f;
    [SerializeField] private float airControlStrength = 0.3f;

    [Header("Gravity")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputAsset;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // Components
    private CharacterController controller;
    
    // Input Actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    
    // Movement State
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isRunning;
    private bool isGrounded;
    private bool jumpRequested;
    
    // Knockback System
    private bool isInKnockback = false;
    private float knockbackTimer = 0f;

    private enum MovementState { Ground, Air, Knockback }
    private MovementState currentState = MovementState.Ground;

    // Animation Hashes
    private int speedHash;
    private int directionHash;
    private int jumpHash;
    private int restHash;
    private int jumpHeightHash;
    private int gravityCeHash;
    
    // Dead zone để tránh drift
    private const float INPUT_DEADZONE = 0.1f;
    
    // Gravity constant
    private float gravity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Calculate gravity from jump height
        // gravity = (2 * jumpHeight) / (timeToJumpApex^2)
        // Using Physics.gravity as base and applying multiplier
        gravity = Physics.gravity.y * gravityMultiplier;
        
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

        // Animation hashes
        speedHash = Animator.StringToHash("Speed");
        directionHash = Animator.StringToHash("Direction");
        jumpHash = Animator.StringToHash("Jump");
        restHash = Animator.StringToHash("Rest");
        jumpHeightHash = Animator.StringToHash("JumpHeight");
        gravityCeHash = Animator.StringToHash("GravityCe");
        
        // Setup Input Actions
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
        // Skip update if paused
        if (PauseManager.IsPaused())
        {
            return;
        }
        
        UpdateKnockbackTimer();
        
        if (!isInKnockback)
        {
            ReadInput();
        }
        
        CheckGrounded();
        ProcessJump();
        UpdateState();
        HandleMovement();
        UpdateAnimator();
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
        
        // Use CharacterController's built-in ground detection
        isGrounded = controller.isGrounded;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private void ProcessJump()
    {
        if (!jumpRequested)
            return;

        jumpRequested = false;

        if (isInKnockback)
            return;

        // Only jump when grounded
        if (currentState == MovementState.Ground && isGrounded)
        {
            PerformGroundJump();
        }
    }

    private void PerformGroundJump()
    {
        // Calculate jump velocity using: v = sqrt(2 * jumpHeight * gravity)
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Add forward momentum when running
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
                break;

            case MovementState.Air:
                if (isGrounded && velocity.y <= 0f)
                {
                    currentState = MovementState.Ground;
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

            case MovementState.Knockback:
                HandleKnockbackMovement();
                break;
        }

        // Move the character
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
        
        // Chỉ normalize nếu magnitude > deadzone
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

        // Apply slight downward force when grounded to stick to ground
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
        // Air control - reduced movement in air
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

    private void HandleKnockbackMovement()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        // Use CharacterController's gravity
        velocity.y += gravity * Time.deltaTime;
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
        
        // Jump height animation (0 to 1 based on velocity)
        float jumpHeight = isGrounded ? 0f : Mathf.Clamp01((velocity.y + 10f) / 20f);
        animator.SetFloat(jumpHeightHash, jumpHeight);

        // Gravity animation (0 to 1 based on fall speed)
        float gravityValue = isGrounded ? 0f : Mathf.Clamp01(-velocity.y / 30f);
        animator.SetFloat(gravityCeHash, gravityValue);
    }

    // ===== PUBLIC METHODS =====

    /// <summary>
    /// Apply knockback to player
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force)
    {
        velocity = direction * force;
        velocity.y = Mathf.Max(velocity.y, Mathf.Sqrt(jumpHeight * -2f * gravity) * 0.5f);
    }

    /// <summary>
    /// Enter knockback state with duration
    /// </summary>
    public void EnterKnockbackState(Vector3 direction, float force, float duration)
    {
        isInKnockback = true;
        knockbackTimer = duration;
        currentState = MovementState.Knockback;
        
        velocity = direction * force;
        moveInput = Vector2.zero;
    }

    /// <summary>
    /// Set camera transform reference
    /// </summary>
    public void SetCameraTransform(Transform camera)
    {
        cameraTransform = camera;
        Debug.Log($"✅ PlayerMovement: Camera reference set to {camera.name}");
    }

    /// <summary>
    /// Get current movement state
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Get current velocity
    /// </summary>
    public Vector3 GetVelocity()
    {
        return velocity;
    }
}