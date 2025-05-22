using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private float dayTotalMoney = 0;
    [SerializeField][Min(1f)] private float money = 0;
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
    }

    public float GetDayTotalMoney()
    {
        return dayTotalMoney;
    } 
    public void ResetDayTotalMoney()
    {
        dayTotalMoney = 0;
    }
}
