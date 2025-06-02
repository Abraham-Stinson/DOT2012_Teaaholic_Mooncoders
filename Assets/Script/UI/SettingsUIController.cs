using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;

    void Start()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogWarning("SettingsManager bulunamadý!");
            return;
        }

        // UI'yý güncel ayarlarla baþlat
        musicSlider.value = SettingsManager.Instance.MusicVolume;
        sfxSlider.value = SettingsManager.Instance.SFXVolume;
        fullscreenToggle.isOn = SettingsManager.Instance.IsFullscreen;
        qualityDropdown.value = SettingsManager.Instance.QualityLevel;

        // Eventleri baðla
        musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSFXVolume);
        fullscreenToggle.onValueChanged.AddListener(SettingsManager.Instance.SetFullscreen);
        qualityDropdown.onValueChanged.AddListener(SettingsManager.Instance.SetQuality);
    }

    void OnDestroy()
    {
        // Eventleri temizle (sahne deðiþimlerinde çakýþma olmamasý için)
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        qualityDropdown.onValueChanged.RemoveAllListeners();
    }
}
