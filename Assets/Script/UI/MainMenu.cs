using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject NewGamePanel;
    public GameObject SettingsPanel;
    private PlayerMovementAndInteractionSystem inputActions;

    [Header("---------- SceneNames ----------")]
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
        SceneManager.LoadScene(MainScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyun kapatýlýyor..."); // Bu sadece Editor'da görsel geri bildirim içindir
    }

}
