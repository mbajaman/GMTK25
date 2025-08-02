using UnityEngine;
using System.Collections;

public class SmoothPlayerRewind : MonoBehaviour
{
    [Header("Rewind Settings")]
    private CharacterController characterController;
    private MonoBehaviour[] movementScripts;
    private Transform playerTransform;
    private bool isRewinding = false;
    
    void Start()
    {
        SetupPlayerComponents();
    }
    
    private void SetupPlayerComponents()
    {
        playerTransform = transform;
        characterController = GetComponent<CharacterController>();
        
        // Find all movement-related scripts to disable during rewind
        movementScripts = GetComponents<MonoBehaviour>();
    }
    
    public void StartSmoothRewind(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        if (!isRewinding)
        {
            StartCoroutine(SmoothRewindCoroutine(targetPosition, targetRotation, duration));
        }
    }
    
    private IEnumerator SmoothRewindCoroutine(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        isRewinding = true;
        
        // Disable player components for smooth rewind
        DisablePlayerComponents();
        
        Vector3 startPosition = playerTransform.position;
        Quaternion startRotation = playerTransform.rotation;
        float elapsed = 0f;
        
        Debug.Log($"Starting smooth rewind from {startPosition} to {targetPosition}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Use smooth interpolation with easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Interpolate position
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            // Interpolate rotation
            Quaternion newRotation = Quaternion.Lerp(startRotation, targetRotation, smoothT);
            
            // Apply position and rotation
            if (characterController != null)
            {
                // Use CharacterController.Move for smooth movement
                Vector3 moveVector = newPosition - playerTransform.position;
                characterController.Move(moveVector);
            }
            else
            {
                playerTransform.position = newPosition;
            }
            
            playerTransform.rotation = newRotation;
            
            yield return null;
        }
        
        // Ensure final position is exact
        if (characterController != null)
        {
            Vector3 finalMove = targetPosition - playerTransform.position;
            characterController.Move(finalMove);
        }
        else
        {
            playerTransform.position = targetPosition;
        }
        playerTransform.rotation = targetRotation;
        
        // Re-enable player components
        EnablePlayerComponents();
        
        isRewinding = false;
        Debug.Log("Smooth rewind complete");
    }
    
    private void DisablePlayerComponents()
    {
        // if (characterController != null)
        // {
        //     characterController.enabled = false;
        // }
        
        if (movementScripts != null)
        {
            foreach (MonoBehaviour script in movementScripts)
            {
                if (script != null && script != this && 
                    (script.GetType().Name.Contains("FirstPersonController") ||
                     script.GetType().Name.Contains("Input") ||
                     script.GetType().Name.Contains("Movement")))
                {
                    script.enabled = false;
                }
            }
        }
        
        Debug.Log("Player components disabled for smooth rewind");
    }
    
    private void EnablePlayerComponents()
    {
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        if (movementScripts != null)
        {
            foreach (MonoBehaviour script in movementScripts)
            {
                if (script != null && script != this && 
                    (script.GetType().Name.Contains("Controller") || 
                     script.GetType().Name.Contains("Input") ||
                     script.GetType().Name.Contains("Movement")))
                {
                    script.enabled = true;
                }
            }
        }
        
        Debug.Log("Player components enabled");
    }
    
    public bool IsRewinding()
    {
        return isRewinding;
    }
}