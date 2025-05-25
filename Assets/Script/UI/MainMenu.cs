using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject NewGamePanel;
    public GameObject SettingsPanel;
    private PlayerMovementAndInteractionSystem inputActions;
    
    [Header("---------- Scene Names ----------")]
    public string MainScene;

    private void Start()
    {
        NewGamePanel.SetActive(false);
        SettingsPanel.SetActive(false);
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
        // Yeni oyun başlatıldığında tüm kayıtları temizle
        PlayerPrefs.DeleteKey("SavedDay");
        PlayerPrefs.DeleteKey("SavedHour");
        PlayerPrefs.DeleteKey("SavedMinute");

        // Pozisyon kayıtları da silinsin (önceden kaydedilmiş objeler varsa)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

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
        Application.Quit();
        Debug.Log("Oyun kapatılıyor...");
    }
}
