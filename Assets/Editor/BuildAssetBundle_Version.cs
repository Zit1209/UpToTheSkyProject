#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Script build AssetBundles với version folders
/// FIXED: Wrapped trong #if UNITY_EDITOR để build được
/// </summary>
public class BuildAssetBundles
{
    // Build thông thường vào Bundles/
    [MenuItem("Assets/Build Asset Bundles/Normal Build")]
    public static void BuildAllAssetBundles()
    {
        string bundleDirectory = "Assets/StreamingAssets/Bundles";
        BuildBundles(bundleDirectory);
    }

    // Build vào folder version cụ thể
    [MenuItem("Assets/Build Asset Bundles/Build to Version v1.0")]
    public static void BuildToV1()
    {
        BuildToVersion("v1.0");
    }

    [MenuItem("Assets/Build Asset Bundles/Build to Version v2.0")]
    public static void BuildToV2()
    {
        BuildToVersion("v2.0");
    }

    [MenuItem("Assets/Build Asset Bundles/Build to Version beta")]
    public static void BuildToBeta()
    {
        BuildToVersion("beta");
    }

    // Build với tên version tùy chỉnh (hiện dialog nhập)
    [MenuItem("Assets/Build Asset Bundles/Build to Custom Version...")]
    public static void BuildToCustomVersion()
    {
        // Mở dialog để nhập tên version
        string version = EditorUtility.SaveFolderPanel(
            "Chọn tên version folder",
            "Assets/StreamingAssets/Bundles",
            "v1.0"
        );

        if (!string.IsNullOrEmpty(version))
        {
            // Lấy tên folder cuối cùng từ path
            string versionName = Path.GetFileName(version);
            BuildToVersion(versionName);
        }
    }

    /// <summary>
    /// Build bundles vào folder version cụ thể
    /// </summary>
    public static void BuildToVersion(string versionName)
    {
        string bundleDirectory = Path.Combine("Assets/StreamingAssets/Bundles", versionName);
        BuildBundles(bundleDirectory);
        Debug.Log($"📦 Đã build bundles vào version: {versionName}");
    }

    /// <summary>
    /// Hàm build bundles chính
    /// </summary>
    static void BuildBundles(string outputPath)
    {
        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Debug.Log($"📁 Đã tạo thư mục: {outputPath}");
        }

        try
        {
            Debug.Log($"🔨 Bắt đầu build bundles vào: {outputPath}");

            // Build asset bundles
            BuildPipeline.BuildAssetBundles(
                outputPath,
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget
            );

            Debug.Log($"✅ Build thành công!");
            
            // Refresh Asset Database
            AssetDatabase.Refresh();

            // Mở folder vừa build
            EditorUtility.RevealInFinder(outputPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Build thất bại: {e.Message}");
            Debug.LogException(e);
        }
    }

    // Xóa tất cả bundles trong một version
    [MenuItem("Assets/Build Asset Bundles/Clear Version Folder...")]
    public static void ClearVersionFolder()
    {
        string path = EditorUtility.OpenFolderPanel(
            "Chọn version folder để xóa",
            "Assets/StreamingAssets/Bundles",
            ""
        );

        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            if (EditorUtility.DisplayDialog(
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa tất cả bundles trong:\n{path}",
                "Xóa",
                "Hủy"))
            {
                Directory.Delete(path, true);
                AssetDatabase.Refresh();
                Debug.Log($"🗑️ Đã xóa folder: {path}");
            }
        }
    }

    // Liệt kê tất cả versions có sẵn
    [MenuItem("Assets/Build Asset Bundles/List All Versions")]
    public static void ListAllVersions()
    {
        string bundlesPath = "Assets/StreamingAssets/Bundles";
        
        if (!Directory.Exists(bundlesPath))
        {
            Debug.Log("📂 Chưa có thư mục Bundles");
            return;
        }

        string[] directories = Directory.GetDirectories(bundlesPath);
        
        if (directories.Length == 0)
        {
            Debug.Log("📂 Không có version nào");
            return;
        }

        Debug.Log("📦 Các version có sẵn:");
        foreach (string dir in directories)
        {
            string versionName = Path.GetFileName(dir);
            string[] bundles = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            int bundleCount = 0;
            
            foreach (string file in bundles)
            {
                // Đếm các file không phải .manifest và .meta
                string ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext) || (!ext.Equals(".manifest") && !ext.Equals(".meta")))
                {
                    bundleCount++;
                }
            }
            
            Debug.Log($"  📁 {versionName} - {bundleCount} bundle(s)");
        }
    }
}
#endif