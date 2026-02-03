#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Script gán Bundle Name nhanh cho Unity 6
/// Right-click prefab → Assign Bundle Name → Chọn tên
/// FIXED: Wrapped trong #if UNITY_EDITOR để build được
/// </summary>
public class QuickBundleAssign
{
    // Menu: Assets → Assign Bundle Name → player
    [MenuItem("Assets/Assign Bundle Name/player")]
    static void AssignPlayer()
    {
        AssignBundleToSelected("player");
    }
    
    // Menu: Assets → Assign Bundle Name → enemies
    [MenuItem("Assets/Assign Bundle Name/enemies")]
    static void AssignEnemies()
    {
        AssignBundleToSelected("enemies");
    }
    
    // Menu: Assets → Assign Bundle Name → terrain
    [MenuItem("Assets/Assign Bundle Name/terrain")]
    static void AssignTerrain()
    {
        AssignBundleToSelected("terrain");
    }
    
    // Menu: Assets → Assign Bundle Name → weapons
    [MenuItem("Assets/Assign Bundle Name/weapons")]
    static void AssignWeapons()
    {
        AssignBundleToSelected("weapons");
    }
    
    // Menu: Assets → Assign Bundle Name → Custom... (Nhập tên tùy chỉnh)
    [MenuItem("Assets/Assign Bundle Name/Custom...")]
    static void AssignCustom()
    {
        string bundleName = EditorInputDialog.Show(
            "Nhập tên bundle",
            "Tên bundle (ví dụ: player, items...):",
            "player"
        );
        
        if (!string.IsNullOrEmpty(bundleName))
        {
            AssignBundleToSelected(bundleName);
        }
    }
    
    // Menu: Assets → Clear Bundle Name (Xóa bundle name)
    [MenuItem("Assets/Clear Bundle Name")]
    static void ClearBundle()
    {
        if (EditorUtility.DisplayDialog(
            "Xóa Bundle Name",
            "Bạn có chắc muốn xóa bundle name của assets đã chọn?",
            "Xóa", "Hủy"))
        {
            AssignBundleToSelected("");
        }
    }
    
    // Hàm chính để gán bundle
    static void AssignBundleToSelected(string bundleName)
    {
        int successCount = 0;
        int failCount = 0;
        
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            
            // Kiểm tra có phải asset không
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"⚠️ {obj.name} không phải asset file!");
                failCount++;
                continue;
            }
            
            // Lấy AssetImporter
            AssetImporter importer = AssetImporter.GetAtPath(path);
            
            if (importer != null)
            {
                // Gán bundle name
                importer.assetBundleName = bundleName.ToLower();
                
                if (string.IsNullOrEmpty(bundleName))
                {
                    Debug.Log($"🗑️ Đã xóa bundle name: {obj.name}");
                }
                else
                {
                    Debug.Log($"✅ Gán bundle '{bundleName}' cho: {obj.name}");
                }
                
                successCount++;
            }
            else
            {
                Debug.LogError($"❌ Không thể gán bundle cho: {obj.name}");
                failCount++;
            }
        }
        
        // Lưu thay đổi
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Hiện thông báo
        string message = $"Thành công: {successCount}\nThất bại: {failCount}";
        EditorUtility.DisplayDialog("Kết quả", message, "OK");
    }
}

/// <summary>
/// Helper class để hiện input dialog
/// </summary>
public class EditorInputDialog : EditorWindow
{
    private string description = "";
    private string inputText = "";
    private string okButton = "OK";
    private string cancelButton = "Cancel";
    private bool shouldClose = false;
    private System.Action<string> onOK;
    
    public static string Show(string title, string description, string defaultText)
    {
        string result = defaultText;
        bool submitted = false;
        
        var window = ScriptableObject.CreateInstance<EditorInputDialog>();
        window.titleContent = new GUIContent(title);
        window.description = description;
        window.inputText = defaultText;
        window.onOK = (text) => { result = text; submitted = true; };
        
        window.ShowModal();
        
        return submitted ? result : null;
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(description);
        EditorGUILayout.Space(10);
        
        GUI.SetNextControlName("InputField");
        inputText = EditorGUILayout.TextField(inputText);
        
        if (!shouldClose)
        {
            GUI.FocusControl("InputField");
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button(okButton) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            onOK?.Invoke(inputText);
            shouldClose = true;
        }
        
        if (GUILayout.Button(cancelButton) || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape))
        {
            shouldClose = true;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (shouldClose)
        {
            Close();
        }
    }
}
#endif