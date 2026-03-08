using System.Collections.Generic;
using UnityEngine;

public class LevelDatabase : MonoBehaviour
{
    public static LevelDatabase instance; // The Singleton

    [HideInInspector]
    public List<LevelConfig> allLevelConfigs = new List<LevelConfig>();

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }

        // Load the spreadsheet instantly when the game boots up!
        LoadLevelsFromCSV();
    }

    private void LoadLevelsFromCSV()
    {
        allLevelConfigs.Clear();
        TextAsset csvFile = Resources.Load<TextAsset>("LevelData");

        if (csvFile == null)
        {
            Debug.LogError("Could not find LevelData.csv in the Resources folder!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string row = lines[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] columns = row.Split(',');

            LevelConfig newLevel = new LevelConfig();
            newLevel.levelNumber = int.Parse(columns[0]);

            newLevel.characterA = new CharacterConfig();
            newLevel.characterA.characterName = columns[1];
            newLevel.characterA.portrait = Resources.Load<Sprite>("Portraits/" + columns[2]);
            newLevel.characterA.startingMoney = float.Parse(columns[3]);
            newLevel.characterA.startingCity = columns[4];

            newLevel.characterB = new CharacterConfig();
            newLevel.characterB.characterName = columns[5];
            newLevel.characterB.portrait = Resources.Load<Sprite>("Portraits/" + columns[6]);
            newLevel.characterB.startingMoney = float.Parse(columns[7]);
            newLevel.characterB.startingCity = columns[8];

            if (columns.Length > 9 && !string.IsNullOrEmpty(columns[9]))
            {
                // We split using the semicolon!
                string[] clouds = columns[9].Split(';');
                foreach (string c in clouds)
                {
                    newLevel.cloudedCities.Add(c.Trim()); // Trim removes accidental spaces
                }
            }

            // for win text
            if (columns.Length > 10 && !string.IsNullOrEmpty(columns[10]))
            {
                newLevel.winText = columns[10].Trim();
            }
            else
            {
                // A fallback message just in case you forget to type one in the spreadsheet!
                newLevel.winText = "Level Complete!";
            }

            allLevelConfigs.Add(newLevel);
        }
        Debug.Log("Successfully loaded " + allLevelConfigs.Count + " levels from the Spreadsheet!");
    }

    // A helper function so other scripts can ask for level data easily
    public LevelConfig GetConfigForLevel(int levelNum)
    {
        foreach (LevelConfig config in allLevelConfigs)
        {
            if (config.levelNumber == levelNum) return config;
        }
        return null;
    }
}