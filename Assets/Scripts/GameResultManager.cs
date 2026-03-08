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

            // Change the text before we turn the screen on!
            if (winTextDisplay != null) winTextDisplay.text = config.winText;
            if (winScreen != null) winScreen.SetActive(true);
        }
        else
        {
            Debug.Log("LOSE! Player A is in " + finalDestA + " but Player B is in " + finalDestB);

            if (loseScreen != null) loseScreen.SetActive(true);
        }
    }

    public string GetFinalDestination(Transform dropZone, string defaultCity)
    {
        // 1. Grab our smart Timeline script
        TimelineColumn timeline = dropZone.GetComponent<TimelineColumn>();

        if (timeline != null)
        {
            // 2. Ask the timeline for ONLY the valid flight tickets (this ignores the time/day markers!)
            List<DraggableFlight> sortedTickets = timeline.GetSortedTickets();

            // 3. If they placed at least one ticket, their final location is the destination of the very last ticket.
            if (sortedTickets.Count > 0)
            {
                return sortedTickets[sortedTickets.Count - 1].flightData.destination;
            }
        }

        // 4. If there are no tickets at all, they never left their starting city!
        return defaultCity;
    }

    // Call this exactly when the level loads (pass in the names from your LevelData!)
    public void SetupLevelPortraits(string nameA, string nameB)
    {
        // 1. Set the Normal faces for the gameplay screen
        if (gamePortraitA != null) gamePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Normal");
        if (gamePortraitB != null) gamePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Normal");

        // 2. Pre-load the Happy faces into the Win Screen
        if (winPortraitA != null) winPortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Happy");
        if (winPortraitB != null) winPortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Happy");

        // 3. Pre-load the Sad faces into the Lose Screen
        if (losePortraitA != null) losePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Sad");
        if (losePortraitB != null) losePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Sad");
    }


    public void HideAllScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }
}