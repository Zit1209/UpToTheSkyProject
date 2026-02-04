using UnityEngine;
using UnityEngine.UI;

public class ModelPreviewUI : MonoBehaviour
{
    [Header("UI")]
    public RawImage previewImage;

    [Header("Preview Settings")]
    public Camera previewCamera;
    public Transform previewRoot;
    public float rotationSpeed = 30f;

    private GameObject currentModel;

    void Update()
    {
        if (currentModel != null)
        {
            currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Hiện prefab model lên Canvas
    /// </summary>
    public void ShowModel(GameObject prefab)
    {
        ClearModel();

        currentModel = Instantiate(prefab, previewRoot);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        SetLayerRecursively(currentModel, LayerMask.NameToLayer("Preview"));
    }

    void ClearModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
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
}
