using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RewindableObject : MonoBehaviour
{
    [Header("Rewind Settings")]
    [SerializeField] private int maxHistoryFrames = 3600; // 60 seconds at 60fps
    [SerializeField] private float rewindTimeScale = 3f; // Speed up rewind by 3x

    private List<TimeState> history = new List<TimeState>();
    private bool isRewinding = false;
    private GameManager gameManager;
    private Coroutine rewindCoroutine;
    private float originalTimeScale;
    private BoxCollider boxCollider;
    private Rigidbody rigidbody;

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

        boxCollider = GetComponent<BoxCollider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isRewinding)
        {
            // Only record history when not rewinding
            if (history.Count >= maxHistoryFrames)
            {
                history.RemoveAt(0);
            }
            history.Add(new TimeState(transform.position, transform.rotation));
        }
    }

    public void StartRewind()
    {
        if (isRewinding) return; // Already rewinding

        isRewinding = true;
        boxCollider.enabled = false;
        rigidbody.useGravity = false;

        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }
        rewindCoroutine = StartCoroutine(RewindCoroutine());
    }

    public void StopRewind()
    {
        isRewinding = false;
        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
            rewindCoroutine = null;
        }
        
        // Restore original time scale
        Time.timeScale = originalTimeScale;
    }

    private IEnumerator RewindCoroutine()
    {
        Debug.Log($"History count: {history.Count}");

        if (history.Count == 0)
        {
            Debug.LogWarning($"RewindableObject {gameObject.name} has no history to rewind");
            yield break;
        }

        // Store original time scale and speed up rewind
        originalTimeScale = Time.timeScale;
        Time.timeScale = rewindTimeScale;

        // Get rewind duration from GameManager
        float rewindDuration = gameManager != null ? gameManager.rewindDuration : 10f;
        
        // Calculate frames per second for rewind (accounting for time scale)
        float rewindFPS = 60f * rewindTimeScale;
        float timePerFrame = 1f / rewindFPS;
        
        // Calculate how many history frames to process
        int totalFrames = history.Count;
        int framesToProcess = Mathf.Min(totalFrames, Mathf.RoundToInt(rewindDuration * rewindFPS));
        
        // Process frames in reverse order
        for (int i = totalFrames - 1; i >= totalFrames - framesToProcess; i--)
        {
            Debug.Log($"Processing frame {i}");
            Debug.Log($"isRewinding: {isRewinding}");
            
            if (!isRewinding) break; // Check if rewind was stopped
            
            TimeState state = history[i];
            transform.position = state.position;
            transform.rotation = state.rotation;
            
            // Wait for one frame at the sped-up time scale
            yield return new WaitForSeconds(timePerFrame);
        }
        
        // Clear history after rewind
        history.Clear();
        isRewinding = false;
        rewindCoroutine = null;
        boxCollider.enabled = true;
        rigidbody.useGravity = true;

        // Restore original time scale
        Time.timeScale = originalTimeScale;
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


