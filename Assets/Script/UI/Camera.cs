using UnityEngine;

/// <summary>
/// Camera tự động follow player
/// Script này sẽ tự động add vào Camera bởi PlayerBundleLoader
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float smoothSpeed = 5f;
    public bool lookAtTarget = true;
    
    [Header("Rotation (Optional)")]
    public bool allowRotation = false;
    public float rotationSpeed = 3f;
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        
        transform.position = smoothedPosition;
        
        // Look at target
        if (lookAtTarget)
        {
            if (allowRotation)
            {
                // Smooth rotation
                Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // Direct look at
                transform.LookAt(target);
            }
        }
    }
    
    /// <summary>
    /// Set target runtime
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Set offset runtime
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}