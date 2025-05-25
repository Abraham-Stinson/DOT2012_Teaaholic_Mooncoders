using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private DayNightCycleController dayNightCycleControllerScript;
    public Adisyon adisyonScript;
    [Header("Menü Elemanları")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    
    [Header("Input Ayarları")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference[] gameplayActions; // Player hareket, bakış, etkileşim vs
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private PlayerInput playerInput; // Oyuncunun input sistemi
    
    // Kamera kontrolü için referans
    private Player playerController;
    
    public bool isPaused = false;
    
    private void Awake()
    {
        // Canvas başlangıçta kapalı olmalı
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        // Player referansını bul
        playerController = FindObjectOfType<Player>();
        
        // PlayerInput referansı yoksa bul
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }
    }
    
    private void OnEnable()
    {
        // Input action'ları etkinleştir
        pauseAction.action.Enable();
        pauseAction.action.performed += TogglePauseMenu;
        
        // Buton click event'larını bağla
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ToMainMenu);
    }
    
    private void OnDisable()
    {
        // Input action'ları devre dışı bırak
        pauseAction.action.performed -= TogglePauseMenu;
        pauseAction.action.Disable();
        
        // Buton click event'larını temizle
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);
            
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);
            
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ToMainMenu);
    }
    
    public void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (adisyonScript == null)
        {
            Debug.Log("Adisyon scripti bulunamadı, yeni bir referans alınıyor.");
            adisyonScript = FindObjectOfType<Adisyon>();
        }

        // Adisyon açıksa menüyü açma
        if (adisyonScript != null && adisyonScript.isAdisyonOpen)
        {
            Debug.Log("Adisyon açık, menüyü açma");
            return;
        }

        // SpecialNPC diyaloğu açıksa menüyü açma
        if (SpecialNPC.isInAnyDialogue)
        {
            Debug.Log("NPC diyaloğu açık, menüyü açma");
            return;
        }

        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        
        // Oyunu duraklat
        Time.timeScale = 0f;
        isPaused = true;
        
        // Mouse imlecini görünür yap ve kilidi kaldır
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Oyuncu kontrollerini devre dışı bırak
        if (playerInput != null)
        {
            // UI action map'i dışındaki tüm action map'leri devre dışı bırak
            playerInput.DeactivateInput();
            // Sadece UI action map'ini etkinleştir (menü için)
            playerInput.actions.FindActionMap("UI").Enable();
        }
        
        // Menüyü göster
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }
        
        // Debug log
        Debug.Log("Oyun duraklatıldı");
    }
    
    public void ContinueGame()
    {
        // Oyunu devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        
        // Mouse imlecini gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Oyuncu kontrollerini tekrar etkinleştir
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
        
        // Menüyü gizle
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        // Debug log
        Debug.Log("Oyun devam ediyor");
    }
    
    public void RestartGame()
    {
        // Zaman ölçeğini normale döndür
        Time.timeScale = 1f;
        isPaused = false;
        
        // Mouse ayarlarını sıfırla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Aktif sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Debug log
        Debug.Log("Oyun yeniden başlatılıyor");
    }
    
    public void ToMainMenu()
    {
        // Zaman ölçeğini normale döndür
        Time.timeScale = 1f;
        
        // Mouse ayarlarını sıfırla
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("MainMenu");
    }
} 