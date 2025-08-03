using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RewindableObject : MonoBehaviour
{
    [Header("Rewind Settings")]
    [SerializeField] private int maxHistoryFrames = 3600; // 60 seconds at 60fps
    // [SerializeField] private float rewindTimeScale = 3f; // Speed up rewind by 3x

    private List<TimeState> history = new List<TimeState>();

    private GameManager gameManager;
    private Coroutine rewindCoroutine;

    private BoxCollider bc;
    private Rigidbody rb = new Rigidbody();
    
    // private float originalTimeScale;
    private bool isRewinding = false;
    private bool hasNotifiedCompletion = false;

    void Start()
    {
        // Find the GameManager in the scene
        gameManager = GameManager.Instance;
        
        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnRewindStart.AddListener(StartRewind);
            gameManager.OnRewindComplete.AddListener(StopRewind);
        }

        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isRewinding)
        {
            // Only record history when not rewinding
            if (history.Count >= maxHistoryFrames)
            {
                Debug.Log($"Removing history at index 0");
                history.RemoveAt(0);
            }
            history.Add(new TimeState(transform.position, transform.rotation));
        }
    }

    public void StartRewind()
    {
        if (isRewinding) return; // Already rewinding

        isRewinding = true;
        hasNotifiedCompletion = false;
        bc.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;

        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }
        rewindCoroutine = StartCoroutine(RewindCoroutine());
    }

    public void StopRewind()
    {
        Debug.Log($"Stopping rewind...");

        isRewinding = false;
        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
            rewindCoroutine = null;
        }
        
        // Restore original time scale
        // Time.timeScale = originalTimeScale;

        // Clear history after rewind
        history.Clear();
        bc.enabled = true;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    private IEnumerator RewindCoroutine()
    {
        if (history.Count == 0)
        {
            Debug.LogWarning($"RewindableObject {gameObject.name} has no history to rewind");
            // Notify GameManager that this object has completed (even though it had nothing to rewind)
            if (gameManager != null && !hasNotifiedCompletion)
            {
                hasNotifiedCompletion = true;
                gameManager.OnRewindableObjectComplete();
            }
            yield break;
        }

        // Store original time scale and speed up rewind
        // originalTimeScale = Time.timeScale;
        // Time.timeScale = rewindTimeScale;
        
        // Calculate how many history frames to process
        int totalFrames = history.Count;

        // Process frames in reverse order
        for (int i = totalFrames - 1; i >= 0; i--)
        {
            // DEV ONLY
            // Debug.Log($"Processing frame {i}");
                        
            TimeState state = history[i];
            transform.position = state.position;
            transform.rotation = state.rotation;
            i -= 6;
            yield return new WaitForSeconds(0.005f);
        }

        // Notify GameManager that this object has completed its rewind
        if (gameManager != null && !hasNotifiedCompletion)
        {
            hasNotifiedCompletion = true;
            gameManager.OnRewindableObjectComplete();
        }
        
        Debug.Log($"RewindableObject {gameObject.name} completed rewind");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnRewindStart.RemoveListener(StartRewind);
            gameManager.OnRewindComplete.RemoveListener(StopRewind);
        }
    }
}


