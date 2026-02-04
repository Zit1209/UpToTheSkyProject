using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Load lại vị trí checkpoint gần nhất
/// Đơn giản - chỉ cần gắn vào button
/// </summary>
public class SimpleCheckpointLoader : MonoBehaviour
{
    /// <summary>
    /// Load checkpoint - GỌI TỪ BUTTON
    /// </summary>
    public void LoadLastCheckpoint()
    {
        // Kiểm tra có checkpoint không
        if (!HasCheckpoint())
        {
            Debug.LogWarning("⚠️ Không có checkpoint nào được lưu!");
            return;
        }
        
        Debug.Log("📂 Loading last checkpoint...");
        
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("❌ Không tìm thấy Player!");
            return;
        }
        
        // Load vị trí từ PlayerPrefs
        float x = PlayerPrefs.GetFloat("PlayerPosX", 0f);
        float y = PlayerPrefs.GetFloat("PlayerPosY", 1f);
        float z = PlayerPrefs.GetFloat("PlayerPosZ", 0f);
        
        Vector3 checkpointPos = new Vector3(x, y, z);
        
        Debug.Log($"📍 Teleporting to: {checkpointPos}");
        
        // Teleport player
        TeleportPlayer(player.transform, checkpointPos);
        
        Debug.Log("✅ Đã load checkpoint!");
    }
    
    /// <summary>
    /// Teleport player đến vị trí
    /// </summary>
    void TeleportPlayer(Transform playerTransform, Vector3 position)
    {
        // Nếu có CharacterController
        CharacterController controller = playerTransform.GetComponent<CharacterController>();
        
        if (controller != null)
        {
            // Disable → Teleport → Enable
            controller.enabled = false;
            playerTransform.position = position;
            controller.enabled = true;
        }
        else
        {
            // Không có CharacterController
            playerTransform.position = position;
        }
        
        // Reset velocity nếu có Rigidbody
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reset PlayerMovement velocity
        PlayerMovement movement = playerTransform.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            // Force reset state (nếu có method public)
            movement.enabled = false;
            movement.enabled = true;
        }
    }
    
    /// <summary>
    /// Kiểm tra có checkpoint không
    /// </summary>
    bool HasCheckpoint()
    {
        return PlayerPrefs.HasKey("PlayerPosX");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}