using UnityEngine;
using UnityEngine.UI;

public class SkinPreview : MonoBehaviour
{
    [Header("Preview Camera")]
    public Camera previewCamera;
    public RawImage previewImage; // RawImage trên Canvas để hiển thị
    
    [Header("Model Settings")]
    public Transform modelParent; // Vị trí để spawn model preview
    public Vector3 modelPosition = new Vector3(0, 0, 0);
    public Vector3 modelRotation = new Vector3(0, 180, 0);
    public float modelScale = 1f;
    
    [Header("Auto Rotation")]
    public bool autoRotate = true;
    public float rotationSpeed = 30f;
    
    private RenderTexture renderTexture;
    private GameObject currentModel;

    void Start()
    {
        SetupPreviewCamera();
    }

    void SetupPreviewCamera()
    {
        // Tạo RenderTexture với kích thước phù hợp (phải là bội số của 2)
        // Sử dụng kích thước 1024x1024 để tránh lỗi
        renderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1; // Tắt anti-aliasing để tránh lỗi
        renderTexture.Create();
        
        // Setup camera render vào texture
        if (previewCamera != null)
        {
            previewCamera.targetTexture = renderTexture;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Trong suốt
            
            // Tắt các post-processing nếu có
            previewCamera.allowHDR = false;
            previewCamera.allowMSAA = false;
        }
        
        // Gán texture vào RawImage
        if (previewImage != null)
        {
            previewImage.texture = renderTexture;
        }
    }

    // Gọi function này để hiển thị model
    public void ShowModel(GameObject modelPrefab)
    {
        // Xóa model cũ nếu có
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
        
        // Tạo model mới
        if (modelPrefab != null && modelParent != null)
        {
            currentModel = Instantiate(modelPrefab, modelParent);
            currentModel.transform.localPosition = modelPosition;
            currentModel.transform.localEulerAngles = modelRotation;
            currentModel.transform.localScale = Vector3.one * modelScale;
            
            // Set layer để chỉ preview camera nhìn thấy (nếu đã tạo layer "Preview")
            int previewLayer = LayerMask.NameToLayer("Preview");
            if (previewLayer != -1)
            {
                SetLayerRecursively(currentModel, previewLayer);
            }
        }
    }

    void Update()
    {
        // Tự động xoay model
        if (autoRotate && currentModel != null)
        {
            currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}