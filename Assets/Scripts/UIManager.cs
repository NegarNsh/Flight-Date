using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Visual Prefab")]
    public FlightUICard flightCardPrefab;

    [Header("Scroll Container Link")]
    public Transform scrollContentContainer;

    public void DisplayFlights(List<Flight> flightsToShow)
    {
        // 1. Clear old cards
        foreach (Transform child in scrollContentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Spawn new cards directly into the grid container
        foreach (Flight flight in flightsToShow)
        {
            FlightUICard newCard = Instantiate(flightCardPrefab, scrollContentContainer);
            newCard.Populate(flight);
        }
    }
}