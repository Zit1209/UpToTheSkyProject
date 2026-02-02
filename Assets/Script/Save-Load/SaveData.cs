using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Trigger zone that saves player data when entered
/// Attach this to a GameObject with a trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class SaveDataTriggerZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool saveOnlyOnce = false;

    [Header("References")]
    [SerializeField] private PlayTimeScore playTimeScore;

    private bool hasSaved = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (saveOnlyOnce && hasSaved)
                return;

            SaveGameData(other.transform);
            hasSaved = true;
        }
    }

    private void SaveGameData(Transform player)
    {
        // Save player position
        PlayerPrefs.SetFloat("PlayerPosX", player.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", player.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", player.position.z);

        // Save play time
        if (playTimeScore != null)
        {
            PlayerPrefs.SetFloat("PlayTime", playTimeScore.GetPlayTime());
        }

        // Save dialogue progress
        SaveDialogueProgress();

        PlayerPrefs.Save();
        Debug.Log("Game data saved!");
    }

    private void SaveDialogueProgress()
    {
        // Find all dialogue trigger zones in the scene
        DialogueTriggerZone[] dialogueZones = FindObjectsOfType<DialogueTriggerZone>();

        // Save the count
        PlayerPrefs.SetInt("DialogueCount", dialogueZones.Length);

        // For each dialogue zone, we need to track if it has been triggered
        // This is a simplified approach - in a real game, you'd use a more robust system
        for (int i = 0; i < dialogueZones.Length; i++)
        {
            // You would need to add a public property to DialogueTriggerZone to check if triggered
            // For now, we'll save placeholder data
            PlayerPrefs.SetInt($"Dialogue_{i}_Read", 0);
        }
    }
}