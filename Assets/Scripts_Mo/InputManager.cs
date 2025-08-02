using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    
    private InputAction rewindAction;
    
    // Events for other scripts to subscribe to
    public System.Action OnRewindPressed;
    public System.Action OnRewindReleased;
    
    // Properties for other scripts to check input state
    public bool IsRewindPressed { get; private set; }
    
    void Awake()
    {
        if (inputActions != null)
        {
            rewindAction = inputActions.FindAction("Player/Rewind");
            
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
        IsRewindPressed = true;
        OnRewindPressed?.Invoke();
    }
    
    private void OnRewindCanceled(InputAction.CallbackContext context)
    {
        IsRewindPressed = false;
        OnRewindReleased?.Invoke();
    }
    
    // Legacy input fallback for when Input System is not enabled
    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        if (Input.GetKeyDown(KeyCode.I))
        {
            OnRewindPressed?.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            OnRewindReleased?.Invoke();
        }
#endif
    }
} 