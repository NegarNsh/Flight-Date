using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterConfig
{
    public string characterName;
    public Sprite portrait;
    public float startingMoney;
    public string startingCity;
}

[System.Serializable]
public class LevelConfig
{
    public int levelNumber;
    public CharacterConfig characterA;
    public CharacterConfig characterB;

    // NEW: The list to hold our hidden cities!
    public List<string> cloudedCities = new List<string>();

    public string winText;
}