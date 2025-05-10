using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadSceneNewGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyun kapat�l�yor..."); // Bu sadece Editor'da g�rsel geri bildirim i�indir
    }

}
