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
    [Range(0, 24)]
    public int endHour = 24;

    [Header("References")]
    public TextMeshProUGUI dayUIText;
    public TextMeshProUGUI timeUIText;
    public Light sunLight;
    public NPCManager npcManager;

    private int day = 1;
    private int hour;
    private int minute;
    private float timer = 0f;
    private bool isPaused = false;
    private bool npcSpawningDisabled = false;

    private const float sunrise = 6f;
    private const float sunset = 18f;

    private void Start()
    {
         
        if (PlayerPrefs.HasKey("SavedDay"))
        {
            day = PlayerPrefs.GetInt("SavedDay");
            hour = PlayerPrefs.GetInt("SavedHour");
            minute = PlayerPrefs.GetInt("SavedMinute");

            // Eğer gece yarısı ise yeni güne geç
            if (hour == 0 && minute == 0)
            {
                day++;
                hour = startHour;
                minute = 0;
            }
        }
        else
        {
            day = 1;
            hour = startHour;
            minute = 0;
        }

        isPaused = false;
        npcSpawningDisabled = hour >= 12;

        UpdateTimeUI();
        UpdateSunLight();

        if (npcManager != null)
        {
            npcManager.enabled = !npcSpawningDisabled;

        }
        SaveManager.LoadAll();
    }

    private void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime;

        if (timer >= minuteDurationSeconds)
        {
            timer = 0f; // Timer'ı sıfırla
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

            if (hour >= 24)
            {
                hour = 0;
                day++;
                isPaused = true; // Yeni gün için duraklat
                Debug.Log("Yeni gün başladı! Gün: " + day);
            }
        }

        // NPC kontrolü
        if (hour == 12 && !npcSpawningDisabled && npcManager != null)
        {
            npcSpawningDisabled = true;
            npcManager.enabled = false;
            Debug.Log("Saat 12:00 - NPC spawn durduruldu");
        }

        // 23:59'da kaydet
        if (hour == 23 && minute == 59)
        {
            SaveGame();
            Debug.Log("Oyun kaydedildi");
        }

        UpdateTimeUI();
        UpdateSunLight();
    }

    public void OnMainDoorInteraction()
    {
        if (!isPaused) return;

        // Yeni gün başlat
        hour = startHour;
        minute = 0;
        isPaused = false;
        npcSpawningDisabled = false;

        if (npcManager != null)
        {
            npcManager.enabled = true;
        }

        Debug.Log("Yeni gün başladı! Saat: " + hour + ":00");
    }

    private void UpdateTimeUI()
    {
        if (dayUIText != null)
            dayUIText.text = "Gün: " + day;

        if (timeUIText != null)
        {
            string ampm = hour < 12 ? "ÖÖ" : "ÖS";
            int displayHour = hour % 12;
            displayHour = displayHour == 0 ? 12 : displayHour;
            timeUIText.text = $"{displayHour}:{minute:00} {ampm}";
        }
    }

    private void UpdateSunLight()
    {
        if (sunLight == null) return;

        float timeOfDay = hour + minute / 60f;
        float sunRotation;

        if (timeOfDay >= sunrise && timeOfDay <= sunset)
        {
            float dayProgress = (timeOfDay - sunrise) / (sunset - sunrise);
            sunRotation = Mathf.Lerp(0, 180, dayProgress);
        }
        else
        {
            if (timeOfDay < sunrise)
            {
                float nightProgress = (timeOfDay + 24 - sunset) / (sunrise + 24 - sunset);
                sunRotation = Mathf.Lerp(180, 360, nightProgress);
            }
            else
            {
                float nightProgress = (timeOfDay - sunset) / (24 + sunrise - sunset);
                sunRotation = Mathf.Lerp(180, 360, nightProgress);
            }
        }

        sunLight.transform.rotation = Quaternion.Euler(sunRotation, -30, 0);
    }

    private void SaveGame()
    {
        PlayerPrefs.SetInt("SavedDay", day);
        PlayerPrefs.SetInt("SavedHour", hour);
        PlayerPrefs.SetInt("SavedMinute", minute);
        SaveManager.SaveAll();
        Debug.Log("Oyun kaydedildi - Gün: " + day + " Saat: " + hour + ":" + minute);
    }
    public void LoadTime()
    {
        if (PlayerPrefs.HasKey("SavedDay"))
        {
            day = PlayerPrefs.GetInt("SavedDay");
            hour = PlayerPrefs.GetInt("SavedHour");
            minute = PlayerPrefs.GetInt("SavedMinute");
        }
    }
}