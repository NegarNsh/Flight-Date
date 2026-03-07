using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager instance;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // The Level Manager will call this when Travel is clicked
    public void CheckWinCondition(PlayerUIManager playerUI, LevelConfig config)
    {
        string finalDestA = GetFinalDestination(playerUI.dropZoneA, config.characterA.startingCity);
        string finalDestB = GetFinalDestination(playerUI.dropZoneB, config.characterB.startingCity);

        if (finalDestA == finalDestB)
        {
            Debug.Log("WIN! Both players met in " + finalDestA);
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

    // Helper to turn off the screens when resetting the level
    public void HideAllScreens()
    {
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
    }
}