using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WearManager : MonoBehaviour
{

    [SerializeField, Min(1f)][Range(1f, 100f)] private float wear = 0;
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI wearText;
    [SerializeField] private float refreshRate = 0.1f;
    private float nextRefreshTime;

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextRefreshTime)
        {
            RefreshUI();
            nextRefreshTime = Time.time + refreshRate;
        }
    }

    void RefreshUI()
    {
        wearText.text = "%" + wear.ToString();
    }
    
    public void AddWear(float amount)
    {
        wear += amount;
        if(wear > 100f)
        {
            wear = 100f;
        }
        else if(wear < 0f)
        {
            wear = 0f;
        }
        RefreshUI();
    }
}
