using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WearManager : MonoBehaviour, ICanSave
{
    [SerializeField, Min(1f)][Range(1f, 100f)] public float wear = 0;
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI wearText;
    [SerializeField] private float refreshRate = 0.1f;
    private float nextRefreshTime;

    [Header("Vignette Settings")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private float vignetteDuration = 1f;
    private Vignette vignette;
    private float vignetteTimer;
    private float previousWear;

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
        }
        previousWear = wear;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextRefreshTime)
        {
            RefreshUI();
            nextRefreshTime = Time.time + refreshRate;
        }

        // Handle vignette effect
        if (vignette != null)
        {
            if (vignetteTimer > 0)
            {
                vignetteTimer -= Time.deltaTime;
                float normalizedTime = vignetteTimer / vignetteDuration;
                vignette.intensity.value = Mathf.Lerp(0, maxVignetteIntensity, normalizedTime);
            }
            else
            {
                vignette.intensity.value = 0;
            }
        }
    }

    void RefreshUI()
    {
        wearText.text = "%" + wear.ToString();
    }

    public void AddWear(float amount)
    {
        float oldWear = wear;
        wear += amount;
        if (wear > 100f)
        {
            wear = 100f;
        }
        else if (wear < 0f)
        {
            wear = 0f;
        }

        // Trigger vignette effect if wear increased
        if (wear > oldWear && vignette != null)
        {
            vignetteTimer = vignetteDuration;
        }

        RefreshUI();
    }

    public void SaveData()
    {
        PlayerPrefs.SetFloat("CurrentWear", wear);
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("CurrentWear"))
        {
            wear = PlayerPrefs.GetFloat("CurrentWear");
            RefreshUI();
        }
    }
    
    public float GetWear()
    {
        return wear;
    }
}
