using UnityEngine;

/// <summary>
/// Changes the skybox when the player enters a trigger zone
/// Attach this to a GameObject with a Collider set as "Is Trigger"
/// </summary>
[RequireComponent(typeof(Collider))]
public class SkyboxChanger : MonoBehaviour
{
    [Header("Skybox Materials")]
    [Tooltip("The skybox material to use when player enters the zone")]
    [SerializeField] private Material newSkybox;

    [Tooltip("The skybox material to restore when player exits the zone")]
    [SerializeField] private Material originalSkybox;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        // Ensure the collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // If no original skybox is assigned, save the current skybox
        if (originalSkybox == null)
        {
            originalSkybox = RenderSettings.skybox;
        }
    }

    /// <summary>
    /// Called when something enters the trigger zone
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag(playerTag))
        {
            // Change to the new skybox
            if (newSkybox != null)
            {
                RenderSettings.skybox = newSkybox;
                
                // Update the skybox reflection in the scene
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                Debug.LogWarning("New Skybox material is not assigned!");
            }
        }
    }

    /// <summary>
    /// Called when something exits the trigger zone
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited is the player
        if (other.CompareTag(playerTag))
        {
            // Restore the original skybox
            if (originalSkybox != null)
            {
                RenderSettings.skybox = originalSkybox;
                
                // Update the skybox reflection in the scene
                DynamicGI.UpdateEnvironment();
            }
        }
    }

    /// <summary>
    /// Manually change to the new skybox (can be called from other scripts)
    /// </summary>
    public void ApplyNewSkybox()
    {
        if (newSkybox != null)
        {
            RenderSettings.skybox = newSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    /// <summary>
    /// Manually restore the original skybox (can be called from other scripts)
    /// </summary>
    public void RestoreOriginalSkybox()
    {
        if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    // Visual helper in Scene view - shows the trigger zone in cyan
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan with transparency
            
            // Draw different shapes based on collider type
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}