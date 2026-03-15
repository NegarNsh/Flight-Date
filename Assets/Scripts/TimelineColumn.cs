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

    [Header("Ticket Sizing")]
    [Tooltip("How much smaller the ticket should be compared to the column width. Try 60 or 80!")]
    public float ticketWidthMargin = 60f;
    public float leftPadding = 30f;  // Pushes the ticket away from the left edge
    public float rightPadding = 10f; // Shrinks the right side so it doesn't hit the scrollbar


    [Header("Internal Ticket Padding")]
    [Tooltip("Pushes the text down from the top edge of the purple ticket")]
    public float contentTopPadding = 20f;
    [Tooltip("Pushes the text in from the left and right edges of the purple ticket")]
    public float contentSidePadding = 10f;



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

    // --- NEW: Snaps tickets to exact times and stretches them perfectly! ---
    // --- THE UNIFIED MATH FIX ---
    public void UpdateTicketsLayout()
    {
        List<DraggableFlight> tickets = GetSortedTickets();
        RectTransform columnRect = GetComponent<RectTransform>();

        foreach (DraggableFlight ticket in tickets)
        {
            if (ticket.flightData == null) continue;

            // 1. Calculate the exact pixel coordinates for Departure (Top) and Arrival (Bottom)
            float yTop = GetYPosition(ticket.flightData.exactDeparture);
            float yBottom = GetYPosition(ticket.flightData.exactArrival);

            // 2. The height is exactly the distance between those two points
            float flightHeight = Mathf.Abs(yTop - yBottom);

            // 3. Shape the main ticket container
            RectTransform ticketRect = ticket.GetComponent<RectTransform>();
            ticketRect.localScale = Vector3.one; // Ensures it doesn't stay giant!

            // ---> NEW: Set Anchors and Pivot to the Top-LEFT so padding works properly <---
            ticketRect.anchorMin = new Vector2(0f, 1f);
            ticketRect.anchorMax = new Vector2(0f, 1f);
            ticketRect.pivot = new Vector2(0f, 1f);

            // ---> NEW: Calculate the true width and apply the Left Padding position <---
            float newWidth = columnRect.rect.width - leftPadding - rightPadding;
            ticketRect.sizeDelta = new Vector2(newWidth, flightHeight);
            ticketRect.anchoredPosition = new Vector2(leftPadding, yTop);

            // 4. FORCE the purple Timeline_Design graphic to stretch perfectly
            if (ticket.timelineDesign != null)
            {
                RectTransform designRect = ticket.timelineDesign.GetComponent<RectTransform>();
                designRect.anchorMin = new Vector2(0, 0);
                designRect.anchorMax = new Vector2(1, 1);
                designRect.offsetMin = Vector2.zero;
                designRect.offsetMax = Vector2.zero;

                // 5. FORCE the Background Image inside it to stretch to the absolute edges
                Transform bgImage = ticket.timelineDesign.transform.Find("Image");
                if (bgImage != null)
                {
                    RectTransform bgRect = bgImage.GetComponent<RectTransform>();
                    bgRect.anchorMin = new Vector2(0, 0);
                    bgRect.anchorMax = new Vector2(1, 1);
                    bgRect.offsetMin = Vector2.zero;
                    bgRect.offsetMax = Vector2.zero;
                }
                // 6. STRETCH THE CONTENT FOLDER, BUT APPLY YOUR PADDING!
                Transform contentFolder = ticket.timelineDesign.transform.Find("ContentFolder");
                if (contentFolder != null)
                {
                    RectTransform contentRect = contentFolder.GetComponent<RectTransform>();

                    // Anchor it to stretch across the whole ticket
                    contentRect.anchorMin = new Vector2(0, 0);
                    contentRect.anchorMax = new Vector2(1, 1);

                    // Apply the padding! 
                    contentRect.offsetMin = new Vector2(contentSidePadding, 10f);
                    contentRect.offsetMax = new Vector2(-contentSidePadding, -contentTopPadding);

                    // 7. ---> THE FIX <--- 
                    // Pin the text INSIDE the ContentFolder (Notice we loop through 'contentFolder' now!)
                    foreach (Transform child in contentFolder)
                    {
                        RectTransform textRect = child.GetComponent<RectTransform>();
                        if (textRect != null)
                        {
                            textRect.anchorMin = new Vector2(0.5f, 1f);
                            textRect.anchorMax = new Vector2(0.5f, 1f);
                        }
                    }
                }
            }
        }
    }
}