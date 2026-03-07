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
}