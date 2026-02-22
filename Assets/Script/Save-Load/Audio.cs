using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages audio settings with save/load functionality
/// Attach this to a Canvas or UI manager GameObject
/// </summary>
public class AudioSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumePercentageText;
    [SerializeField] private Button saveButton;

    [Header("Settings")]
    [SerializeField] private float defaultVolume = 0.8f;

    private const string VOLUME_KEY = "AudioVolume";

    private void Start()
    {
        // Setup slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Setup save button
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveVolume);
        }

        // Load and apply saved volume
        LoadAndApplyVolume();
    }

    private void OnVolumeChanged(float value)
    {
        // Apply volume immediately
        AudioListener.volume = value;

        // Update percentage display
        UpdateVolumeDisplay(value);
    }

    private void UpdateVolumeDisplay(float value)
    {
        if (volumePercentageText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100f);
            volumePercentageText.text = $"{percentage}%";
        }
    }

    public void SaveVolume()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat(VOLUME_KEY, volumeSlider.value);
            PlayerPrefs.Save();
            Debug.Log($"Volume saved: {volumeSlider.value}");
        }
    }

    public void LoadAndApplyVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        SetVolume(savedVolume);
    }

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }

        AudioListener.volume = volume;
        UpdateVolumeDisplay(volume);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveVolume);
        }
    }
}