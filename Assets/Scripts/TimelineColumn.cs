using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;

public class TimelineColumn : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    public enum PlayerSide { PlayerA, PlayerB }
    public PlayerSide mySide;

    [Header("Timeline Settings")]
    public float pixelsPerHour = 100f; // Set this to stretch the UI! 100 pixels = 1 hour.
    public GameObject timeMarkerPrefab;
    public DateTime timelineStart;

    public void GenerateTimeline(List<Flight> levelFlights)
    {
        // 1. Clean up old lines if we restart the level
        foreach (Transform child in transform) Destroy(child.gameObject);

        if (levelFlights.Count == 0) return;

        // 2. Find the absolute earliest and latest times in the level!
        DateTime earliest = DateTime.MaxValue;
        DateTime latest = DateTime.MinValue;

        foreach (var flight in levelFlights)
        {
            if (flight.exactDeparture < earliest) earliest = flight.exactDeparture;
            if (flight.exactArrival > latest) latest = flight.exactArrival;
        }

        // 3. THE MATH FIX: Force the start time to be an EVEN number!
        int startHour = earliest.Hour;
        if (startHour % 2 != 0) startHour--; // If it's an odd hour (like 9), drop it down to 8!

        // Subtract 2 hours for padding so there is empty space above the first ticket
        timelineStart = earliest.Date.AddHours(startHour - 2);

        int endHour = latest.Hour;
        if (endHour % 2 != 0) endHour++; // If odd, push it up to an even number for the end!

        DateTime timelineEnd = latest.Date.AddHours(endHour + 2); // Add padding at the bottom

        float totalHours = (float)(timelineEnd - timelineStart).TotalHours;

        // 4. Resize the Content box so scrolling perfectly fits the whole day!
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, totalHours * pixelsPerHour);

        // 5. Draw the Time Markers every 2 hours!
        for (int i = 0; i <= totalHours; i += 2)
        {
            GameObject marker = Instantiate(timeMarkerPrefab, this.transform);
            RectTransform markerRT = marker.GetComponent<RectTransform>();

            // Move it down the timeline
            markerRT.anchoredPosition = new Vector2(0, -(i * pixelsPerHour));

            // Set the text
            TextMeshProUGUI timeText = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (timeText != null) timeText.text = timelineStart.AddHours(i).ToString("H:mm");
        }
    }
    // --- DROP ZONE LOGIC ---
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
            if (flight != null) flight.parentAfterDrag = this.transform;
        }
    }

    // The Magic Helper Function: Tells the flight exactly where to snap!
    public float GetYPosition(DateTime flightTime)
    {
        return -((float)(flightTime - timelineStart).TotalHours * pixelsPerHour);
    }
}