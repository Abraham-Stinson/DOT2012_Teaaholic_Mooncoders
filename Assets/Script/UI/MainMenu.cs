using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("---------- Scene Names ----------")]
    public string MainScene;

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
        Debug.Log("YENİ OYUN BAŞLATILIYOR...");
    }

    public void LoadSceneContinue()
    {
        // Eğer kayıt varsa, devam et
        if (PlayerPrefs.HasKey("SavedDay"))
        {
            SceneManager.LoadScene(MainScene);
            Debug.Log("DEVAM EDİLİYOR...");
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
