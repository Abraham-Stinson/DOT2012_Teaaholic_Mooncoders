using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("---------- GameObjects ----------")]
    public GameObject pauseMenuUI;
    private bool isPaused = false;
    [Header("---------- SceneNames ----------")]
    public string MainMenuScene;
    void Start()
    {
        Resume(); // Oyun baþladýðýnda menü kapalý olmalý
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadSceneMainMenu()
    {
        SceneManager.LoadScene(MainMenuScene);
        Time.timeScale = 1f;
    }


}
