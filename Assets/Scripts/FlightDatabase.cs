using System;
using System.Collections.Generic;
using UnityEngine;

// This script *only* cares about loading and filtering the CSV data.
// A level designer just types the CSV name in the inspector.
public class FlightDatabase : MonoBehaviour
{
    // The Singleton pattern allows all other scripts to easily ask for data.
    public static FlightDatabase instance;

    [Header("Data Source")]
    [Tooltip("Drag your CSV text asset file here.")]
    public TextAsset flightDataCSV;

    // We only keep ONE master bucket now to hold *all* loaded flights!
    private List<Flight> allFlights = new List<Flight>();

    void Awake()
    {
        // Simple Singleton setup.
        if (instance == null) { instance = this; } else { Destroy(gameObject); }

        // Automatically load data when the game boots up.
        LoadFlightsFromCSV();
    }

    // (This remains the same robust CSV reading logic from image_1.png).
    // It must use 'int.Parse()' on 'departureDay' and 'basePrice' to avoid previous errors.
    void LoadFlightsFromCSV()
    {
        string[] rows = flightDataCSV.text.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string cleanRow = rows[i].Trim();
            if (string.IsNullOrEmpty(cleanRow)) continue; // Skip empty rows

            string[] columns = cleanRow.Split(',');
            if (columns.Length >= 8)
            {
                Flight newFlight = new Flight();
                newFlight.level = int.Parse(columns[0]);
                newFlight.flightID = columns[1];
                newFlight.origin = columns[2];
                newFlight.destination = columns[3];
                newFlight.basePrice = int.Parse(columns[4]);
                newFlight.departureDay = int.Parse(columns[5]);
                newFlight.startTimeText = columns[6];
                newFlight.durationHours = int.Parse(columns[7]);

                // The calendar math logic.
                DateTime baseDate = new DateTime(2000, 1, newFlight.departureDay);
                newFlight.exactDeparture = baseDate.Add(TimeSpan.Parse(newFlight.startTimeText));
                newFlight.exactArrival = newFlight.exactDeparture.AddHours(newFlight.durationHours);

                allFlights.Add(newFlight);
            }
        }
    }

    // --- THE DYNAMIC SCALABILITY FUNCTION ---
    // This is how we handle 100 levels. Other scripts just call this function and say "Give me flights for level 42!".
    public List<Flight> GetFlightsForLevel(int targetLevel)
    {
        List<Flight> filteredList = new List<Flight>();
        foreach (Flight f in allFlights)
        {
            // If the CSV column says the flight belongs in the level we are looking for.
            if (f.level == targetLevel)
            {
                filteredList.Add(f);
            }
        }
        return filteredList;
    }
}