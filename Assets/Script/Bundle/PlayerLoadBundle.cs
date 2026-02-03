using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Load player từ bundle và setup đầy đủ components
/// UPDATED: Load position từ save data nếu có
/// </summary>
public class PlayerBundleLoader : MonoBehaviour
{
    [Header("Bundle Settings")]
    public string bundleName = "player";
    
    [Header("Spawn Settings")]
    public Vector3 defaultSpawnPosition = new Vector3(0f, 1f, 0f);
    public Transform spawnPoint;
    
    [Header("Required Assets - GÁN TRONG INSPECTOR!")]
    [Tooltip("InputActionAsset cho player (REQUIRED!)")]
    public InputActionAsset playerInputAsset;
    
    [Tooltip("Main Camera (optional - tự động tìm nếu null)")]
    public Camera mainCamera;
    
    [Header("Camera Follow")]
    public bool enableCameraFollow = true;
    public Vector3 cameraOffset = new Vector3(0f, 2f, -5f);
    public float cameraSmooth = 5f;
    
    [Header("References")]
    public PlayTimeScore playTimeScore;
    
    private GameObject spawnedPlayer;
    private bool isLoading = false;
    
    void Start()
    {
        // Auto-find camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Load player
        StartCoroutine(LoadPlayerWithSkin());
    }
    
    IEnumerator LoadPlayerWithSkin()
    {
        if (isLoading)
        {
            Debug.LogWarning("⚠️ Đang load player...");
            yield break;
        }
        
        isLoading = true;
        
        // 1. Load skin version
        string skinVersion = SkinManager.LoadSelectedSkin();
        string assetName = SkinManager.GetAssetNameFromVersion(skinVersion);
        
        Debug.Log("═══════════════════════════════════");
        Debug.Log($"🎨 Loading player: {skinVersion}");
        Debug.Log($"📦 Bundle: {bundleName}");
        Debug.Log($"🎯 Asset: {assetName}");
        
        // 2. Build path
        string path = Path.Combine(
            Application.streamingAssetsPath,
            "Bundles",
            skinVersion,
            bundleName
        );
        
        Debug.Log($"📁 Path: {path}");
        
        // 3. Check file exists
        if (!File.Exists(path))
        {
            Debug.LogError($"❌ Bundle không tồn tại: {path}");
            Debug.LogError("Hãy build bundles trước!");
            isLoading = false;
            yield break;
        }
        
        // 4. Load bundle
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        
        if (bundle == null)
        {
            Debug.LogError($"❌ Load bundle thất bại!");
            isLoading = false;
            yield break;
        }
        
        Debug.Log("✅ Bundle loaded!");
        
        // 5. Load prefab
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
        
        if (prefab == null)
        {
            Debug.LogError($"❌ Asset '{assetName}' không tìm thấy!");
            bundle.Unload(true);
            isLoading = false;
            yield break;
        }
        
        Debug.Log($"✅ Prefab loaded: {prefab.name}");
        
        // ===== 6. XÁC ĐỊNH VỊ TRÍ SPAWN =====
        Vector3 spawnPos = GetSpawnPosition();
        Debug.Log($"📍 Spawn position: {spawnPos}");
        // ====================================
        
        // 7. Spawn player
        spawnedPlayer = Instantiate(prefab, spawnPos, Quaternion.identity);
        spawnedPlayer.name = "Player";
        
        Debug.Log($"✅ Player spawned!");
        
        // 8. SETUP PLAYER COMPONENTS
        yield return StartCoroutine(SetupPlayerComponents(spawnedPlayer));
        
        // 9. Load play time nếu có
        LoadPlayTime();
        
        // 10. Unload bundle
        bundle.Unload(false);
        
        Debug.Log("═══════════════════════════════════");
        
        isLoading = false;
    }
    
    /// <summary>
    /// Lấy vị trí spawn (từ save hoặc default)
    /// </summary>
    Vector3 GetSpawnPosition()
    {
        // Kiểm tra có save data không
        if (PlayerPrefs.HasKey("PlayerPosX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            
            Debug.Log($"📂 Loaded position from save: ({x}, {y}, {z})");
            return new Vector3(x, y, z);
        }
        
        // Nếu không có save, dùng spawn point hoặc default
        if (spawnPoint != null)
        {
            Debug.Log("📍 Using spawn point");
            return spawnPoint.position;
        }
        
        Debug.Log("📍 Using default spawn position");
        return defaultSpawnPosition;
    }
    
    /// <summary>
    /// Load play time từ save
    /// </summary>
    void LoadPlayTime()
    {
        if (playTimeScore == null)
            return;
        
        if (PlayerPrefs.HasKey("PlayTime"))
        {
            float savedTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            playTimeScore.SetTime(savedTime);
            playTimeScore.StartTimer();
            Debug.Log($"⏱️ Loaded play time: {savedTime}s");
        }
        else
        {
            playTimeScore.ResetTimer();
            playTimeScore.StartTimer();
            Debug.Log("⏱️ Started new timer");
        }
    }
    
    /// <summary>
    /// Setup tất cả components sau khi spawn
    /// </summary>
    IEnumerator SetupPlayerComponents(GameObject player)
    {
        Debug.Log("🔧 Setting up player...");
        
        // 1. Get PlayerMovement
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        
        if (movement == null)
        {
            Debug.LogError("❌ PlayerMovement không tìm thấy!");
            yield break;
        }
        
        // 2. FIX INPUTACTIONASSET (Dùng Reflection)
        if (playerInputAsset != null)
        {
            Debug.Log("🎮 Assigning InputActionAsset...");
            
            // Disable trước
            movement.enabled = false;
            
            // Gán InputActionAsset bằng reflection
            var inputField = typeof(PlayerMovement).GetField("inputAsset", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (inputField != null)
            {
                inputField.SetValue(movement, playerInputAsset);
                Debug.Log("✅ InputActionAsset assigned!");
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy field 'inputAsset'");
            }
            
            // Wait 1 frame
            yield return null;
            
            // Enable lại
            movement.enabled = true;
            
            Debug.Log("✅ PlayerMovement enabled!");
        }
        else
        {
            Debug.LogError("❌ PlayerInputAsset NULL! Hãy gán trong Inspector!");
        }
        
        // 3. Setup Tag
        player.tag = "Player";
        
        // 4. Setup Camera Follow
        if (enableCameraFollow && mainCamera != null)
        {
            SetupCameraFollow(player.transform);
        }
        
        Debug.Log("✅ Player setup completed!");
    }
    
    /// <summary>
    /// Setup camera follow player
    /// </summary>
    void SetupCameraFollow(Transform target)
    {
        CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
        
        if (camFollow == null)
        {
            camFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }
        
        camFollow.target = target;
        camFollow.offset = cameraOffset;
        camFollow.smoothSpeed = cameraSmooth;
        
        Debug.Log("✅ Camera follow setup!");
    }
    
    /// <summary>
    /// Get player reference
    /// </summary>
    public GameObject GetPlayer()
    {
        return spawnedPlayer;
    }
    
    /// <summary>
    /// Reload player với skin khác
    /// </summary>
    public void ReloadPlayer(string newSkinVersion)
    {
        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }
        
        SkinManager.SaveSelectedSkin(newSkinVersion);
        StartCoroutine(LoadPlayerWithSkin());
    }
}