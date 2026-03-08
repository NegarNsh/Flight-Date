using UnityEngine;
using TMPro; // NEW: We need this to talk to the Text block!

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

    private string GetFinalDestination(Transform dropZone, string defaultCity)
    {
        if (dropZone.childCount == 0) return defaultCity;

        Transform lastCard = dropZone.GetChild(dropZone.childCount - 1);
        DraggableFlight flight = lastCard.GetComponent<DraggableFlight>();

        return flight.flightData.destination;
    }

    public void HideAllScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }
}