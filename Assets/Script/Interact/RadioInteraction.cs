using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class RadioInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public Camera playerCamera;
    public TextMeshProUGUI interactionText;

    [Header("Audio Settings")]
    public AudioSource radioAudioSource;
    public AudioClip[] staticNoiseClips;
    public AudioClip[] musicTracks;

    private int currentTrackIndex = 0;
    private bool isRadioOn = false;
    private bool isTransitioning = false;

    private PlayerMovementAndInteractionSystem inputActions;

    void Awake()
    {
        inputActions = new PlayerMovementAndInteractionSystem();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.ChrachterController.Use.performed += OnInteractPressed;
    }

    void OnDisable()
    {
        inputActions.ChrachterController.Use.performed -= OnInteractPressed;
        inputActions.Disable();
    }

    void Update()
    {
        ShowInteractionPrompt();
    }

    void ShowInteractionPrompt()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            switch (hit.collider.tag)
            {
                case "RadioPowerButton":
                    interactionText.text = "Radyo Aç/Kapa (F)";
                    interactionText.enabled = true;
                    return;

                case "RadioChangeTrackButton":
                    interactionText.text = "Þarký Deðiþtir (F)";
                    interactionText.enabled = true;
                    return;
            }
        }

        interactionText.enabled = false;
    }

    void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (isTransitioning) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            switch (hit.collider.tag)
            {
                case "RadioPowerButton":
                    PlayStaticThenTogglePower(); // coroutine yok, doðrudan çaðrýlýyor
                    break;

                case "RadioChangeTrackButton":
                    StartCoroutine(PlayStaticThenChangeTrack());
                    break;
            }
        }
    }

    void PlayStaticThenTogglePower()
    {
        isTransitioning = true;

        isRadioOn = !isRadioOn;

        if (isRadioOn && musicTracks.Length > 0)
        {
            radioAudioSource.clip = musicTracks[currentTrackIndex];
            radioAudioSource.loop = true;
            radioAudioSource.Play();
        }
        else
        {
            radioAudioSource.Stop();
            radioAudioSource.clip = null;
        }

        isTransitioning = false;
        Debug.Log("Radyo durumu: " + (isRadioOn ? "Açýk" : "Kapalý"));
    }

    IEnumerator PlayStaticThenChangeTrack()
    {
        if (!isRadioOn || musicTracks.Length == 0) yield break;

        isTransitioning = true;

        PlayRandomStatic();
        yield return new WaitForSeconds(radioAudioSource.clip.length);

        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        radioAudioSource.clip = musicTracks[currentTrackIndex];
        radioAudioSource.loop = true;
        radioAudioSource.Play();

        isTransitioning = false;
        Debug.Log("Þarký deðiþtirildi: " + radioAudioSource.clip.name);
    }

    void PlayRandomStatic()
    {
        if (staticNoiseClips.Length == 0) return;

        AudioClip staticClip = staticNoiseClips[Random.Range(0, staticNoiseClips.Length)];
        radioAudioSource.Stop();
        radioAudioSource.clip = staticClip;
        radioAudioSource.loop = false;
        radioAudioSource.Play();
    }
}
