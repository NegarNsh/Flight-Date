using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Current Game State")]
    public int currentLevel = 1;
    public int totalLevels = 5;
    public bool isLevelActive = false;

    [Header("Script Connections")]
    public UIManager uiManager;
    public PlayerUIManager playerUIManager;
    public MapManager mapManager;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        Invoke("LoadCurrentLevel", 0.1f);
    }

    public void LoadCurrentLevel()
    {
        List<Flight> flightsForThisLevel = FlightDatabase.instance.GetFlightsForLevel(currentLevel);
        if (uiManager != null) uiManager.DisplayFlights(flightsForThisLevel);

        // ---> THE UI SPACING FIX <---
        // This acts like a giant "Refresh" button for the entire Canvas.
        // It forces the Shop's Layout Group to do its math instantly before the player even sees it!
        Canvas.ForceUpdateCanvases();

        // ---> NEW TIMELINE GENERATOR TRIGGER <---
        if (playerUIManager != null)
        {
            playerUIManager.dropZoneA.GetComponent<TimelineColumn>().GenerateTimeline(flightsForThisLevel);
            playerUIManager.dropZoneB.GetComponent<TimelineColumn>().GenerateTimeline(flightsForThisLevel);
        }

        // Notice we ask the new LevelDatabase for the config!
        LevelConfig configForThisLevel = LevelDatabase.instance.GetConfigForLevel(currentLevel);

        if (configForThisLevel != null && playerUIManager != null)
        {
            playerUIManager.SetupLevelCharacters(configForThisLevel);
        }
        else
        {
            Debug.LogError("Hey! You forgot to set up Level " + currentLevel + " in the CSV!");
            return;
        }

        if (mapManager != null && configForThisLevel != null)
        {
            //Setup the clouds FIRST!
            mapManager.SetupClouds(configForThisLevel.cloudedCities);
            // Then place the avatars!
            mapManager.PlaceAvatarsAtStart(configForThisLevel.characterA.startingCity, configForThisLevel.characterB.startingCity);
        }

        if (GameResultManager.instance != null)
        {
            // NEW: If currentLevel is 5, and totalLevels is 5, it knows this is the end!
            GameResultManager.instance.isFinalLevel = (currentLevel == totalLevels);
        }

        isLevelActive = true;
    }

    // --- BUTTON FUNCTIONS ---

    public void FinishLevel()
    {
        isLevelActive = false;

        // Pass the check off to our new GameResultManager!
        LevelConfig config = LevelDatabase.instance.GetConfigForLevel(currentLevel);
        if (GameResultManager.instance != null)
        {
            GameResultManager.instance.CheckWinCondition(playerUIManager, config);
        }
    }

    public void GoToNextLevel()
    {
        currentLevel++;
        if (GameResultManager.instance != null) GameResultManager.instance.HideAllScreens();
        ClearBoard();
        LoadCurrentLevel();
    }

    public void RetryLevel()
    {
        if (GameResultManager.instance != null) GameResultManager.instance.HideAllScreens();
        ClearBoard();
        LoadCurrentLevel();
    }

    public void GoToMainMenu()
    {
        // NOTE: Change "MainMenu" to the EXACT name of your actual menu scene file!
        SceneManager.LoadScene("MainMenu");
    }

    private void ClearBoard()
    {
        if (playerUIManager != null)
        {
            foreach (Transform child in playerUIManager.dropZoneA) Destroy(child.gameObject);
            foreach (Transform child in playerUIManager.dropZoneB) Destroy(child.gameObject);

            if (MapManager.instance != null) MapManager.instance.ClearAllLines();
        }
    }


}