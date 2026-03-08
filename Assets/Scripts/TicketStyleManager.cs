using UnityEngine;
using UnityEngine.UI;

public class TicketStyleManager : MonoBehaviour
{
    [Header("The Ticket Background")]
    public Image backgroundImage; // Drag your Ticket's Image component here!

    [Header("Shop Style (The default)")]
    public Sprite shopSprite; // The jagged ticket edge
    public Color shopColor = new Color(0.9f, 0.8f, 1f); // Light Purple

    [Header("Timeline Styles (When dragged)")]
    public Sprite timelineSprite; // The solid rounded box
    public Color colorPlayerA = new Color(0.3f, 0.1f, 0.5f); // Dark Purple
    public Color colorPlayerB = new Color(0.2f, 0.7f, 0.4f); // Green

    [Header("Error Style")]
    public Color errorColor = new Color(1f, 0.6f, 0.6f); // Red

    void Start()
    {
        SetShopStyle(); // Always start looking like a shop ticket!
    }

    public void SetShopStyle()
    {
        if (backgroundImage != null && shopSprite != null) backgroundImage.sprite = shopSprite;
        if (backgroundImage != null) backgroundImage.color = shopColor;
    }

    public void SetTimelineStyle(TimelineColumn.PlayerSide side, bool isValid)
    {
        if (backgroundImage != null && timelineSprite != null) backgroundImage.sprite = timelineSprite;

        if (backgroundImage != null)
        {
            if (!isValid) backgroundImage.color = errorColor;
            else backgroundImage.color = (side == TimelineColumn.PlayerSide.PlayerA) ? colorPlayerA : colorPlayerB;
        }
    }
}