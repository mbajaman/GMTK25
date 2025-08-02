# GameManager System for 60-Second Game

This system provides a complete framework for managing a 60-second game with 3 levels, timer tracking, and rewind functionality.

## Components

### GameManager.cs
The main controller for the game. Handles:
- Timer management (5 seconds per level for testing)
- Level progression (3 levels total)
- Rewind system (10-second rewind when time runs out)
- Integration with RewindableObjects
- Event system for UI and other components
- Singleton pattern for easy access

### GameUI.cs
UI controller that displays:
- Timer countdown (MM:SS format)
- Current level
- Rewind indicator
- Color-coded timer (white → yellow → red)
- Timer slider

### SmoothPlayerRewind.cs
Handles smooth player rewind animation:
- Smooth interpolation between rewind positions
- Disables player input during rewind
- Configurable rewind duration
- Easing functions for natural movement

### RewindableObject.cs
Object rewind system that:
- Records position and rotation history
- Integrates with GameManager rewind events
- Automatically rewinds when level timer expires
- Configurable rewind duration and FPS

## Setup Instructions

### 1. Create GameManager GameObject
1. Create an empty GameObject in your scene
2. Name it "GameManager"
3. Add the `GameManager` script to it
4. Configure the settings in the inspector:
   - Max Levels: 3
   - Level Time Limit: 5 (for testing)
   - Rewind Duration: 10
   - Show Debug Info: true

### 2. Setup UI
1. Create a Canvas in your scene
2. Add UI elements for:
   - Timer Text (TextMeshPro)
   - Level Text (TextMeshPro)
   - Rewind Indicator (GameObject)
   - Timer Slider (Slider)
3. Add the `GameUI` script to the Canvas
4. Assign the UI references in the inspector

### 3. Setup Player Rewind
1. Add the `SmoothPlayerRewind` script to your player GameObject
2. The script will automatically handle smooth rewind animations
3. No additional configuration needed

### 4. Setup RewindableObjects
1. Add the `RewindableObject` script to any objects you want to rewind
2. Configure rewind settings in the inspector:
   - Rewind Duration: 60 (seconds of history to store)
   - FPS: 60 (how many snapshots per second)
3. The objects will automatically rewind when the level timer expires

### 5. Player Setup
1. Ensure your player GameObject has the "Player" tag
2. The GameManager will automatically track player position for rewind

## Key Features

### Timer System
- 5-second countdown per level (configurable)
- Automatic rewind when time runs out
- Visual feedback with color changes
- Event-driven updates

### Rewind System
- Records player position 10 times per second
- Smooth 10-second rewind animation
- Integrates with RewindableObjects for comprehensive rewind
- Resets all level elements to starting positions
- Maintains game state during rewind

### Smooth Player Rewind
- Smooth interpolation between rewind positions
- Disables player input during rewind
- Uses CharacterController.Move for smooth movement
- Easing functions for natural animation

### RewindableObjects Integration
- Automatic rewind of all RewindableObjects when timer expires
- Configurable rewind duration and FPS per object
- Seamless integration with GameManager events
- No manual button input required

### Level Management
- Automatic progression through 3 levels
- Level completion detection
- Game completion handling
- Debug information display

### Event System
The GameManager provides events that other scripts can subscribe to:
- `OnLevelStart`: Called when a level begins
- `OnLevelComplete`: Called when a level is completed
- `OnTimeUp`: Called when the timer reaches zero
- `OnRewindStart`: Called when rewind begins
- `OnRewindComplete`: Called when rewind finishes
- `OnGameComplete`: Called when all levels are completed
- `OnTimerUpdate`: Called every frame with remaining time
- `OnLevelChanged`: Called when level changes

## Usage Examples

### Accessing GameManager from other scripts:
```csharp
GameManager gameManager = GameManager.Instance;
float timeRemaining = gameManager.GetTimeRemaining();
int currentLevel = gameManager.GetCurrentLevel();
```

### Subscribing to events:
```csharp
void Start()
{
    GameManager.Instance.OnLevelComplete.AddListener(OnLevelComplete);
}

void OnLevelComplete()
{
    Debug.Log("Level completed!");
}
```

### Creating custom game mechanics:
```csharp
public class CustomGameMechanic : MonoBehaviour
{
    void Start()
    {
        // Subscribe to game events
        GameManager.Instance.OnLevelStart.AddListener(ResetMechanic);
    }
    
    public void CompleteObjective()
    {
        GameManager.Instance.CompleteLevel();
    }
}
```

### Adding RewindableObjects:
```csharp
// Simply add the RewindableObject script to any GameObject
// It will automatically integrate with the GameManager rewind system
```

## Customization

### Modifying Timer Duration
Change the `levelTimeLimit` variable in GameManager inspector or modify the code.

### Adjusting Rewind Speed
Modify the `rewindDuration` variable in GameManager inspector.

### Adding More Levels
Change the `maxLevels` variable in GameManager inspector.

### Custom Game Mechanics
Create your own game systems that integrate with the GameManager events.

### RewindableObjects Settings
- Adjust `rewindDuration` to store more or less history
- Modify `fps` to change snapshot frequency
- Each object can have different settings

## Debug Features

- Console logging for all major events
- Debug info display in console
- Inspector-configurable debug settings
- Visual feedback for timer states
- RewindableObject debug messages

## Performance Notes

- Rewind system uses arrays to store position data
- Maximum 300 snapshots stored (60 seconds × 5 per second)
- Automatic cleanup of old snapshots
- Efficient event-driven architecture
- RewindableObjects use Lists for flexible history storage 