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
}