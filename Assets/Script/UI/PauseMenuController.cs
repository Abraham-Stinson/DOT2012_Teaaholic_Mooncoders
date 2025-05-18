using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menü Elemanları")]
    [SerializeField] private GameObject pauseMenuCanvas;

    [Header("Input Ayarları")]
    [SerializeField] private InputActionReference pauseAction; // ESC tuşu gibi
    [SerializeField] private InputActionReference[] gameplayActions; // Player hareket, bakış, etkileşim vs
    [SerializeField] private MonoBehaviour[] playerControlScripts;


    private bool isPaused = false;

    private void Awake()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePressed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePressed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
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

        // Tüm gameplay inputlarını devre dışı bırak
        foreach (var actionRef in gameplayActions)
        {
            if (actionRef != null)
                actionRef.action.Disable();
        }

        foreach (var script in playerControlScripts)
        {
            script.enabled = false;
        }


        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        Debug.Log("Oyun duraklatıldı");
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Tüm gameplay inputlarını tekrar etkinleştir
        foreach (var actionRef in gameplayActions)
        {
            if (actionRef != null)
                actionRef.action.Enable();
        }

        foreach (var script in playerControlScripts)
        {
            script.enabled = true;
        }


        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        Debug.Log("Oyun devam ediyor");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }
}
