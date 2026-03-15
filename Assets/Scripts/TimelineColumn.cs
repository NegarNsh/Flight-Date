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
            // This forces the background line to the very BACK of the folder, acting like wallpaper!
            marker.transform.SetAsFirstSibling();
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

                // NEW: Wait for the frame to end, then snap everything into place!
                Invoke("UpdateTicketsLayout", 0.05f);
            }
        }
    }

    // --- NEW: Gets tickets in chronological order AND physically sorts the UI! ---
    // --- NEW: Gets tickets in chronological order AND physically sorts the UI! ---
    public List<DraggableFlight> GetSortedTickets()
    {
        List<DraggableFlight> tickets = new List<DraggableFlight>();

        // Find every physical ticket currently dropped in this column
        foreach (Transform child in transform)
        {
            DraggableFlight df = child.GetComponent<DraggableFlight>();
            if (df != null) tickets.Add(df);
        }

        // 1. Sort the tickets mathematically by their exact departure time!
        tickets.Sort((a, b) => a.flightData.exactDeparture.CompareTo(b.flightData.exactDeparture));

        // 2. THE FIX: Force the UI boxes to physically stack in that exact chronological order, 
        // BUT start stacking them at the END of the list so they sit on top of the black lines!
        int totalChildren = transform.childCount;
        for (int i = 0; i < tickets.Count; i++)
        {
            // We push them to the very end of the line, keeping their order intact!
            tickets[i].transform.SetSiblingIndex((totalChildren - tickets.Count) + i);
        }

        return tickets;
    }

    // --- NEW: Snaps tickets to exact times and stretches them! ---
    public void UpdateTicketsLayout()
    {
        List<DraggableFlight> tickets = GetSortedTickets();

        foreach (DraggableFlight ticket in tickets)
        {
            if (ticket.flightData == null) continue;

            RectTransform ticketRect = ticket.GetComponent<RectTransform>();
            RectTransform columnRect = GetComponent<RectTransform>();

            // 1. STRETCH: Calculate height based on duration
            float flightHours = (float)(ticket.flightData.exactArrival - ticket.flightData.exactDeparture).TotalHours;
            float newHeight = flightHours * pixelsPerHour;

            // Set new width and height
            ticketRect.sizeDelta = new Vector2(columnRect.rect.width, newHeight);

            // 2. POSITION: Find the exact Y coordinate based on Departure Time!
            float exactYPos = GetYPosition(ticket.flightData.exactDeparture);

            // Snap it into place! (X is 0 to center it, Y is our exact calculation)
            ticketRect.anchoredPosition = new Vector2(0, exactYPos);
        }
    }
}