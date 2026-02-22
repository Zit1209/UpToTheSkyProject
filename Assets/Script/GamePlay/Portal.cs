using UnityEngine;
using System.Collections;

/// <summary>
/// Teleports the player from a trigger point to a destination point
/// Attach this to the teleport trigger GameObject
/// </summary>
[RequireComponent(typeof(Collider))]
public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform destination;
    [SerializeField] private string playerTag = "Player";

    [Header("Rotation Options")]
    [SerializeField] private bool applyDestinationRotation = false;

    [Header("Cooldown Settings")]
    [SerializeField] private float teleportCooldown = 1f;

    private bool canTeleport = true;

    private void Awake()
    {
        // Ensure trigger collider is set
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the player and teleportation is ready
        if (other.CompareTag(playerTag) && canTeleport && destination != null)
        {
            TeleportPlayer(other.transform);
        }
    }

    private void TeleportPlayer(Transform player)
    {
        // Disable teleportation temporarily to prevent loops
        canTeleport = false;

        // Handle CharacterController if present
        CharacterController controller = player.GetComponent<CharacterController>();
        
        if (controller != null)
        {
            // Disable CharacterController to allow position change
            controller.enabled = false;
            player.position = destination.position;
            controller.enabled = true;
        }
        else
        {
            // Direct position change for Rigidbody or Transform
            player.position = destination.position;
        }

        // Apply destination rotation if enabled
        if (applyDestinationRotation)
        {
            player.rotation = destination.rotation;
        }

        // Start cooldown coroutine
        StartCoroutine(TeleportCooldown());
    }

    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    /// <summary>
    /// Manually trigger teleportation (can be called from other scripts)
    /// </summary>
    public void TeleportPlayerManually(Transform player)
    {
        if (canTeleport && destination != null)
        {
            TeleportPlayer(player);
        }
    }

    /// <summary>
    /// Reset cooldown immediately
    /// </summary>
    public void ResetCooldown()
    {
        canTeleport = true;
    }

    // Visual helper in Scene view
    private void OnDrawGizmos()
    {
        if (destination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destination.position);
            Gizmos.DrawWireSphere(destination.position, 0.5f);
        }
    }
}