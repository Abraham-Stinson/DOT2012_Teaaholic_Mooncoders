using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
