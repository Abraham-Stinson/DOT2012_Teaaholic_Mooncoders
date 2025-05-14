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
    public TextMeshProUGUI dayUIText;    // Separate UI element for day
    public TextMeshProUGUI timeUIText;   // Separate UI element for time
    public Light sunLight;
    public NPCManager npcManager; // Reference to NPC Manager

    private int day = 1;
    private int hour;
    private int minute;
    private float timer = 0f;
    private bool isPaused = false;
    private bool npcSpawningDisabled = false; // Track if NPC spawning is disabled

    private const float sunrise = 6f;
    private const float sunPeak = 13f;
    private const float sunset = 18f;

    private void Start()
    {
        day = 1;
        hour = startHour;
        minute = 0;
        npcSpawningDisabled = false;
        UpdateTimeUI();
        UpdateSunLight();
        
        // Find NPCManager if not assigned
        if (npcManager == null)
        {
            npcManager = FindObjectOfType<NPCManager>();
            if (npcManager == null)
            {
                Debug.LogWarning("NPCManager bulunamadı!");
            }
        }
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

        // Saat 12'den sonra müşteri spawning'i durdur
        if (hour >= 12 && !npcSpawningDisabled && npcManager != null)
        {
            npcSpawningDisabled = true;
            npcManager.enabled = false; // NPCManager'ı devre dışı bırak
            Debug.Log("Saat 12:00 oldu - Müşteri spawning durduruldu");
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
        // Day UI güncelleme
        if (dayUIText != null)
        {
            dayUIText.text = $"Gün {day}";
        }

        // Time UI güncelleme
        if (timeUIText != null)
        {
            int displayHour = hour % 12;
            displayHour = (displayHour == 0) ? 12 : displayHour;
            string ampm = (hour < 12) ? "ÖÖ" : "ÖS";
            string minuteStr = minute.ToString("00");

            timeUIText.text = $"{displayHour}:{minuteStr} {ampm}";
        }
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
        
        // Yeni gün başladığında müşteri spawning'i tekrar aktif et
        if (npcSpawningDisabled && npcManager != null)
        {
            npcSpawningDisabled = false;
            npcManager.enabled = true; // NPCManager'ı tekrar etkinleştir
            Debug.Log("Yeni gün başladı - Müşteri spawning tekrar aktif");
        }

        UpdateTimeUI();
        UpdateSunLight();
    }
}
