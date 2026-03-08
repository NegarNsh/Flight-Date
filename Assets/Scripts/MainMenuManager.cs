using UnityEngine;
using UnityEngine.SceneManagement; // NEW: This is the magic tool that loads levels!

public class MainMenuManager : MonoBehaviour
{
    // This function will be triggered by our Start button
    public void PlayGame()
    {
        // Tell Unity to load the scene exactly named "GameScene"
        // Make sure this perfectly matches whatever you named your puzzle scene!
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        // 1. Snitch to the console so we know it works in the Editor
        Debug.Log("QUIT BUTTON CLICKED! (The game will close in the final build)");

        // 2. The actual command that closes the application
        Application.Quit();
    }

}