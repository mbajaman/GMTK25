using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxLevels = 3;
    [SerializeField] private float levelTimeLimit = 60f; // Changed to 5 seconds for testing
    [SerializeField] private List<GameObject> rewindPoints = new List<GameObject>();

    [SerializeField] public float rewindDuration = 10f;
    [SerializeField] public GameObject player;
    
    [Header("Events")]
    public UnityEvent OnLevelStart;
    public UnityEvent OnTimeUp;
    public UnityEvent OnRewindStart;
    public UnityEvent OnRewindComplete;
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
    private Vector3 playerStartPosition;
    
    // RewindableObjects management
    private RewindableObject[] rewindableObjects;
    private int completedRewinds = 0;
    private int totalRewindableObjects = 0;
    
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

        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");

        // Record the Player's position and rotation
        playerStartPosition = rewindPoints[0].transform.position;

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
        Time.timeScale = 1f;
        
        // Refresh RewindableObjects list
        rewindableObjects = FindObjectsByType<RewindableObject>(FindObjectsSortMode.None);

        Debug.Log($"RewindableObjects found: {rewindableObjects.Length}");
        
        OnLevelChanged?.Invoke(CurrentLevel);
        OnLevelStart?.Invoke();
        
        Debug.Log($"Level {CurrentLevel} started. Time remaining: {TimeRemaining:F1} seconds");
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

    public void CompleteLevel()
    {
        CurrentLevel++;
        if (!IsGameActive || IsRewinding)
            return;
        
        Debug.Log($"Level {CurrentLevel} completed!");

        if (CurrentLevel <= maxLevels)
        {
            // Get the rewind point for the current level
            GameObject rewindPoint = rewindPoints[CurrentLevel - 1];
            playerStartPosition = rewindPoint.transform.position;
        }

        if (CurrentLevel > maxLevels)
        {
            CompleteGame();
        }

        if (CurrentLevel < maxLevels)
        {
            StartLevel(CurrentLevel);
        }
    }
    
    private void TimeUp()
    {
        IsGameActive = false;
        OnTimeUp?.Invoke(); // To subscribe and do camera effects
        
        Debug.Log($"Time's up on level {CurrentLevel}! Starting rewind...");
        
        // Start rewind process
        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }
        rewindCoroutine = StartCoroutine(RewindToStart());

        player.GetComponent<SmoothPlayerRewind>().StartSmoothRewind(playerStartPosition, rewindDuration);
    }
    
    private IEnumerator RewindToStart()
    {
        IsRewinding = true;
        OnRewindStart?.Invoke();
        
        Debug.Log("Starting rewind process...");
        
        // Reset rewind completion tracking
        completedRewinds = 0;
        totalRewindableObjects = rewindableObjects.Length;
        Time.timeScale = 3f;
        
        // If there are no RewindableObjects, complete immediately
        if (totalRewindableObjects == 0)
        {
            Debug.Log("No RewindableObjects found in scene. Completing rewind immediately.");
            IsRewinding = false;
            OnRewindComplete?.Invoke();
            StartLevel(CurrentLevel);
            yield break;
        }
        
        Debug.Log($"Waiting for {totalRewindableObjects} RewindableObjects to complete rewind...");
        
        // Wait for all RewindableObjects to complete their rewind with timeout
        float timeoutDuration = rewindDuration * 3f; // 3x the rewind duration as timeout
        float elapsedTime = 0f;
        
        while (completedRewinds < totalRewindableObjects && elapsedTime < timeoutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (elapsedTime >= timeoutDuration)
        {
            Debug.LogWarning($"Rewind timeout reached after {timeoutDuration}s. Only {completedRewinds}/{totalRewindableObjects} objects completed.");
        }

        // Move the player to the start position within a radius of 0.1f
        while (Vector3.Distance(player.transform.position, playerStartPosition) > 0.1f)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, playerStartPosition, Time.unscaledDeltaTime * 10f);
            yield return null;
        }
        
        IsRewinding = false;
        OnRewindComplete?.Invoke();
        
        Debug.Log("All rewinds complete! Restarting level...");
        
        // Restart the level
        StartLevel(CurrentLevel);
    }
    
    // Called by RewindableObjects when they complete their rewind
    public void OnRewindableObjectComplete()
    {
        completedRewinds++;
        Debug.Log($"RewindableObject completed rewind. Progress: {completedRewinds}/{totalRewindableObjects}");
    }
    
    
    private void CompleteGame()
    {
        IsGameComplete = true;
        IsGameActive = false;
        SceneManager.LoadScene("EndMenu");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        return !IsRewinding && TimeRemaining > 0;
    }
} 