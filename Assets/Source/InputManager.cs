using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    
    private InputAction rewindAction;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    
    // Events for other scripts to subscribe to
    public System.Action OnRewindPressed;
    public System.Action OnRewindReleased;
    
    // Properties for other scripts to check input state
    public bool IsRewindPressed { get; private set; }
    public bool IsInputEnabled { get; private set; } = true;
    
    void Awake()
    {
        if (inputActions != null)
        {
            rewindAction = inputActions.FindAction("Player/Rewind");
            moveAction = inputActions.FindAction("Player/Move");
            lookAction = inputActions.FindAction("Player/Look");
            jumpAction = inputActions.FindAction("Player/Jump");
            sprintAction = inputActions.FindAction("Player/Sprint");
            
            if (rewindAction != null)
            {
                rewindAction.performed += OnRewindPerformed;
                rewindAction.canceled += OnRewindCanceled;
            }
        }
    }
    
    void OnEnable()
    {
        if (rewindAction != null)
        {
            rewindAction.Enable();
        }
    }
    
    void OnDisable()
    {
        if (rewindAction != null)
        {
            rewindAction.Disable();
        }
    }
    
    private void OnRewindPerformed(InputAction.CallbackContext context)
    {
        if (IsInputEnabled)
        {
            IsRewindPressed = true;
            OnRewindPressed?.Invoke();
        }
    }
    
    private void OnRewindCanceled(InputAction.CallbackContext context)
    {
        IsRewindPressed = false;
        OnRewindReleased?.Invoke();
    }
    
    // Public methods to enable/disable input
    public void DisableInput()
    {
        // IsInputEnabled = false;
        // IsRewindPressed = false;
        
        // // Disable all input actions
        // if (moveAction != null) moveAction.Disable();
        // if (lookAction != null) lookAction.Disable();
        // if (jumpAction != null) jumpAction.Disable();
        // if (sprintAction != null) sprintAction.Disable();
        
        // Debug.Log("Player input disabled");
    }
    
    public void EnableInput()
    {
        IsInputEnabled = true;
        
        // Re-enable all input actions
        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (sprintAction != null) sprintAction.Enable();
        
        Debug.Log("Player input enabled");
    }
    
    // Legacy input fallback for when Input System is not enabled
    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        if (IsInputEnabled)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                OnRewindPressed?.Invoke();
            }
            if (Input.GetKeyUp(KeyCode.I))
            {
                OnRewindReleased?.Invoke();
            }
        }
#endif
    }
} 