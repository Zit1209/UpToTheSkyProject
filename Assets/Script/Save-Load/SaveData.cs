using UnityEngine;

/// <summary>
/// Trigger zone that saves player data when entered
/// UPDATED: Thêm lưu skin đã chọn
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

        // ===== SAVE SKIN =====

        if (SkinManager.HasSelectedSkin())
        {
            string currentSkin = SkinManager.LoadSelectedSkin();
            PlayerPrefs.SetString("SavedSkin", currentSkin);
            Debug.Log($"💾 Đã lưu skin: {currentSkin}");
        }
        // ==========================

        // Save dialogue progress
        SaveDialogueProgress();

        // Save có data hay không
        PlayerPrefs.SetInt("HasSaveData", 1);
        
        PlayerPrefs.Save();
        Debug.Log("✅ Game data saved!");
    }

    private void SaveDialogueProgress()
    {
        // Find all dialogue trigger zones in the scene
        DialogueTriggerZone[] dialogueZones = FindObjectsOfType<DialogueTriggerZone>();

        // Save the count
        PlayerPrefs.SetInt("DialogueCount", dialogueZones.Length);

        for (int i = 0; i < dialogueZones.Length; i++)
        {
            PlayerPrefs.SetInt($"Dialogue_{i}_Read", 0);
        }
    }
}