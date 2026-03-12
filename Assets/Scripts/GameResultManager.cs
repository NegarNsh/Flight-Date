using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameResultManager : MonoBehaviour
{
    // --- VARIABLES ---
    public static GameResultManager instance;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject endGameScreen; // NEW: The final victory screen!

    [Header("UI Text")]
    public TextMeshProUGUI winTextDisplay;

    [Header("In-Game Portraits")]
    public UnityEngine.UI.Image gamePortraitA;
    public UnityEngine.UI.Image gamePortraitB;

    [Header("Win Screen Portraits")]
    public UnityEngine.UI.Image winPortraitA;
    public UnityEngine.UI.Image winPortraitB;

    [Header("Lose Screen Portraits")]
    public UnityEngine.UI.Image losePortraitA;
    public UnityEngine.UI.Image losePortraitB;

    [HideInInspector]
    public bool isFinalLevel = false; // NEW: The LevelManager will toggle this!

    // --- UNITY SETUP ---
    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // --- GAME LOGIC ---
    public void CheckWinCondition(PlayerUIManager playerUI, LevelConfig config)
    {
        string finalDestA = GetFinalDestination(playerUI.dropZoneA, config.characterA.startingCity);
        string finalDestB = GetFinalDestination(playerUI.dropZoneB, config.characterB.startingCity);

        if (finalDestA == finalDestB)
        {
            Debug.Log("WIN! Both players met in " + finalDestA);

            if (winTextDisplay != null) winTextDisplay.text = config.winText;

            // NEW: Decide whether to show the normal Win Screen or the End Game Screen!
            if (isFinalLevel && endGameScreen != null)
            {
                endGameScreen.SetActive(true);
            }
            else if (winScreen != null)
            {
                winScreen.SetActive(true);
            }
        }
        else
        {
            Debug.Log("LOSE! Player A is in " + finalDestA + " but Player B is in " + finalDestB);
            if (loseScreen != null) loseScreen.SetActive(true);
        }
    }

    public string GetFinalDestination(Transform dropZone, string defaultCity)
    {
        TimelineColumn timeline = dropZone.GetComponent<TimelineColumn>();
        if (timeline != null)
        {
            List<DraggableFlight> sortedTickets = timeline.GetSortedTickets();
            if (sortedTickets.Count > 0)
            {
                return sortedTickets[sortedTickets.Count - 1].flightData.destination;
            }
        }
        return defaultCity;
    }

    public void SetupLevelPortraits(string nameA, string nameB)
    {
        if (gamePortraitA != null) gamePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Normal");
        if (gamePortraitB != null) gamePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Normal");

        if (winPortraitA != null) winPortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Happy");
        if (winPortraitB != null) winPortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Happy");

        if (losePortraitA != null) losePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Sad");
        if (losePortraitB != null) losePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Sad");
    }

    public void HideAllScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (endGameScreen != null) endGameScreen.SetActive(false); // NEW: Hide this too!
    }
}