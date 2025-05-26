using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject NewGamePanel;
    public GameObject SettingsPanel;
    [SerializeField] private Button continueButton; // Devam Et butonu
    private PlayerMovementAndInteractionSystem inputActions;
    
    [Header("---------- Scene Names ----------")]
    public string MainScene;

    private void Start()
    {
        NewGamePanel.SetActive(false);
        SettingsPanel.SetActive(false);
        
        // Eğer kayıtlı oyun yoksa Devam Et butonunu devre dışı bırak
        if (continueButton != null)
        {
            continueButton.interactable = PlayerPrefs.HasKey("SavedDay");
        }
    }

    void Awake()
    {
        inputActions = new PlayerMovementAndInteractionSystem();
        inputActions.UI.Back.performed += ctx => ClosePanel();
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Disable();
    }

    void ClosePanel()
    {
        if (NewGamePanel.activeSelf)
        {
            NewGamePanel.SetActive(false);
        }

        if (SettingsPanel.activeSelf)
        {
            SettingsPanel.SetActive(false);
        }
    }

    public void LoadSceneNewGame()
    {
        // Tüm kayıtları temizle
        SaveManager.DeleteAllSaveData();
        
        // Sahneyi yükle
        SceneManager.LoadScene(MainScene);
    }

    public void LoadSceneContinue()
    {
        // Eğer kayıt varsa, devam et
        if (PlayerPrefs.HasKey("SavedDay"))
        {
            SceneManager.LoadScene(MainScene);
        }
        else
        {
            Debug.Log("Kayıt bulunamadı.");
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        Debug.Log("Oyun kapatılıyor...");
    }
}
