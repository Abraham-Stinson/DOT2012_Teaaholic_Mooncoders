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
    public bool inMainMenu;

    private PlayerMovementAndInteractionSystem inputActions;
    [SerializeField] private GameObject pressButtonUI;

    private void Awake()
    {
        inputActions = new PlayerMovementAndInteractionSystem();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pressButtonUI != null)
        {
            pressButtonUI.SetActive(false);
        }
        StartCoroutine(ShowPressButtonUI());

    }

    private void OnEnable()
    {
        if (inMainMenu)
        {
            return;
        }
        inputActions.UI.Enable();
        inputActions.UI.Submit.performed += OnSubmit;
    }

    private void OnDisable()
    {
        if (inMainMenu)
        {
            return;
        }
        inputActions.UI.Submit.performed -= OnSubmit;
        inputActions.UI.Disable();
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (inMainMenu)
        {
            return;
        }
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

    IEnumerator ShowPressButtonUI()
    {
        yield return new WaitForSeconds(4f);
        if (pressButtonUI != null)
        {
            pressButtonUI.SetActive(true);
        }
    }
}
