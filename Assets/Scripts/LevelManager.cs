using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Current Game State")]
    public int currentLevel = 1;
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

    private void ClearBoard()
    {
        if (playerUIManager != null)
        {
            foreach (Transform child in playerUIManager.dropZoneA) Destroy(child.gameObject);
            foreach (Transform child in playerUIManager.dropZoneB) Destroy(child.gameObject);
        }
    }
}