using TMPro; 
using UnityEngine;
using UnityEngine.UI;

// This script sits directly on your blue FlightCard_Prefab template.
// It acts as the "waiter" for that single card, filling in the data.
public class FlightUICard : MonoBehaviour
{
    [Header("Shop Text Boxes")]    // Level Designers link these slots in the inspector, so no transform.Find needed!
    public TextMeshProUGUI shopOriginText;
    public TextMeshProUGUI shopDestinationText;
    public TextMeshProUGUI shopDepTimeText;
    public TextMeshProUGUI shopArrTimeText;
    public TextMeshProUGUI shopPriceText;

    [Header("Timeline Text Boxes")]    // Level Designers link these slots in the inspector, so no transform.Find needed!
    public TextMeshProUGUI timelineOriginText;
    public TextMeshProUGUI timelineDestinationText;
    public TextMeshProUGUI timelineDepTimeText;
    public TextMeshProUGUI timelineArrTimeText;
    public TextMeshProUGUI timelinePriceText;

    [Header("Optional Styling")]
    public Image priceBackground; // For color coding surges/discounts later!

    // --- THE CORE POPULATION MACHINE ---
    // This is the single function called by the UIManager to fill this specific card.
    public void Populate(Flight data)
    {
        if (data == null) return; // Safety check

        // 1. Fill basic text slots for the SHOP design!
        if (shopOriginText != null) shopOriginText.text = data.origin;
        if (shopDestinationText != null) shopDestinationText.text = data.destination;
        if (shopPriceText != null) shopPriceText.text = "$" + data.basePrice.ToString();
        if (shopDepTimeText != null) shopDepTimeText.text = data.exactDeparture.ToString("d MMM H:mm");
        if (shopArrTimeText != null) shopArrTimeText.text = data.exactArrival.ToString("d MMM H:mm");

        // 2. Fill basic text slots for the TIMELINE design! (Doing the exact same thing here)
        if (timelineOriginText != null) timelineOriginText.text = data.origin;
        if (timelineDestinationText != null) timelineDestinationText.text = data.destination;
        if (timelinePriceText != null) timelinePriceText.text = "$" + data.basePrice.ToString();
        if (timelineDepTimeText != null) timelineDepTimeText.text = data.exactDeparture.ToString("d MMM H:mm");
        if (timelineArrTimeText != null) timelineArrTimeText.text = data.exactArrival.ToString("d MMM H:mm");

        // NEW: Hand the data over to the drag script!
        GetComponent<DraggableFlight>().flightData = data;
    }
}