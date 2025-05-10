using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCycleController : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Real seconds per one in-game minute")]
    public float minuteDurationSeconds = 0.6667f;

    [Tooltip("Game start hour")]
    [Range(0, 23)]
    public int startHour = 9;

    [Tooltip("Game end hour")]
    [Range(1, 24)]
    public int endHour = 24;

    [Header("References")]
    public TextMeshProUGUI timeUIText;
    public Light sunLight;

    private int day = 1;
    private int hour;
    private int minute;
    private float timer = 0f;
    private bool isPaused = false;

    private const float sunrise = 6f;
    private const float sunPeak = 13f;
    private const float sunset = 18f;

    private void Start()
    {
        day = 1;
        hour = startHour;
        minute = 0;
        UpdateTimeUI();
        UpdateSunLight();
    }

    private void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime;
        
        if (timer >= minuteDurationSeconds)
        {
            timer -= minuteDurationSeconds;
            IncrementTimeByOneMinute();
        }
    }

    private void IncrementTimeByOneMinute()
    {
        minute++;
        if (minute >= 60)
        {
            minute = 0;
            hour++;
            if (hour >= 24)  // 24:00 olduğunda (gece yarısı)
            {
                hour = 0;    // Saati 00:00 olarak ayarla
            }
        }

        // Gece yarısında (00:00) zamanı durdur
        if (hour == 0 && minute == 0)
        {
            isPaused = true;

            Debug.Log("Gün sonu - Zaman durduruldu");
        }

        UpdateTimeUI();
        UpdateSunLight();
    }

    private void UpdateTimeUI()
    {
        if (timeUIText == null) return;

        int displayHour = hour % 12;
        displayHour = (displayHour == 0) ? 12 : displayHour;
        string ampm = (hour < 12) ? "AM" : "PM";
        string minuteStr = minute.ToString("00");

        timeUIText.text = $"Day: {day} | {displayHour}:{minuteStr} {ampm}";
    }

    private void UpdateSunLight()
    {
        if (sunLight == null)
        {
            Debug.LogWarning("SunLight atanmamış!");
            return;
        }

        float timeOfDay = hour + (minute / 60f);  // Saat ve dakikayı ondalık saate dönüştür
        float sunRotation;

        if (timeOfDay >= sunrise && timeOfDay <= sunset)
        {
            // Güneşin konumunu hassas olarak hesapla (6:00 - 18:00 arası)
            float dayProgress = (timeOfDay - sunrise) / (sunset - sunrise);
            sunRotation = Mathf.Lerp(0, 180, dayProgress);
        }
        else
        {
            // Gece vakti (dakika hassasiyetinde geçiş için)
            if (timeOfDay < sunrise)
            {
                // Gece yarısından gün doğumuna kadar (0:00-6:00)
                float nightProgress = (timeOfDay + 24 - sunset) / (sunrise + 24 - sunset);
                sunRotation = Mathf.Lerp(180, 360, nightProgress) % 360;
            }
            else
            {
                // Gün batımından gece yarısına kadar (18:00-24:00)
                float nightProgress = (timeOfDay - sunset) / (24 + sunrise - sunset);
                sunRotation = Mathf.Lerp(180, 360, nightProgress) % 360;
            }
        }

        sunLight.transform.rotation = Quaternion.Euler(sunRotation, -30, 0);
    }

    // Kapı ile etkileşim olduğunda çağrılacak
    public void OnMainDoorInteraction()
    {
        if (!isPaused) return;

        day++;
        hour = startHour;
        minute = 0;
        isPaused = false;
        timer = 0f;

        UpdateTimeUI();
        UpdateSunLight();
    }
}
