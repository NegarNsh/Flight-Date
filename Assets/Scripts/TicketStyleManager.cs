using UnityEngine;
using UnityEngine.UI;

public class TicketStyleManager : MonoBehaviour
{
    [Header("The Two Layouts")]
    public GameObject shopDesign;
    public GameObject timelineDesign;

    [Header("Timeline Colors")]
    public Image timelineBackgroundImage; // Drag the background from Timeline_Design here!
    public Color colorPlayerA = new Color(0.3f, 0.1f, 0.5f); // Dark Purple
    public Color colorPlayerB = new Color(0.2f, 0.7f, 0.4f); // Green
    public Color errorColor = new Color(1f, 0.6f, 0.6f);     // Red

    void Start()
    {
        SetShopStyle(); // Always start looking like a shop ticket!
    }

    public void SetShopStyle()
    {
        // Turn ON the shop visuals, turn OFF the timeline visuals
        if (shopDesign != null) shopDesign.SetActive(true);
        if (timelineDesign != null) timelineDesign.SetActive(false);
    }

    public void SetTimelineStyle(TimelineColumn.PlayerSide side, bool isValid)
    {
        // Turn OFF the shop visuals, turn ON the timeline visuals
        if (shopDesign != null) shopDesign.SetActive(false);
        if (timelineDesign != null) timelineDesign.SetActive(true);

        // Color the timeline background!
        if (timelineBackgroundImage != null)
        {
            if (!isValid) timelineBackgroundImage.color = errorColor;
            else timelineBackgroundImage.color = (side == TimelineColumn.PlayerSide.PlayerA) ? colorPlayerA : colorPlayerB;
        }
    }
}