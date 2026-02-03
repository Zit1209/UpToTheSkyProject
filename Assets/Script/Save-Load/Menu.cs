using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggles canvases on and off
/// Attach this to a Canvas or UI manager GameObject
/// </summary>
public class CanvasToggle : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Optional Button")]
    [SerializeField] private Button toggleButton;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ShowTargetCanvas);
        }
    }

    /// <summary>
    /// Shows the target canvas
    /// Call this from a UI button
    /// </summary>
    public void ShowTargetCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the current canvas
    /// Call this from a UI button on the canvas itself
    /// </summary>
    public void HideCurrentCanvas()
    {
        Canvas currentCanvas = GetComponent<Canvas>();
        if (currentCanvas != null)
        {
            currentCanvas.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Hides a specific canvas
    /// </summary>
    public void HideCanvas(Canvas canvas)
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the target canvas on/off
    /// </summary>
    public void ToggleTargetCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.gameObject.SetActive(!targetCanvas.gameObject.activeSelf);
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ShowTargetCanvas);
        }
    }
}