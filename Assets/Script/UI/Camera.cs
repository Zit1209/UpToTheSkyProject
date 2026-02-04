using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera tự động follow player - FIXED: Không xoay player nữa
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothSpeed = 5f;
    
    [Header("Mouse Rotation")]
    public bool enableMouseRotation = true;
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -40f;
    public float maxVerticalAngle = 80f;
    
    [Header("Cursor Control")]
    public bool lockCursorOnStart = true;
    
    private float rotationX = 0f; // Vertical rotation
    private float rotationY = 0f; // Horizontal rotation
    
    private bool isInitialized = false;
    
    void Start()
    {
        // Initialize rotation from current camera rotation
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
        
        Debug.Log($"🎥 CameraFollow started. Target: {(target != null ? target.name : "NULL - waiting for SetTarget()")}");
    }
    
    void OnEnable()
    {
        // Nếu đã có target và chưa initialize, thì initialize
        if (target != null && !isInitialized)
        {
            Initialize();
        }
    }
    
    void Initialize()
    {
        if (isInitialized) return;
        
        Debug.Log($"🎬 Initializing CameraFollow with target: {target.name}");
        
        if (lockCursorOnStart)
        {
            LockCursor();
        }
        
        // Initialize rotation từ player's rotation
        if (target != null)
        {
            rotationY = target.eulerAngles.y;
            rotationX = 0f; // Camera nhìn thẳng
        }
        
        isInitialized = true;
        Debug.Log("✅ CameraFollow initialized!");
    }
    
    void Update()
    {
        HandleCursorToggle();
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Handle mouse rotation
        if (enableMouseRotation && Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseRotation();
        }
        
        // Update camera position
        UpdateCameraPosition();
    }
    
    void HandleCursorToggle()
    {
        // Press Escape to toggle cursor lock
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
        
        // Alternative: Hold Left Alt to temporarily unlock
        if (keyboard != null)
        {
            if (keyboard.leftAltKey.isPressed)
            {
                UnlockCursor();
            }
            else if (keyboard.leftAltKey.wasReleasedThisFrame && lockCursorOnStart)
            {
                LockCursor();
            }
        }
    }
    
    void HandleMouseRotation()
    {
        // Get mouse input from new Input System
        var mouse = Mouse.current;
        if (mouse == null) return;
        
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.02f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.02f;
        
        // Update rotation
        rotationY += mouseX; // Horizontal (quanh trục Y)
        rotationX -= mouseY; // Vertical (quanh trục X)
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        // QUAN TRỌNG: Chỉ xoay CAMERA, KHÔNG xoay player
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    
    void UpdateCameraPosition()
    {
        if (target == null)
            return;
        
        // SIMPLE FOLLOW - không can thiệp vào player rotation
        if (enableMouseRotation && Cursor.lockState == CursorLockMode.Locked)
        {
            // Camera xoay tự do, player tự xoay theo input của PlayerMovement
            Quaternion horizontalRotation = Quaternion.Euler(0f, rotationY, 0f);
            Vector3 rotatedOffset = horizontalRotation * offset;
            Vector3 desiredPosition = target.position + rotatedOffset;
            
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            // Không dùng mouse rotation: simple follow
            Vector3 desiredPosition = target.position + offset;
            
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
            
            // Camera nhìn vào player
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
    
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Set target runtime
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target != null)
        {
            Debug.Log($"🎯 Camera target set to: {target.name} at position {target.position}");
            
            // Initialize nếu chưa được initialize
            Initialize();
        }
        else
        {
            Debug.LogWarning("⚠️ Camera target set to NULL!");
        }
    }
    
    /// <summary>
    /// Set offset runtime
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    /// <summary>
    /// Set mouse sensitivity runtime
    /// </summary>
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    /// <summary>
    /// Get camera's horizontal rotation (cho PlayerMovement dùng nếu cần)
    /// </summary>
    public float GetCameraYRotation()
    {
        return rotationY;
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (target == null) return;
        
        // Draw line from camera to target
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.position);
        
        // Draw sphere at target
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 0.5f);
    }
}