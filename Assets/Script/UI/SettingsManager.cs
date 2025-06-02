using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public int QualityLevel { get; private set; }
    public bool IsFullscreen { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Tüm sahnelerde ayný obje
            LoadSettings();
        }
        else
        {
            Destroy(gameObject); // Sahneye yanlýþlýkla iki kere eklendiyse, ikincisini sil
        }
    }

    public void LoadSettings()
    {
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        QualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        IsFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        ApplySettings();
    }

    public void ApplySettings()
    {
        AudioListener.volume = MusicVolume;
        Screen.fullScreen = IsFullscreen;
        QualitySettings.SetQualityLevel(QualityLevel);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplySettings();
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        IsFullscreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        ApplySettings();
        PlayerPrefs.Save();
    }

    public void SetQuality(int level)
    {
        QualityLevel = level;
        PlayerPrefs.SetInt("QualityLevel", level);
        ApplySettings();
        PlayerPrefs.Save();
    }
}
