using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("---------- SceneNames ----------")]
    public string MainScene;
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
