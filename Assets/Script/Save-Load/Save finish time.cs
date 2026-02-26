using UnityEngine;

/// <summary>
/// Trigger zone that stops timer and auto-saves when player enters
/// Attach this to a GameObject with a trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("References")]
    [SerializeField] private PlayTimeScore playTimeScore;
    
    [Header("Options")]
    [SerializeField] private bool triggerOnce = true;
    
    private bool hasTriggered = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        
        // Auto-find PlayTimeScore if not assigned
        if (playTimeScore == null)
        {
            playTimeScore = FindFirstObjectByType<PlayTimeScore>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (triggerOnce && hasTriggered)
                return;
            
            hasTriggered = true;
            
            // Stop timer
            if (playTimeScore != null)
            {
                playTimeScore.StopTimer();
                Debug.Log("⏱️ Timer stopped!");
                
                // Save time
                float finalTime = playTimeScore.GetPlayTime();
                PlayerPrefs.SetFloat("PlayTime", finalTime);
                PlayerPrefs.Save();
                Debug.Log($"💾 Time saved: {finalTime}s");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayTimeScore not found!");
            }
        }
    }
}