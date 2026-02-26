using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple trigger that loads Win Scene when player touches it
/// Attach this to a GameObject with a trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class WinZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string playerTag = "Player";
    
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("🏆 Player WIN!");
            SceneManager.LoadScene(winSceneName);
        }
    }
}