using UnityEngine;
using UnityEngine.InputSystem;

public class RewindTest : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string rewindActionName = "Rewind";
    [SerializeField] private string timeScaleActionName = "TimeScale";
    
    private GameManager gameManager;
    private InputAction rewindAction;
    private InputAction timeScaleAction;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
        
        // Set up input actions from your Input Action Asset
        if (inputActions != null)
        {
            rewindAction = inputActions.FindAction(rewindActionName);
            timeScaleAction = inputActions.FindAction(timeScaleActionName);
            
            if (rewindAction != null)
            {
                rewindAction.performed += OnRewindPressed;
                rewindAction.Enable();
            }
            else
            {
                Debug.LogError($"Rewind action '{rewindActionName}' not found in Input Actions!");
            }
            
            if (timeScaleAction != null)
            {
                timeScaleAction.performed += OnTimeScalePressed;
                timeScaleAction.Enable();
            }
            else
            {
                Debug.LogError($"TimeScale action '{timeScaleActionName}' not found in Input Actions!");
            }
        }
        else
        {
            Debug.LogError("Input Actions Asset not assigned to RewindTest!");
        }
    }
    
    void OnDestroy()
    {
        // Clean up input actions
        if (rewindAction != null)
        {
            rewindAction.performed -= OnRewindPressed;
            rewindAction.Disable();
        }
        
        if (timeScaleAction != null)
        {
            timeScaleAction.performed -= OnTimeScalePressed;
            timeScaleAction.Disable();
        }
    }
    
    private void OnRewindPressed(InputAction.CallbackContext context)
    {
        if (gameManager != null)
        {
            Debug.Log("Manual rewind test triggered!");
            gameManager.PauseGame();
            StartCoroutine(TestRewind());
        }
    }
    
    private void OnTimeScalePressed(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 3f;
            Debug.Log("Time scale set to 3x");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("Time scale set to 1x");
        }
    }
    
    private System.Collections.IEnumerator TestRewind()
    {
        Debug.Log("Starting test rewind...");
        
        // Trigger rewind
        gameManager.SendMessage("TimeUp");
        
        // Wait for rewind to complete
        yield return new WaitForSeconds(gameManager.rewindDuration + 1f);
        
        Debug.Log("Test rewind completed!");
        gameManager.ResumeGame();
    }
} 