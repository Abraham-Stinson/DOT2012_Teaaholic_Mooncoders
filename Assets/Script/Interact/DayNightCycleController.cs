using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DayNightCycleController : MonoBehaviour, ICanSave
{
    [SerializeField]
    private PauseMenuController pauseMenuControllerScript; // Reference to the PauseMenuController script
    [SerializeField] NPCManager npcManagerScript; // Reference to NPC Manager
    [SerializeField] WearManager wearManager; // Reference to NPC Manager
    [SerializeField] float surgeryMoney = 20000; // Reference to NPC Manager

    [Header("Player")]
    [SerializeField] private int howManyDaysPlayerPlay = 30;
    [SerializeField] private Player player;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform playerDayStartPosition;
    [Header("Money Manager")]
    [SerializeField] private MoneyManager moneyManager;
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
    

    [SerializeField] private int day = 1;
    [SerializeField] private int hour;
    [SerializeField] private int minute;
    private float timer = 0f;
    private bool isDayFinished = false;
    private bool npcSpawningDisabled = false; // Track if NPC spawning is disabled

    private const float sunrise = 6f;
    private const float sunPeak = 13f;
    private const float sunset = 18f;

    [Header("Gün Sonu Ekranı")]
    [SerializeField] private GameObject endOfDayScreenUI;
    [SerializeField] private TextMeshProUGUI totalMoneyEarnedText;
    [SerializeField] private TextMeshProUGUI totalMoneySpentText;
    [SerializeField] private TextMeshProUGUI totalMoneyDifferenceText;

    private float finalMoney;
    private float finalWear;

    private void Start()
    {
        // Zaman akışını başlat
        Time.timeScale = 1f;

        UpdateNPCSpawnTime();

        // Eğer yeni oyun başlatılıyorsa (kayıt yoksa)
        if (!PlayerPrefs.HasKey("SavedDay"))
        {
            // Yeni oyun başlangıç değerlerini ayarla
            day = 1;
            hour = startHour;
            minute = 0;
            isDayFinished = false;
            npcSpawningDisabled = false;

            // Oyuncuyu başlangıç pozisyonuna koy
            if (playerObject != null && playerDayStartPosition != null)
            {
                playerObject.transform.position = playerDayStartPosition.position;
                playerObject.transform.rotation = playerDayStartPosition.rotation;
            }
        }
        else
        {
            // Kaydedilmiş oyunu yükle
            LoadData();
            LoadPlayerPosition();
        }

        UpdateTimeUI();
        UpdateSunLight();

        if (npcManager == null)
        {
            npcManager = FindObjectOfType<NPCManager>();
            if (npcManager == null)
            {
                Debug.LogWarning("NPCManager bulunamadı!");
            }
        }

        // Debug kontrolleri
        Debug.Log($"Start metodu sonunda - Gün: {day}, Saat: {hour}:{minute}, isDayFinished: {isDayFinished}, TimeScale: {Time.timeScale}");
    }

    private void Update()
    {
        // Debug kontrolü
        if (Time.frameCount % 300 == 0) // 300 frame'de bir log yaz
        {
            Debug.Log($"Update çalışıyor - TimeScale: {Time.timeScale}, isDayFinished: {isDayFinished}, isPaused: {pauseMenuControllerScript?.isPaused}");
        }

        if (isDayFinished || (pauseMenuControllerScript != null && pauseMenuControllerScript.isPaused))
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= minuteDurationSeconds)
        {
            timer -= minuteDurationSeconds;
            IncrementTimeByOneMinute();
            //Debug.Log($"Zaman arttırıldı - Saat: {hour}:{minute}");
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
        if (hour >= 12)
        {

        }

        // Gece yarısında (00:00) zamanı durdur
        if (hour == 22 && minute == 0 && !npcSpawningDisabled && npcManager != null)
        {
            npcSpawningDisabled = true;
            npcManager.enabled = false; // NPCManager'ı devre dışı bırak
            Debug.Log($"Saat {hour} : {minute} oldu - Müşteri spawning durduruldu");

            Debug.Log("Gün sonu - Zaman durduruldu");
        }
        if (hour == 0 && minute == 0)
        {
            isDayFinished = true; // Gün bitişi
            Debug.Log($"Gün {day} bitti - Saat {hour}:{minute}");
        }

        UpdateTimeUI();
        UpdateSunLight();
    }

    private void UpdateTimeUI()
    {
        // Day UI güncelleme
        if (dayUIText != null)
        {
            dayUIText.text = $"Gün: {day}";
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
    public void OnDoorInteraction()
    {
        if (!isDayFinished)
        {
            Debug.Log("Gün bitmedi");
            return;
        }
        if (IsThereAnyNPC())
        {
            Debug.Log("NPC var");
            return;
        }

        UpdateNPCSpawnTime();
        SaveGame();
        day++;
        hour = startHour;
        minute = 0;
        timer = 0f;


        Cursor.lockState = CursorLockMode.None; // Fare imlecini serbest bırak
        Cursor.visible = true; // Fare imlecini görünür yap
        Time.timeScale = 0f; // Zamanı durdur
        endOfDayScreenUI.SetActive(true);
        totalMoneyEarnedText.color = Color.green;
        totalMoneySpentText.color = Color.red;
        totalMoneyEarnedText.text = "Toplam Kazanılan Para: " + moneyManager.GetDayTotalMoney().ToString("F2");
        totalMoneySpentText.text = "Toplam Harcanan Para: " + moneyManager.GetTotalSpentMoney().ToString("F2");
        totalMoneyDifferenceText.text = "Toplam Fark: " + (moneyManager.GetDayTotalMoney() - moneyManager.GetTotalSpentMoney()).ToString("F2");
        moneyManager.ResetDayMoney();

        //Gün sonu ekranı gelsin ve toplam kazanılan para ve toplam harcanan para gösterilsin ve farkı alınsın



    }
    public void OnNextDayInteraction()
    {
        if (day > howManyDaysPlayerPlay)
        {
            //Oyun burada bitiyor 
            Debug.Log("[OYUN BITTI ALOOO]Oyun bitti");
            finalMoney = moneyManager.GetMoney();
            finalWear = wearManager.GetWear();
            //1.son çürüme 50 den aşağı ise belli para yoksa birinin yardım etme şansı, çürüme yüksekse az, çürüme az ise çok şans tutarsa%70 ihtimalle karı hayatta 
            //2.son çürüme 50 den düşükse ve para varsa, %70 ihtimal ile karı yaşar
            //3.son çürüme 50 den yüksekse ve para varsa çürümenin yüksekliğine göre çürümeye göre o olasılıkla 100 99 olacak para çalınmazsa %40 ihtimalle karı yaşar
            //4.son çürüme 50 den yüksek ve para yoksa direk karı ölür

            if (finalWear <= 50)
            {//çürünürlük düşükse
                if (finalMoney < surgeryMoney)//Amelyat Parası yoksa
                {
                    if (finalWear < 10 && UnityEngine.Random.Range(0, 100) < 50)
                    {
                        //Kadın Hayatta
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            WomenSurvives(true);
                        }
                        else
                        {
                            WomenDies(6);
                        }
                    }
                    else if (finalWear < 20 && UnityEngine.Random.Range(0, 100) < 40)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            WomenSurvives(true);
                        }
                        else
                        {
                            WomenDies(6);
                        }
                    }
                    else if (finalWear < 30 && UnityEngine.Random.Range(0, 100) < 30)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            WomenSurvives(true);
                        }
                        else
                        {
                            WomenDies(6);
                        }
                    }
                    else if (finalWear < 40 && UnityEngine.Random.Range(0, 100) < 20)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            WomenSurvives(true);
                        }
                        else
                        {
                            WomenDies(6);
                        }
                    }
                    else if (finalWear <= 50 && UnityEngine.Random.Range(0, 100) < 10)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 70)
                        {
                            WomenSurvives(true);
                        }
                        else
                        {
                            WomenDies(6);
                        }
                    }
                }
                else if (finalMoney >= surgeryMoney)
                {
                    if (UnityEngine.Random.Range(0, 100) < 70)
                    {
                        WomenSurvives(false);
                    }
                    else
                    {
                        WomenDies(3);
                    }
                }
            }
            else if (finalWear > 50)
            {
                if (finalMoney < surgeryMoney)
                {
                    WomenDies(5);
                }
                else if (finalMoney >= surgeryMoney)
                {
                    if (finalWear < 60 && UnityEngine.Random.Range(0, 100) < 50)
                    {
                        //Kadın Hayatta
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            WomenSurvives(false);
                        }
                        else
                        {
                            WomenDies(3);
                        }
                    }
                    else//Para çalındı
                    {
                        WomenDies(4);
                    }
                    if (finalWear < 70 && UnityEngine.Random.Range(0, 100) < 40)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            WomenSurvives(false);
                        }
                        else
                        {
                            WomenDies(3);
                        }
                    }
                    else
                    {
                        WomenDies(4);
                    }
                    if (finalWear < 80 && UnityEngine.Random.Range(0, 100) < 30)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            WomenSurvives(false);
                        }
                        else
                        {
                            WomenDies(3);
                        }
                    }
                    else
                    {
                        WomenDies(4);
                    }
                    if (finalWear < 90 && UnityEngine.Random.Range(0, 100) < 20)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            WomenSurvives(false);
                        }
                        else
                        {
                            WomenDies(3);
                        }
                    }
                    else
                    {
                        WomenDies(4);
                    }
                    if (finalWear <= 100 && UnityEngine.Random.Range(0, 100) < 10)
                    {
                        if (UnityEngine.Random.Range(0, 100) < 40)
                        {
                            WomenSurvives(false);
                        }
                        else
                        {
                            WomenDies(3);
                        }
                    }
                    else
                    {
                        WomenDies(4);
                    }
                }
            }

            return;
        }

        SaveGame();
        //Oyuncuyu gün başlatma pozisyonuna koy
        playerObject.transform.position = playerDayStartPosition.position;
        playerObject.transform.rotation = playerDayStartPosition.rotation;

        isDayFinished = false;
        Time.timeScale = 1f; // Zamanı başlat

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Mouse.current.WarpCursorPosition(Vector2.zero);
        endOfDayScreenUI.SetActive(false);

        // Yeni gün başladığında müşteri spawning'i tekrar aktif et
        if (npcSpawningDisabled && npcManager != null)
        {
            npcSpawningDisabled = false;
            npcManager.enabled = true;
            Debug.Log("Yeni gün başladı - Müşteri spawning tekrar aktif");
        }

        UpdateTimeUI();
        UpdateSunLight();
    }

    bool IsThereAnyNPC()
    {
        bool hasCustomers = GameObject.FindGameObjectsWithTag("NPC_Customer").Length > 0;
        bool hasPrivateNPCs = GameObject.FindGameObjectsWithTag("NPC_Private").Length > 0;

        if (hasCustomers || hasPrivateNPCs)
        {
            Debug.Log("NPC var");
            return true;
        }

        Debug.Log("NPC yok");
        return false;
    }

    public int GetCurrentDay()
    {
        return day;
    }

    public int GetCurrentHour()
    {
        return hour;
    }

    private void SaveGame()
    {
        // Zaman bilgileri
        PlayerPrefs.SetInt("SavedDay", day);
        PlayerPrefs.SetInt("SavedHour", hour);
        PlayerPrefs.SetInt("SavedMinute", minute);

        // Oyuncu pozisyonu
        if (playerObject != null)
        {
            Vector3 playerPos = playerObject.transform.position;
            PlayerPrefs.SetFloat("PlayerPosX", playerPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", playerPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", playerPos.z);

            Vector3 playerRot = playerObject.transform.eulerAngles;
            PlayerPrefs.SetFloat("PlayerRotX", playerRot.x);
            PlayerPrefs.SetFloat("PlayerRotY", playerRot.y);
            PlayerPrefs.SetFloat("PlayerRotZ", playerRot.z);
        }

        // Oyun durumu
        PlayerPrefs.SetInt("IsDayFinished", isDayFinished ? 1 : 0);
        PlayerPrefs.SetInt("NPCSpawningDisabled", npcSpawningDisabled ? 1 : 0);

        // Money ve Wear değerlerini kaydet
        if (moneyManager != null)
        {
            moneyManager.SaveData();
        }

        // Tüm Saveable objeleri kaydet
        SaveManager.SaveAll();

        // Kayıtları diske yaz
        PlayerPrefs.Save();

        Debug.Log($"Oyun kaydedildi - Gün: {day} Saat: {hour}:{minute}");
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey("SavedDay"))
        {
            day = PlayerPrefs.GetInt("SavedDay");
            hour = PlayerPrefs.GetInt("SavedHour");
            minute = PlayerPrefs.GetInt("SavedMinute");
            isDayFinished = PlayerPrefs.GetInt("IsDayFinished", 0) == 1;
            npcSpawningDisabled = PlayerPrefs.GetInt("NPCSpawningDisabled", 0) == 1;

            // Eğer gece yarısı ise yeni güne geç
            if (hour == 0 && minute == 0)
            {
                day++;
                hour = startHour;
                minute = 0;
                isDayFinished = false;
            }

            // Zamanın akması için
            isDayFinished = false; // Değeri zorla false yapıyoruz
            Time.timeScale = 1f;

            Debug.Log($"Oyun yüklendi - Gün: {day}, Saat: {hour}:{minute}, Gün Bitti: {isDayFinished}, TimeScale: {Time.timeScale}");
        }
        else
        {
            // İlk kez oynanıyor
            day = 1;
            hour = startHour;
            minute = 0;
            isDayFinished = false;
            Time.timeScale = 1f;
            Debug.Log("Yeni oyun başlatıldı");
        }

        LoadPlayerPosition();
        UpdateTimeUI();
        UpdateSunLight();
    }

    private void LoadPlayerPosition()
    {
        if (PlayerPrefs.HasKey("PlayerPosX") && playerObject != null)
        {
            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat("PlayerPosX"),
                PlayerPrefs.GetFloat("PlayerPosY"),
                PlayerPrefs.GetFloat("PlayerPosZ")
            );

            Vector3 rotation = new Vector3(
                PlayerPrefs.GetFloat("PlayerRotX"),
                PlayerPrefs.GetFloat("PlayerRotY"),
                PlayerPrefs.GetFloat("PlayerRotZ")
            );

            playerObject.transform.position = position;
            playerObject.transform.eulerAngles = rotation;

            Debug.Log("Oyuncu pozisyonu yüklendi");
        }
    }

    void UpdateNPCSpawnTime()
    {
        if (day < 7)
        {
            npcManagerScript.minSpawnDelay = npcManagerScript.minSpawnArray[0];
            npcManagerScript.maxSpawnDelay = npcManagerScript.maxSpawnArray[0];
        }
        else if (day < 16)
        {
            npcManagerScript.minSpawnDelay = npcManagerScript.minSpawnArray[1];
            npcManagerScript.maxSpawnDelay = npcManagerScript.maxSpawnArray[1];
        }
        else if (day < 25)
        {
            npcManagerScript.minSpawnDelay = npcManagerScript.minSpawnArray[2];
            npcManagerScript.maxSpawnDelay = npcManagerScript.maxSpawnArray[2];
        }
        else
        {
            npcManagerScript.minSpawnDelay = npcManagerScript.minSpawnArray[3];
            npcManagerScript.maxSpawnDelay = npcManagerScript.maxSpawnArray[3];
        }
    }

    public void SaveData()
    {
        // Zaman bilgileri
        PlayerPrefs.SetInt("SavedDay", day);
        PlayerPrefs.SetInt("SavedHour", hour);
        PlayerPrefs.SetInt("SavedMinute", minute);

        // Oyuncu pozisyonu
        if (playerObject != null)
        {
            Vector3 playerPos = playerObject.transform.position;
            PlayerPrefs.SetFloat("PlayerPosX", playerPos.x);
            PlayerPrefs.SetFloat("PlayerPosY", playerPos.y);
            PlayerPrefs.SetFloat("PlayerPosZ", playerPos.z);

            Vector3 playerRot = playerObject.transform.eulerAngles;
            PlayerPrefs.SetFloat("PlayerRotX", playerRot.x);
            PlayerPrefs.SetFloat("PlayerRotY", playerRot.y);
            PlayerPrefs.SetFloat("PlayerRotZ", playerRot.z);
        }

        // Oyun durumu
        PlayerPrefs.SetInt("IsDayFinished", isDayFinished ? 1 : 0);
        PlayerPrefs.SetInt("NPCSpawningDisabled", npcSpawningDisabled ? 1 : 0);

        Debug.Log($"Oyun kaydedildi - Gün: {day} Saat: {hour}:{minute}");
    }

    private void WomenSurvives(bool isMoneyGivenByChairTable)
    {
        if (isMoneyGivenByChairTable)
        {
            SceneManager.LoadScene("FinalScene_2(happy)");
        }
        else
        {
            SceneManager.LoadScene("FinalScene_1(happy)");
        }


    }
    private void WomenDies(int endWays)
    {
        if (endWays == 3)//Para var ama kadın öldü
        {
            SceneManager.LoadScene("FinalScene_3(sad)");
        }
        else if (endWays == 4)//Para var ama çalındı ve kadın öldü
        {
            SceneManager.LoadScene("FinalScene_4(sad)");
        }
        else if (endWays == 5)//para yok ve kadın öldü
        {
            SceneManager.LoadScene("FinalScene_5(sad)");
        }
        else if (endWays == 6)//para yok yardım edildi ama kadın öldü
        {
            SceneManager.LoadScene("FinalScene_6(sad)");
        }
    }

}
