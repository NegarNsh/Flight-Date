using TMPro; // NEW: We need this to talk to the Text block!
using UnityEngine;
using System.Collections.Generic;
public class GameResultManager : MonoBehaviour
{
    public static GameResultManager instance;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("UI Text")]
    public TextMeshProUGUI winTextDisplay; // NEW: The physical text box on your screen!

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void CheckWinCondition(PlayerUIManager playerUI, LevelConfig config)
    {
        string finalDestA = GetFinalDestination(playerUI.dropZoneA, config.characterA.startingCity);
        string finalDestB = GetFinalDestination(playerUI.dropZoneB, config.characterB.startingCity);

        if (finalDestA == finalDestB)
        {
            Debug.Log("WIN! Both players met in " + finalDestA);

            // NEW: Change the text before we turn the screen on!
            if (winTextDisplay != null)
            {
                winTextDisplay.text = config.winText;
            }

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

    public void HideAllScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }
}