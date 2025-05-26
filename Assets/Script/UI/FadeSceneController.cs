using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class FadeSceneController : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    public string nextSceneName;

    private PlayerMovementAndInteractionSystem inputActions;

    private void Awake()
    {
        inputActions = new PlayerMovementAndInteractionSystem();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Submit.performed += OnSubmit;
    }

    private void OnDisable()
    {
        inputActions.UI.Submit.performed -= OnSubmit;
        inputActions.UI.Disable();
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeIn()
    {
        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            fadeCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
    }

    private IEnumerator FadeOutAndLoad()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        // Sahne ge�i�i
        SceneManager.LoadScene(nextSceneName);
    }
}
