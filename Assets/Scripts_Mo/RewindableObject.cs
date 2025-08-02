using UnityEngine;
using System.Collections.Generic;

public class RewindableObject : MonoBehaviour
{
    public float rewindDuration = 60f;
    public int fps = 60;

    private List<TimeState> history = new List<TimeState>();
    private bool isRewinding = false;
    private int maxFrames;
    private InputManager inputManager;

    void Start()
    {
        maxFrames = Mathf.RoundToInt(rewindDuration * fps);
        
        // Find the InputManager in the scene
        inputManager = FindAnyObjectByType<InputManager>();
        
        // Subscribe to input events
        if (inputManager != null)
        {
            inputManager.OnRewindPressed += StartRewind;
            inputManager.OnRewindReleased += StopRewind;
        }
    }

    void Update()
    {
        // Legacy input fallback if InputManager is not found
        if (inputManager == null)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                StartRewind();
            }
            if (Input.GetKeyUp(KeyCode.I))
            {
                StopRewind();
            }
        }
    }

    void FixedUpdate()
    {
        if (isRewinding)
        {
            if (history.Count > 0)
            {
                TimeState state = history[history.Count - 1];
                transform.position = state.position;
                transform.rotation = state.rotation;
                history.RemoveAt(history.Count - 1);
            }
        }
        else 
        {
            if (history.Count >= maxFrames)
            {
                history.RemoveAt(0);
            }
            history.Add(new TimeState(transform.position, transform.rotation));
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
    }

    public void StopRewind()
    {
        isRewinding = false;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (inputManager != null)
        {
            inputManager.OnRewindPressed -= StartRewind;
            inputManager.OnRewindReleased -= StopRewind;
        }
    }
}


