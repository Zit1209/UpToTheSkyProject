using UnityEngine;

/// <summary>
/// Simple trap that destroys itself when the player touches it
/// Attach this to trap objects and set their tag to "Trap"
/// </summary>
public class SimpleTrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableInsteadOfDestroy = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (disableInsteadOfDestroy)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}