using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Load player from bundle and setup components
/// FINAL VERSION - Matches your actual PlayerMovement.cs
/// </summary>
public class PlayerBundleLoader : MonoBehaviour
{
    [Header("Bundle Settings")]
    public string bundleName = "player";
    
    [Header("Spawn Settings")]
    public Vector3 defaultSpawnPosition = new Vector3(0f, 1f, 0f);
    public Transform spawnPoint;
    
    [Header("Required Assets - ATTACH IN INSPECTOR!")]
    [Tooltip("InputActionAsset for player (REQUIRED!)")]
    public InputActionAsset playerInputAsset;
    
    [Tooltip("Main Camera (optional - auto-find if null)")]
    public Camera mainCamera;
    
    [Header("Camera Follow")]
    public bool enableCameraFollow = true;
    public Vector3 cameraOffset = new Vector3(0f, 2f, -5f);
    public float cameraSmooth = 5f;
    
    [Header("References")]
    public MonoBehaviour playTimeScore;
    
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
            Debug.LogWarning("⚠️ Already loading player...");
            yield break;
        }
        
        isLoading = true;
        
        // 1. Load skin version
        string skinVersion = SkinManager.LoadSelectedSkin();
        string assetName = SkinManager.GetAssetNameFromVersion(skinVersion);
        
        Debug.Log("╔═══════════════════════════════════╗");
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
            Debug.LogError($"❌ Bundle not found: {path}");
            Debug.LogError("Please build bundles first!");
            isLoading = false;
            yield break;
        }
        
        // 4. Load bundle
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        
        if (bundle == null)
        {
            Debug.LogError($"❌ Failed to load bundle!");
            isLoading = false;
            yield break;
        }
        
        Debug.Log("✅ Bundle loaded!");
        
        // 5. Load prefab
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
        
        if (prefab == null)
        {
            Debug.LogError($"❌ Asset '{assetName}' not found!");
            bundle.Unload(true);
            isLoading = false;
            yield break;
        }
        
        Debug.Log($"✅ Prefab loaded: {prefab.name}");
        
        // 6. Determine spawn position
        Vector3 spawnPos = GetSpawnPosition();
        Debug.Log($"📍 Spawn position: {spawnPos}");
        
        // 7. Spawn player
        spawnedPlayer = Instantiate(prefab, spawnPos, Quaternion.identity);
        spawnedPlayer.name = "Player";
        
        Debug.Log($"✅ Player spawned!");
        
        // 8. Setup player components
        yield return StartCoroutine(SetupPlayerComponents(spawnedPlayer));
        
        // 9. Load play time if component exists
        LoadPlayTime();
        
        // 10. Unload bundle
        bundle.Unload(false);
        
        Debug.Log("╚═══════════════════════════════════╝");
        
        isLoading = false;
    }
    
    Vector3 GetSpawnPosition()
    {
        if (PlayerPrefs.HasKey("PlayerPosX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            
            Debug.Log($"📂 Loaded position from save: ({x}, {y}, {z})");
            return new Vector3(x, y, z);
        }
        
        if (spawnPoint != null)
        {
            Debug.Log("📍 Using spawn point");
            return spawnPoint.position;
        }
        
        Debug.Log("📍 Using default spawn position");
        return defaultSpawnPosition;
    }
    
    void LoadPlayTime()
    {
        if (playTimeScore == null)
            return;
        
        var type = playTimeScore.GetType();
        
        if (PlayerPrefs.HasKey("PlayTime"))
        {
            float savedTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            
            var setTimeMethod = type.GetMethod("SetTime");
            if (setTimeMethod != null)
            {
                setTimeMethod.Invoke(playTimeScore, new object[] { savedTime });
            }
            
            var startMethod = type.GetMethod("StartTimer");
            if (startMethod != null)
            {
                startMethod.Invoke(playTimeScore, null);
            }
            
            Debug.Log($"⏱️ Loaded play time: {savedTime}s");
        }
        else
        {
            var resetMethod = type.GetMethod("ResetTimer");
            if (resetMethod != null)
            {
                resetMethod.Invoke(playTimeScore, null);
            }
            
            var startMethod = type.GetMethod("StartTimer");
            if (startMethod != null)
            {
                startMethod.Invoke(playTimeScore, null);
            }
            
            Debug.Log("⏱️ Started new timer");
        }
    }
    
    IEnumerator SetupPlayerComponents(GameObject player)
    {
        Debug.Log("🔧 Setting up player...");
        
        // Get PlayerMovement component directly
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        
        if (movement == null)
        {
            Debug.LogWarning("⚠️ PlayerMovement component not found on player!");
        }
        else
        {
            // Setup InputActionAsset
            if (playerInputAsset != null)
            {
                Debug.Log("🎮 Assigning InputActionAsset...");
                
                // Disable component
                movement.enabled = false;
                
                // Use reflection to set the inputAsset field
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
                    Debug.LogWarning("⚠️ Field 'inputAsset' not found in PlayerMovement");
                }
                
                yield return null;
                
                // Re-enable component
                movement.enabled = true;
                
                Debug.Log("✅ PlayerMovement enabled!");
            }
            else
            {
                Debug.LogError("❌ PlayerInputAsset is NULL! Please assign in Inspector!");
            }
        }
        
        // Setup Tag
        player.tag = "Player";
        
        // Setup Camera Follow
        if (enableCameraFollow && mainCamera != null)
        {
            SetupCameraFollow(player.transform);
        }
        
        Debug.Log("✅ Player setup completed!");
    }
    
    void SetupCameraFollow(Transform target)
    {
        // Check if CameraFollow already exists
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
    
    public GameObject GetPlayer()
    {
        return spawnedPlayer;
    }
    
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