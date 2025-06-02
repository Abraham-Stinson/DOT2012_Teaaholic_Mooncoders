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
    [SerializeField] private GameObject settingsPanel; // ✅ Yeni eklendi
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsButton; // ✅ Yeni eklendi
    [SerializeField] private Button backFromSettingsButton; // ✅ Yeni eklendi

    [Header("Input Ayarları")]
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private InputActionReference[] gameplayActions;
    [SerializeField] private MonoBehaviour[] playerControlScripts;
    [SerializeField] private PlayerInput playerInput;

    private Player playerController;
    public bool isPaused = false;

    private void Awake()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false); // ✅ Ayarlar paneli başta kapalı

        playerController = FindObjectOfType<Player>();

        if (playerInput == null)
            playerInput = FindObjectOfType<PlayerInput>();
    }

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += TogglePauseMenu;

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ToMainMenu);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings); // ✅

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(CloseSettings); // ✅
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= TogglePauseMenu;
        pauseAction.action.Disable();

        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ToMainMenu);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings); // ✅

        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.RemoveListener(CloseSettings); // ✅
    }

    public void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (MarketSystem.isMarketSelectionOpen || SpecialNPC.isInAnyDialogue ||
            (adisyonScript != null && adisyonScript.isAdisyonOpen) ||
            MarketSystem.isMarketOpen)
        {
            Debug.Log("Menü gösterilemiyor, bir UI zaten açık.");
            return;
        }

        if (adisyonScript == null)
            adisyonScript = FindObjectOfType<Adisyon>();

        if (isPaused)
            ContinueGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            playerInput.actions.FindActionMap("UI").Enable();
        }

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false); // ✅ Pause menüdeyken settings kapalı kalmalı

        Debug.Log("Oyun duraklatıldı");
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
            playerInput.ActivateInput();

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false); // ✅ Settings de kapansın

        Debug.Log("Oyun devam ediyor");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Oyun yeniden başlatılıyor");
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }

    // ✅ Yeni eklenen ayarlar metodları:
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        Debug.Log("Ayarlar paneli açıldı");
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        Debug.Log("Ayarlar paneli kapatıldı");
    }
}
