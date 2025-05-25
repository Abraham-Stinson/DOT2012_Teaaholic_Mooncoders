using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Objelerin durumunu kaydeden ve yükleyen bileşen.
/// Bir GameObject'e eklenerek pozisyon, rotasyon, ölçek ve 
/// aktif durumların otomatik olarak kaydedilmesini sağlar.
/// </summary>
public class Saveable : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("Objenin benzersiz kayıt anahtarı. Boş bırakırsanız otomatik oluşturulur.")]
    public string saveKey;
    
    [Tooltip("Objenin pozisyonu kaydedilsin mi?")]
    [SerializeField] public bool savePosition = true;
    
    [Tooltip("Objenin rotasyonu kaydedilsin mi?")]
    [SerializeField] public bool saveRotation = false;
    
    [Tooltip("Objenin ölçeği kaydedilsin mi?")]
    [SerializeField] public bool saveScale = false;
    
    [Tooltip("Objenin aktif/pasif durumu kaydedilsin mi?")]
    [SerializeField] public bool saveActiveState = true;

    [Header("Component Settings")]
    [Tooltip("Işık bileşeni varsa durumu kaydedilsin mi?")]
    [SerializeField] private bool saveLightSettings = true;
    
    [Tooltip("Renderer bileşeni varsa durumu kaydedilsin mi?")]
    [SerializeField] private bool saveRendererSettings = true;

    private Light attachedLight;
    private Renderer attachedRenderer;

    private void Awake()
    {
        attachedLight = GetComponent<Light>();
        attachedRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(saveKey))
            saveKey = gameObject.name + "_" + GetInstanceID();

        SaveManager.Register(this);
    }

    private void OnDisable()
    {
        SaveManager.Unregister(this);
    }

    /// <summary>
    /// Objenin durumunu kaydeder. Bu metod SaveManager tarafından çağrılır.
    /// </summary>
    public virtual void Save()
    {
        // Transform kaydet
        if (savePosition)
        {
            Vector3 pos = transform.position;
            PlayerPrefs.SetFloat(saveKey + "_posX", pos.x);
            PlayerPrefs.SetFloat(saveKey + "_posY", pos.y);
            PlayerPrefs.SetFloat(saveKey + "_posZ", pos.z);
        }
        
        if (saveRotation)
        {
            Vector3 rot = transform.eulerAngles;
            PlayerPrefs.SetFloat(saveKey + "_rotX", rot.x);
            PlayerPrefs.SetFloat(saveKey + "_rotY", rot.y);
            PlayerPrefs.SetFloat(saveKey + "_rotZ", rot.z);
        }
        
        if (saveScale)
        {
            Vector3 scale = transform.localScale;
            PlayerPrefs.SetFloat(saveKey + "_scaleX", scale.x);
            PlayerPrefs.SetFloat(saveKey + "_scaleY", scale.y);
            PlayerPrefs.SetFloat(saveKey + "_scaleZ", scale.z);
        }

        // GameObject durumu
        if (saveActiveState)
        {
            PlayerPrefs.SetInt(saveKey + "_active", gameObject.activeInHierarchy ? 1 : 0);
        }

        // Component durumları
        if (saveLightSettings && attachedLight != null)
        {
            PlayerPrefs.SetInt(saveKey + "_lightEnabled", attachedLight.enabled ? 1 : 0);
            PlayerPrefs.SetFloat(saveKey + "_lightIntensity", attachedLight.intensity);
        }
        
        if (saveRendererSettings && attachedRenderer != null)
        {
            PlayerPrefs.SetInt(saveKey + "_rendererEnabled", attachedRenderer.enabled ? 1 : 0);
        }
    }

    /// <summary>
    /// Objenin durumunu yükler. Bu metod SaveManager tarafından çağrılır.
    /// </summary>
    public virtual void Load()
    {
        // Transform yükle
        if (savePosition && PlayerPrefs.HasKey(saveKey + "_posX"))
        {
            float x = PlayerPrefs.GetFloat(saveKey + "_posX");
            float y = PlayerPrefs.GetFloat(saveKey + "_posY");
            float z = PlayerPrefs.GetFloat(saveKey + "_posZ");
            transform.position = new Vector3(x, y, z);
        }
        
        if (saveRotation && PlayerPrefs.HasKey(saveKey + "_rotX"))
        {
            float x = PlayerPrefs.GetFloat(saveKey + "_rotX");
            float y = PlayerPrefs.GetFloat(saveKey + "_rotY");
            float z = PlayerPrefs.GetFloat(saveKey + "_rotZ");
            transform.eulerAngles = new Vector3(x, y, z);
        }
        
        if (saveScale && PlayerPrefs.HasKey(saveKey + "_scaleX"))
        {
            float x = PlayerPrefs.GetFloat(saveKey + "_scaleX");
            float y = PlayerPrefs.GetFloat(saveKey + "_scaleY");
            float z = PlayerPrefs.GetFloat(saveKey + "_scaleZ");
            transform.localScale = new Vector3(x, y, z);
        }

        // GameObject durumu
        if (saveActiveState && PlayerPrefs.HasKey(saveKey + "_active"))
        {
            int activeState = PlayerPrefs.GetInt(saveKey + "_active");
            gameObject.SetActive(activeState == 1);
        }

        // Component durumları
        if (saveLightSettings && attachedLight != null && PlayerPrefs.HasKey(saveKey + "_lightEnabled"))
        {
            int enabledInt = PlayerPrefs.GetInt(saveKey + "_lightEnabled");
            attachedLight.enabled = enabledInt == 1;
            
            if (PlayerPrefs.HasKey(saveKey + "_lightIntensity"))
            {
                attachedLight.intensity = PlayerPrefs.GetFloat(saveKey + "_lightIntensity");
            }
        }
        
        if (saveRendererSettings && attachedRenderer != null && PlayerPrefs.HasKey(saveKey + "_rendererEnabled"))
        {
            int enabledInt = PlayerPrefs.GetInt(saveKey + "_rendererEnabled");
            attachedRenderer.enabled = enabledInt == 1;
        }
    }
}