using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Ses Efektleri")]
    public AudioSource cayDoldurmaSesi;
    public AudioSource okeySesi;
    public AudioSource tavlaSesi;
    public AudioSource iskambilSesi;
    public AudioSource OpeningBook;
    public AudioSource ClosingBook;
    public AudioSource Writing;
    public AudioSource Coin;
    public AudioSource Walking;
    public AudioSource SwitchOffOn;
    public AudioSource MainMenuMusic;
    public AudioSource PhonePickup;
    public AudioSource PhoneClose;
    public AudioSource Running;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("SoundManager Start çalıştı, aktif sahne: " + SceneManager.GetActiveScene().name);
        // İlk başta MainMenu sahnesindeysek müziği başlat
        StartCoroutine(CheckAndPlayMainMenuMusic());
    }

    private IEnumerator CheckAndPlayMainMenuMusic()
    {
        Debug.Log("CheckAndPlayMainMenuMusic başladı");
        // AudioSource'ların tam olarak yüklenmesini bekle
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.05f); // Süreyi artırdım

        Debug.Log("Aktif sahne: " + SceneManager.GetActiveScene().name);
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Debug.Log("MainMenu sahnesinde, müzik çalmaya çalışıyor...");
            PlayMainMenuMusic();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // Sahne yüklenince biraz bekle sonra müziği çal
            StartCoroutine(DelayedMainMenuMusic());
        }
        else
        {
            // MainMenu dışındaki sahnelerde müziği durdur
            if (MainMenuMusic != null && MainMenuMusic.isPlaying)
            {
                MainMenuMusic.Stop();
            }
        }
    }

    private IEnumerator DelayedMainMenuMusic()
    {
        yield return new WaitForSeconds(0.2f);
        PlayMainMenuMusic();
    }

    private void PlayMainMenuMusic()
    {
        if (MainMenuMusic != null)
        {
            Debug.Log("MainMenuMusic null değil, çalmaya çalışıyor...");
            Debug.Log("MainMenuMusic isPlaying: " + MainMenuMusic.isPlaying);
            Debug.Log("MainMenuMusic clip: " + (MainMenuMusic.clip != null ? MainMenuMusic.clip.name : "NULL"));
            Debug.Log("MainMenuMusic volume: " + MainMenuMusic.volume);

            if (!MainMenuMusic.isPlaying)
            {
                MainMenuMusic.Play();
                Debug.Log("MainMenuMusic.Play() çağrıldı!");
            }
        }
        else
        {
            Debug.LogError("MainMenuMusic NULL!");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Ses efektleri metodları
    public void PlayCayDoldurma() => cayDoldurmaSesi?.Play();
    public void PlayOkey() => okeySesi?.Play();
    public void PlayTavla() => tavlaSesi?.Play();
    public void PlayIskambil() => iskambilSesi?.Play();
    public void OpenBook() => OpeningBook?.Play();
    public void CloseBook() => ClosingBook?.Play();
    public void Write() => Writing?.Play();
    public void Money() => Coin?.Play();
    public void Switch() => SwitchOffOn?.Play();
    public void PhoneOpen() => PhonePickup?.Play();
    public void PhoneClosed() => PhoneClose?.Play();
    public void MenuMusic() => MainMenuMusic?.Play();

    public void Walk()
    {
        if (Walking != null && !Walking.isPlaying)
        {
            Walking.Play();
        }
    }

    public void Run()
    {
        if (Running != null && !Running.isPlaying)
        {
            Running.Play();
        }
    }

    public void StopWalkingSounds()
    {
        if (Walking != null && Walking.isPlaying)
        {
            Walking.Stop();
        }
        if (Running != null && Running.isPlaying)
        {
            Running.Stop();
        }
    }
}