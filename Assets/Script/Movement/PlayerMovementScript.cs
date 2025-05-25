using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour
{
    PlayerMovementAndInteractionSystem playerInput;
    CharacterController characterController;
    public PauseMenuController pauseMenuController;
    Player player;
    public Adisyon adisyonScript;
    [Header("Movement")]
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 currentRunMovement;
    bool isMovementPressed;
    bool isRunPressed;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runMultiply = 3f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange = 85f;
    private float cameraPitch = 0f;
    private Vector2 mouseDelta;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    // Animation parameters
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    void Awake()
    {
        playerInput = new PlayerMovementAndInteractionSystem();
        characterController = GetComponent<CharacterController>();
        pauseMenuController=GetComponent<PauseMenuController>();

        // If no camera transform assigned, try to find camera in children
        if (cameraTransform == null)
        {
            Camera mainCamera = GetComponentInChildren<Camera>();
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        // If no animator assigned, try to find animator component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Movement input callbacks
        playerInput.ChrachterController.Move.started += OnMovementInput;
        playerInput.ChrachterController.Move.canceled += OnMovementInput;
        playerInput.ChrachterController.Move.performed += OnMovementInput;
        playerInput.ChrachterController.Run.started += OnRun;
        playerInput.ChrachterController.Run.canceled += OnRun;

        // Mouse input callback
        playerInput.ChrachterController.Look.performed += OnLookInput;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;

        // Update run movement
        currentRunMovement.x = currentMovement.x * runMultiply;
        currentRunMovement.z = currentMovement.z * runMultiply;

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

        // Update animation parameters based on movement
        UpdateAnimationState();
    }

    void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();

        // Update animation parameters when run state changes
        UpdateAnimationState();
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
        // Horizontal rotation (character rotation)
        float mouseX = mouseDelta.x * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseX);

        // Vertical rotation (camera pitch)
        if (cameraTransform != null)
        {
            float mouseY = mouseDelta.y * mouseSensitivity;
            cameraPitch -= mouseY; // Invert Y axis for more natural feel
            cameraPitch = Mathf.Clamp(cameraPitch, -upDownRange, upDownRange);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        // Reset mouse delta after applying rotation
        mouseDelta = Vector2.zero;
    }

    void UpdateAnimationState()
    {
        if (animator == null) return;

        if (isMovementPressed)
        {
            if (isRunPressed)
            {
                // Running state
                animator.SetBool(IsWalking, false);
                animator.SetBool(IsRunning, true);
            }
            else
            {
                // Walking state
                animator.SetBool(IsWalking, true);
                animator.SetBool(IsRunning, false);
            }
        }
        else
        {
            // Idle state
            animator.SetBool(IsWalking, false);
            animator.SetBool(IsRunning, false);
        }
    }

    void Update()
    {
        HandleGravity();
        
        // Check if pauseMenuController is null and try to find it
        if (pauseMenuController == null)
        {
            pauseMenuController = FindObjectOfType<PauseMenuController>();
        }
        
        // Check if game is paused, dialogue is active, or market is open
        if ((pauseMenuController != null && pauseMenuController.isPaused) || SpecialNPC.isInAnyDialogue || MarketSystem.isMarketOpen) return;
        
        // If adisyon is open, prevent movement    
        if (adisyonScript != null && adisyonScript.isAdisyonOpen) return;

        HandleRotation();

        // Calculate movement direction based on character's forward direction
        Vector3 moveDirection = transform.forward * currentMovement.z + transform.right * currentMovement.x;
        moveDirection.y = currentMovement.y; // Apply gravity

        float speed = isRunPressed ? walkSpeed * runMultiply : walkSpeed;
        characterController.Move(moveDirection * speed * Time.deltaTime);

        // Ensure animation state is updated each frame
        UpdateAnimationState();
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