using UnityEngine;

public class Saveable : MonoBehaviour
{
    public string saveKey;

    private Light attachedLight;

    private void Awake()
    {
        attachedLight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(saveKey))
            saveKey = gameObject.name;

        SaveManager.Register(this);
    }

    private void OnDisable()
    {
        SaveManager.Unregister(this);
    }

    public void Save()
    {
        // Pozisyon kaydet (eğer gerekliyse)
        Vector3 pos = transform.position;
        PlayerPrefs.SetFloat(saveKey + "_posX", pos.x);
        PlayerPrefs.SetFloat(saveKey + "_posY", pos.y);
        PlayerPrefs.SetFloat(saveKey + "_posZ", pos.z);

        // Işık durumu kaydet (varsa)
        if (attachedLight != null)
        {
            PlayerPrefs.SetInt(saveKey + "_lightEnabled", attachedLight.enabled ? 1 : 0);
        }
    }

    public void Load()
    {
        // Pozisyon yükle (eğer gerekliyse)
        if (PlayerPrefs.HasKey(saveKey + "_posX"))
        {
            float x = PlayerPrefs.GetFloat(saveKey + "_posX");
            float y = PlayerPrefs.GetFloat(saveKey + "_posY");
            float z = PlayerPrefs.GetFloat(saveKey + "_posZ");

            transform.position = new Vector3(x, y, z);
        }

        // Işık durumu yükle
        if (attachedLight != null && PlayerPrefs.HasKey(saveKey + "_lightEnabled"))
        {
            int enabledInt = PlayerPrefs.GetInt(saveKey + "_lightEnabled");
            attachedLight.enabled = enabledInt == 1;
        }
    }
}
