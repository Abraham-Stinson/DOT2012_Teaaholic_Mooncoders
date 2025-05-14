using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menü Elemanları")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    
    [Header("Input Ayarları")]
    [SerializeField] private InputActionReference pauseAction;
    
    private bool isPaused = false;
    
    private void Awake()
    {
        // Canvas başlangıçta kapalı olmalı
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
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
            exitButton.onClick.AddListener(ExitGame);
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
            exitButton.onClick.RemoveListener(ExitGame);
    }
    
    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    private void PauseGame()
    {
        // Oyunu duraklat
        Time.timeScale = 0f;
        isPaused = true;
        
        // Menüyü göster
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }
        
        // Debug log
        Debug.Log("Oyun duraklatıldı");
    }
    
    private void ContinueGame()
    {
        // Oyunu devam ettir
        Time.timeScale = 1f;
        isPaused = false;
        
        // Menüyü gizle
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        // Debug log
        Debug.Log("Oyun devam ediyor");
    }
    
    private void RestartGame()
    {
        // Zaman ölçeğini normale döndür
        Time.timeScale = 1f;
        isPaused = false;
        
        // Aktif sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Debug log
        Debug.Log("Oyun yeniden başlatılıyor");
    }
    
    private void ExitGame()
    {
        // Debug log
        Debug.Log("Oyundan çıkılıyor");
        
#if UNITY_EDITOR
        // Unity Editor'da çalışırken oyunu durdur
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Build'de oyundan çık
        Application.Quit();
#endif
    }
} 