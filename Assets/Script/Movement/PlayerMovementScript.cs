using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour
{
    PlayerMovementAndInteractionSystem playerInput;
    CharacterController characterController;
    public PauseMenuController pauseMenuController;
    public Adisyon adisyonScript;

    [Header("Movement")]
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiply = 2f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange = 85f;
    private float cameraPitch = 0f;
    private Vector2 mouseDelta;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    void Awake()
    {
        playerInput = new PlayerMovementAndInteractionSystem();
        characterController = GetComponent<CharacterController>();
        pauseMenuController = GetComponent<PauseMenuController>();

        if (cameraTransform == null)
        {
            Camera mainCamera = GetComponentInChildren<Camera>();
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInput.ChrachterController.Move.started += OnMovementInput;
        playerInput.ChrachterController.Move.canceled += OnMovementInput;
        playerInput.ChrachterController.Move.performed += OnMovementInput;
        playerInput.ChrachterController.Run.started += OnRun;
        playerInput.ChrachterController.Run.canceled += OnRun;
        playerInput.ChrachterController.Look.performed += OnLookInput;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovement.x * runMultiply;
        currentRunMovement.z = currentMovement.z * runMultiply;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        UpdateAnimationState();
        UpdateWalkingSound(); // SES GÜNCELLEME EKLENDİ
    }

    void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
        UpdateAnimationState();
        UpdateWalkingSound(); // SES GÜNCELLEME EKLENDİ
    }

    void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundedGravity = -0.05f;
            currentMovement.y = groundedGravity;
            currentRunMovement.y = groundedGravity;
        }
        else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity * Time.deltaTime;
            currentRunMovement.y += gravity * Time.deltaTime;
        }
    }

    void HandleRotation()
    {
        float mouseX = mouseDelta.x * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseX);

        if (cameraTransform != null)
        {
            float mouseY = mouseDelta.y * mouseSensitivity;
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -upDownRange, upDownRange);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        mouseDelta = Vector2.zero;
    }

    void UpdateAnimationState()
    {
        if (animator == null) return;

        if (isMovementPressed)
        {
            if (isRunPressed)
            {
                animator.SetBool(IsWalking, false);
                animator.SetBool(IsRunning, true);
            }
            else
            {
                animator.SetBool(IsWalking, true);
                animator.SetBool(IsRunning, false);
            }
        }
        else
        {
            animator.SetBool(IsWalking, false);
            animator.SetBool(IsRunning, false);
        }
    }

    void UpdateWalkingSound()
    {
        if (SoundManager.Instance == null) return;
        
        if (!isMovementPressed)
        {
            SoundManager.Instance.StopWalkingSounds();
            return;
        }

        if (isRunPressed)
        {
            SoundManager.Instance.StopWalkingSounds();
            SoundManager.Instance.Run();
        }
        else
        {
            SoundManager.Instance.StopWalkingSounds();
            SoundManager.Instance.Walk();
        }
    }

    void Update()
    {
        HandleGravity();

        if (pauseMenuController == null)
            pauseMenuController = FindObjectOfType<PauseMenuController>();
        if (pauseMenuController != null && pauseMenuController.isPaused) return;
        if (adisyonScript != null && adisyonScript.isAdisyonOpen) return;
        if (SpecialNPC.isInAnyDialogue) return;
        if (MarketSystem.isMarketOpen) return;
        if (MarketSystem.isMarketSelectionOpen) return;

        HandleRotation();

        Vector3 moveDirection = transform.forward * currentMovement.z + transform.right * currentMovement.x;
        moveDirection.y = currentMovement.y;

        float speed = isRunPressed ? walkSpeed * runMultiply : walkSpeed;
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    void OnEnable()
    {
        playerInput.ChrachterController.Enable();
    }

    void OnDisable()
    {
        playerInput.ChrachterController.Disable();
    }
}