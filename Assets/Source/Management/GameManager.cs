using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxLevels = 3;
    [SerializeField] private float levelTimeLimit = 5f; // Changed to 5 seconds for testing
    [SerializeField] private float rewindDuration = 10f;
    
    [Header("Events")]
    public UnityEvent OnLevelStart;
    public UnityEvent OnLevelComplete;
    public UnityEvent OnTimeUp;
    public UnityEvent OnRewindStart;
    public UnityEvent OnRewindComplete;
    public UnityEvent OnGameComplete;
    public UnityEvent<float> OnTimerUpdate;
    public UnityEvent<int> OnLevelChanged;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Public properties for other scripts to access
    public int CurrentLevel { get; private set; } = 1;
    public float TimeRemaining { get; private set; }
    public bool IsRewinding { get; private set; }
    public bool IsGameActive { get; private set; }
    public bool IsGameComplete { get; private set; }
    
    // Private variables
    private Coroutine rewindCoroutine;
    private Vector3[] rewindPositions;
    private Quaternion[] rewindRotations;
    private float[] rewindTimeStamps;
    private int rewindIndex = 0;
    //TODO: Fix for 60 seconds
    private const int MAX_REWIND_SNAPSHOTS = 300; // 60 seconds * 60 snapshots per second
    private SmoothPlayerRewind smoothPlayerRewind;
    
    // RewindableObjects management
    private RewindableObject[] rewindableObjects;
    

    
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize rewind arrays
        rewindPositions = new Vector3[MAX_REWIND_SNAPSHOTS];
        rewindRotations = new Quaternion[MAX_REWIND_SNAPSHOTS];
        rewindTimeStamps = new float[MAX_REWIND_SNAPSHOTS];
    }
    
    void Start()
    {
        // Find all RewindableObjects in the scene
        rewindableObjects = FindObjectsByType<RewindableObject>(FindObjectsSortMode.None);
        
        // Find SmoothPlayerRewind
        smoothPlayerRewind = FindAnyObjectByType<SmoothPlayerRewind>();
        
        StartLevel(1);
    }
    
    void Update()
    {
        if (IsGameActive && !IsRewinding)
        {
            UpdateTimer();
            RecordRewindSnapshot();
        }
        
        if (showDebugInfo)
        {
            DisplayDebugInfo();
        }
    }
    
    public void StartLevel(int level)
    {
        if (level < 1 || level > maxLevels)
        {
            Debug.LogError($"Invalid level: {level}. Must be between 1 and {maxLevels}");
            return;
        }
        
        CurrentLevel = level;
        TimeRemaining = levelTimeLimit;
        IsGameActive = true;
        IsRewinding = false;
        
        // Reset rewind system
        rewindIndex = 0;
        
        // Refresh RewindableObjects list
        rewindableObjects = FindObjectsByType<RewindableObject>(FindObjectsSortMode.None);
        

        
        OnLevelChanged?.Invoke(CurrentLevel);
        OnLevelStart?.Invoke();
        
        Debug.Log($"Level {CurrentLevel} started. Time remaining: {TimeRemaining:F1} seconds");
    }
    
    public void CompleteLevel()
    {
        if (!IsGameActive || IsRewinding)
            return;
            
        IsGameActive = false;
        OnLevelComplete?.Invoke();
        
        Debug.Log($"Level {CurrentLevel} completed!");
        
        if (CurrentLevel >= maxLevels)
        {
            CompleteGame();
        }
        else
        {
            // Start next level after a short delay
            StartCoroutine(StartNextLevelAfterDelay(2f));
        }
    }
    
    private IEnumerator StartNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartLevel(CurrentLevel + 1);
    }
    
    private void UpdateTimer()
    {
        TimeRemaining -= Time.deltaTime;
        OnTimerUpdate?.Invoke(TimeRemaining);
        
        if (TimeRemaining <= 0)
        {
            TimeUp();
        }
    }
    
    private void TimeUp()
    {
        IsGameActive = false;
        OnTimeUp?.Invoke();
        
        Debug.Log($"Time's up on level {CurrentLevel}! Starting rewind...");
        
        // Start rewind process
        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }
        rewindCoroutine = StartCoroutine(RewindToStart());
    }
    
    private void RecordRewindSnapshot()
    {
        if (rewindIndex >= MAX_REWIND_SNAPSHOTS)
        {
            // Shift array to make room for new snapshot
            for (int i = 0; i < MAX_REWIND_SNAPSHOTS - 1; i++)
            {
                rewindPositions[i] = rewindPositions[i + 1];
                rewindRotations[i] = rewindRotations[i + 1];
                rewindTimeStamps[i] = rewindTimeStamps[i + 1];
            }
            rewindIndex = MAX_REWIND_SNAPSHOTS - 1;
        }
        
        // Record current state
        rewindPositions[rewindIndex] = GetPlayerPosition();
        rewindRotations[rewindIndex] = GetPlayerRotation();
        rewindTimeStamps[rewindIndex] = Time.time;
        rewindIndex++;
    }
    
   private IEnumerator RewindToStart()
    {
        IsRewinding = true;
        OnRewindStart?.Invoke();
        
        Debug.Log("Starting rewind process...");
        

        
        // Start rewind for all RewindableObjects
        StartRewindableObjects();
        
        // Calculate how many snapshots to rewind through
        int snapshotsToRewind = Mathf.Min(rewindIndex, (int)(rewindDuration * 10));
        float timePerSnapshot = rewindDuration / snapshotsToRewind;
        
        // Smooth rewind through snapshots
        for (int i = snapshotsToRewind - 1; i >= 0; i--)
        {
            if (i < rewindIndex && smoothPlayerRewind != null)
            {
                smoothPlayerRewind.StartSmoothRewind(
                    rewindPositions[i], 
                    rewindRotations[i], 
                    timePerSnapshot
                );
                
                // Wait for this rewind step to complete
                while (smoothPlayerRewind.IsRewinding())
                {
                    yield return null;
                }
            }
        }
        
        // Stop rewind for all RewindableObjects
        StopRewindableObjects();
        
        // Reset to level start
        ResetLevelToStart();
        
        IsRewinding = false;
        OnRewindComplete?.Invoke();
        
        Debug.Log("Rewind complete! Restarting level...");
        

        
        // Restart the level
        StartLevel(CurrentLevel);
    }
    
    private void StartRewindableObjects()
    {
        if (rewindableObjects != null)
        {
            foreach (RewindableObject rewindableObj in rewindableObjects)
            {
                if (rewindableObj != null)
                {
                    rewindableObj.StartRewind();
                }
            }
        }
    }
    
    private void StopRewindableObjects()
    {
        if (rewindableObjects != null)
        {
            foreach (RewindableObject rewindableObj in rewindableObjects)
            {
                if (rewindableObj != null)
                {
                    rewindableObj.StopRewind();
                }
            }
        }
    }
    
    private void ResetLevelToStart()
    {
        //TODO: Implement this
        // This method should reset all level elements to their starting positions
        // You'll need to implement this based on your specific level mechanics
        
        // Reset player position if needed
        // TODO: Turning this off for now.
        // Transform player = GetPlayerTransform();
        // if (player != null)
        // {
        //     player.position = GetLevelStartPosition();
        //     player.rotation = GetLevelStartRotation();
        // }
    }
    
    private void CompleteGame()
    {
        IsGameComplete = true;
        IsGameActive = false;
        OnGameComplete?.Invoke();
        
        Debug.Log("Congratulations! You've completed all levels!");
    }
    
    // Helper methods for getting/setting player transform
    private Vector3 GetPlayerPosition()
    {
        Transform player = GetPlayerTransform();
        return player != null ? player.position : Vector3.zero;
    }
    
    private Quaternion GetPlayerRotation()
    {
        Transform player = GetPlayerTransform();
        return player != null ? player.rotation : Quaternion.identity;
    }
    
    private void SetPlayerTransform(Vector3 position, Quaternion rotation)
    {
        Transform player = GetPlayerTransform();
        if (player != null)
        {
            player.position = position;
            player.rotation = rotation;
        }
    }
    
    private Transform GetPlayerTransform()
    {
        // You may need to adjust this based on your player setup
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }
    
    private Vector3 GetLevelStartPosition()
    {
        // Return the starting position for the current level
        // You'll need to implement this based on your level design
        return Vector3.zero;
    }
    
    private Quaternion GetLevelStartRotation()
    {
        // Return the starting rotation for the current level
        return Quaternion.identity;
    }
    
    private void DisplayDebugInfo()
    {
        if (IsGameActive)
        {
            Debug.Log($"Level: {CurrentLevel} | Time: {TimeRemaining:F1}s | Rewind Snapshots: {rewindIndex}");
        }
    }
    
    // Public methods for other scripts to interact with GameManager
    public void PauseGame()
    {
        IsGameActive = false;
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        IsGameActive = true;
        Time.timeScale = 1f;
    }
    
    public void RestartLevel()
    {
        StartLevel(CurrentLevel);
    }
    
    public void SkipToLevel(int level)
    {
        StartLevel(level);
    }
    
    public float GetTimeRemaining()
    {
        return TimeRemaining;
    }
    
    public int GetCurrentLevel()
    {
        return CurrentLevel;
    }
    
    public bool IsLevelComplete()
    {
        return !IsGameActive && !IsRewinding && TimeRemaining > 0;
    }
} 