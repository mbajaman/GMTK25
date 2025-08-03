using UnityEngine;

public class LevelCompletedScript : MonoBehaviour
{
    [SerializeField] private bool isLevelComplete = false;
    private GameManager gameManager;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("LevelCompletedScript");
        Debug.Log("gameManager.IsLevelComplete(): " + gameManager.IsLevelComplete());
        if (other.gameObject.CompareTag("Player") && gameManager.IsLevelComplete() && !isLevelComplete)
        {
            isLevelComplete = true;
            gameManager.CompleteLevel();
        }
    }
}
