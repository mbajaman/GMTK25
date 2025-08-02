using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject rewindIndicator;
    [SerializeField] private Slider timerSlider;
    
    [Header("UI Settings")]
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color dangerTimerColor = Color.red;
    [SerializeField] private float warningThreshold = 15f;
    [SerializeField] private float dangerThreshold = 5f;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        if (gameManager != null)
        {
            // Subscribe to events
            gameManager.OnTimerUpdate.AddListener(UpdateTimerDisplay);
            gameManager.OnLevelChanged.AddListener(UpdateLevelDisplay);
            gameManager.OnRewindStart.AddListener(ShowRewindIndicator);
            gameManager.OnRewindComplete.AddListener(HideRewindIndicator);
            gameManager.OnLevelStart.AddListener(OnLevelStart);
            
            // Initialize display
            UpdateLevelDisplay(gameManager.CurrentLevel);
            UpdateTimerDisplay(gameManager.TimeRemaining);
        }
        else
        {
            Debug.LogError("GameManager not found! Make sure GameManager is in the scene.");
        }
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            // Unsubscribe from events
            gameManager.OnTimerUpdate.RemoveListener(UpdateTimerDisplay);
            gameManager.OnLevelChanged.RemoveListener(UpdateLevelDisplay);
            gameManager.OnRewindStart.RemoveListener(ShowRewindIndicator);
            gameManager.OnRewindComplete.RemoveListener(HideRewindIndicator);
            gameManager.OnLevelStart.RemoveListener(OnLevelStart);
        }
    }
    
    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Change color based on time remaining
            if (timeRemaining <= dangerThreshold)
            {
                timerText.color = dangerTimerColor;
            }
            else if (timeRemaining <= warningThreshold)
            {
                timerText.color = warningTimerColor;
            }
            else
            {
                timerText.color = normalTimerColor;
            }
        }
        
        if (timerSlider != null)
        {
            // Update slider value (assuming 60 second max)
            timerSlider.value = timeRemaining / 60f;
        }
    }
    
    private void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }
    
    private void ShowRewindIndicator()
    {
        if (rewindIndicator != null)
        {
            rewindIndicator.SetActive(true);
        }
    }
    
    private void HideRewindIndicator()
    {
        if (rewindIndicator != null)
        {
            rewindIndicator.SetActive(false);
        }
    }
    
    private void OnLevelStart()
    {
        // Reset timer display
        UpdateTimerDisplay(gameManager.TimeRemaining);
        HideRewindIndicator();
    }
} 