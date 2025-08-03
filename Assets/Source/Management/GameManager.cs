using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxLevels = 3;
    [SerializeField] private float levelTimeLimit = 60f; // Changed to 5 seconds for testing
    [SerializeField] public float rewindDuration = 10f;
    
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
    }
    
    void Start()
    {
        // Cap frame rate to 60 FPS
        Application.targetFrameRate = 60;

        // Find all RewindableObjects in the scene
        rewindableObjects = FindObjectsByType<RewindableObject>(FindObjectsSortMode.None);
        
        StartLevel(1);
    }
    
    void Update()
    {
        if (IsGameActive && !IsRewinding)
        {
            UpdateTimer();
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
        
        // Refresh RewindableObjects list
        rewindableObjects = FindObjectsByType<RewindableObject>(FindObjectsSortMode.None);

        Debug.Log($"RewindableObjects found: {rewindableObjects.Length}");
        
        OnLevelChanged?.Invoke(CurrentLevel);
        OnLevelStart?.Invoke();
        
        Debug.Log($"Level {CurrentLevel} started. Time remaining: {TimeRemaining:F1} seconds");
    }
    
    public void CompleteLevel()
    {
        //TODO: Implement successful level completion
        // if (!IsGameActive || IsRewinding)
        //     return;
            
        // IsGameActive = false;
        // OnLevelComplete?.Invoke();
        
        // Debug.Log($"Level {CurrentLevel} completed!");
        
        // if (CurrentLevel >= maxLevels)
        // {
        //     CompleteGame();
        // }
    }
    
    private void UpdateTimer()
    {
        if (IsRewinding)
        {
            // During rewind, increment the timer based on rewind duration and time scale
            TimeRemaining += Time.deltaTime * Time.timeScale;
            OnTimerUpdate?.Invoke(TimeRemaining);
        }
        else if (IsGameActive)
        {
            // During normal gameplay, decrement the timer
            TimeRemaining -= Time.deltaTime;
            OnTimerUpdate?.Invoke(TimeRemaining);
            
            if (TimeRemaining <= 0)
            {
                TimeUp();
            }
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
    
       private IEnumerator RewindToStart()
    {
        IsRewinding = true;
        OnRewindStart?.Invoke();
        
        Debug.Log("Starting rewind process...");
        
        // Wait for the rewind duration (RewindableObjects will handle their own timing)
        yield return new WaitForSeconds(rewindDuration);
        
        IsRewinding = false;
        OnRewindComplete?.Invoke();
        
        Debug.Log("Rewind complete! Restarting level...");
        
        // Restart the level
        StartLevel(CurrentLevel);
    }
    
    
    private void CompleteGame()
    {
        IsGameComplete = true;
        IsGameActive = false;
        OnGameComplete?.Invoke();
        
        Debug.Log("Congratulations! You've completed all levels!");
    }

    private void DisplayDebugInfo()
    {
        if (IsGameActive)
        {
            Debug.Log($"Level: {CurrentLevel} | Time: {TimeRemaining:F1}s");
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