using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Level");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MainMenuBtn()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit(); // Won't work in editor
    }
}
