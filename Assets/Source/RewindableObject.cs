using UnityEngine;
using System.Collections.Generic;

public class RewindableObject : MonoBehaviour
{
    [Header("Rewind Settings")]
    public float rewindDuration = 60f;
    public int fps = 60;

    private List<TimeState> history = new List<TimeState>();
    private bool isRewinding = false;
    private int maxFrames;
    private GameManager gameManager;

    void Start()
    {
        maxFrames = Mathf.RoundToInt(rewindDuration * fps);
        
        // Find the GameManager in the scene
        gameManager = GameManager.Instance;
        
        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnRewindStart.AddListener(StartRewind);
            gameManager.OnRewindComplete.AddListener(StopRewind);
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
        Debug.Log($"RewindableObject {gameObject.name} started rewinding");
    }

    public void StopRewind()
    {
        isRewinding = false;
        Debug.Log($"RewindableObject {gameObject.name} stopped rewinding");
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


