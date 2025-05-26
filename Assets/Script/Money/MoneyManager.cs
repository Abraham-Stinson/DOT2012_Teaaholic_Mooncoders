using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour, ICanSave
{
    [SerializeField] private float dayTotalMoney = 0;
    [SerializeField] private float dayTotalSpendMoney = 0;
    [SerializeField][Min(1f)] private float money = 0;
    [SerializeField] private float totalEarnedMoney = 0;
    [SerializeField] private float totalSpentMoney = 0;
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI moneyText;
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
        moneyText.text = money.ToString("F2");
    }

    public void AddMoney(float amount)
    {
        dayTotalMoney += amount;
        money += amount;
        RefreshUI();
        SoundManager.Instance.Money();
    }

    public void SpendMoney(float amount)
    {
        if (amount <= money)
        {
            dayTotalSpendMoney += amount;
            money -= amount;
            RefreshUI();
            SoundManager.Instance.Money();
        }
        else
        {
            Debug.LogWarning("Not enough money to spend!");
        }
    }

    public float GetMoney()
    {
        return money;
    }
    public float GetDayTotalMoney()
    {
        return dayTotalMoney;
    } 
    public float GetTotalSpentMoney()
    {
        return dayTotalSpendMoney;
    } 

    public void ResetDayMoney()
    {
        dayTotalMoney = 0;
        dayTotalSpendMoney = 0;
    }

    public void SaveData()
    {
        PlayerPrefs.SetFloat("CurrentMoney", money);
        PlayerPrefs.SetFloat("DayTotalMoney", dayTotalMoney);
        PlayerPrefs.SetFloat("TotalSpentMoney", totalSpentMoney);
        PlayerPrefs.SetFloat("TotalEarnedMoney", totalEarnedMoney);
    }
    
    public void LoadData()
    {
        if (PlayerPrefs.HasKey("CurrentMoney"))
        {
            money = PlayerPrefs.GetFloat("CurrentMoney");
            dayTotalMoney = PlayerPrefs.GetFloat("DayTotalMoney");
            totalSpentMoney = PlayerPrefs.GetFloat("TotalSpentMoney");
            totalEarnedMoney = PlayerPrefs.GetFloat("TotalEarnedMoney");
            RefreshUI();
        }
    }

}
