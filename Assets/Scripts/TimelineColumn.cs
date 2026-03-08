using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;

public class TimelineColumn : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    public enum PlayerSide { PlayerA, PlayerB }
    public PlayerSide mySide;

    [Header("Timeline Visual Settings")]
    public float pixelsPerHour = 50f;
    public GameObject timeMarkerPrefab;
    public GameObject dayMarkerPrefab;

    [Header("Day Spacing (Edit these!)")]
    public float topPadding = 20f;         // Space above the very first day
    public float spaceBeforeNewDay = 50f;  // Space after 22:00, before the new Day text
    public float spaceAfterDay = 30f;      // Space after the Day text, before the time starts

    public DateTime timelineStart;

    public void GenerateTimeline(List<Flight> levelFlights)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        if (levelFlights.Count == 0) return;

        DateTime earliest = DateTime.MaxValue;
        DateTime latest = DateTime.MinValue;

        foreach (var flight in levelFlights)
        {
            if (flight.exactDeparture < earliest) earliest = flight.exactDeparture;
            if (flight.exactArrival > latest) latest = flight.exactArrival;
        }

        int startHour = earliest.Hour;
        if (startHour % 2 != 0) startHour--;
        timelineStart = earliest.Date.AddHours(startHour - 2);

        int endHour = latest.Hour;
        if (endHour % 2 != 0) endHour++;
        DateTime timelineEnd = latest.Date.AddHours(endHour + 2);

        float totalHours = (float)(timelineEnd - timelineStart).TotalHours;
        int totalDays = (timelineEnd.Date - timelineStart.Date).Days;

        // 1. Calculate the exact total height including all our new spaces!
        float totalHeight = topPadding + spaceAfterDay + (totalHours * pixelsPerHour) + (totalDays * (spaceBeforeNewDay + spaceAfterDay));

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, totalHeight);

        DateTime lastDayDrawn = DateTime.MinValue;

        // 2. Draw the Markers using our Master Formula
        for (int i = 0; i <= totalHours; i += 2)
        {
            DateTime currentTime = timelineStart.AddHours(i);

            // Ask the Master Formula exactly where this time goes!
            float currentY = GetYPosition(currentTime);

            if (currentTime.Date > lastDayDrawn.Date)
            {
                lastDayDrawn = currentTime.Date;

                if (dayMarkerPrefab != null)
                {
                    GameObject dayMarker = Instantiate(dayMarkerPrefab, this.transform);
                    RectTransform dayRT = dayMarker.GetComponent<RectTransform>();

                    // Place the Day text directly between the Before and After space!
                    float dayY = currentY + spaceAfterDay;
                    dayRT.anchoredPosition = new Vector2(0, dayY);

                    TextMeshProUGUI dayText = dayMarker.GetComponentInChildren<TextMeshProUGUI>();
                    if (dayText != null) dayText.text = currentTime.DayOfWeek.ToString();
                }
            }

            GameObject marker = Instantiate(timeMarkerPrefab, this.transform);
            RectTransform markerRT = marker.GetComponent<RectTransform>();
            markerRT.anchoredPosition = new Vector2(0, currentY);

            TextMeshProUGUI timeText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (timeText != null) timeText.text = currentTime.ToString("H:mm");
        }
    }

    // --- THE MASTER FORMULA ---
    // This perfectly calculates the Y coordinate while accounting for the day gaps!
    public float GetYPosition(DateTime targetTime)
    {
        float basePixels = (float)(targetTime - timelineStart).TotalHours * pixelsPerHour;
        int dayCrossings = (targetTime.Date - timelineStart.Date).Days; // How many midnights did we cross?

        float offset = topPadding;
        offset += spaceAfterDay; // Space for the very first day
        offset += dayCrossings * (spaceBeforeNewDay + spaceAfterDay); // Add the gaps for new days!

        return -(basePixels + offset); // Negative because UI goes downwards
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableFlight flight = eventData.pointerDrag.GetComponent<DraggableFlight>();
            if (flight != null) flight.parentAfterDrag = this.transform;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableFlight flight = eventData.pointerDrag.GetComponent<DraggableFlight>();
            if (flight != null)
            {
                flight.parentAfterDrag = this.transform;

            }
        }
    }

    // --- NEW: Gets all dropped flights in chronological order! ---
    public List<Flight> GetSortedFlights()
    {
        List<DraggableFlight> flights = new List<DraggableFlight>();

        // Find every ticket currently dropped in this column
        foreach (Transform child in transform)
        {
            DraggableFlight df = child.GetComponent<DraggableFlight>();
            if (df != null) flights.Add(df);
        }

        // Sort them by their exact departure time!
        flights.Sort((a, b) => a.flightData.exactDeparture.CompareTo(b.flightData.exactDeparture));

        // Extract just the data to send to the map
        List<Flight> sortedData = new List<Flight>();
        foreach (var f in flights) sortedData.Add(f.flightData);

        return sortedData;
    }
}