using TMPro; // We need this for crisp UI text!
using UnityEngine;
using UnityEngine.UI;

// This script sits directly on your blue FlightCard_Prefab template.
// It acts as the "waiter" for that single card, filling in the data.
public class FlightUICard : MonoBehaviour
{
    [Header("UI Slots (Drag from prefab children)")]
    // Level Designers link these slots in the inspector, so no transform.Find needed!
    public TextMeshProUGUI originText;
    public TextMeshProUGUI destinationText;
    public TextMeshProUGUI depTimeText;
    public TextMeshProUGUI arrTimeText;
    public TextMeshProUGUI priceText;

    [Header("Optional Styling")]
    public Image priceBackground; // For color coding surges/discounts later!

    // --- THE CORE POPULATION MACHINE ---
    // This is the single function called by the UIManager to fill this specific card.
    public void Populate(Flight data)
    {
        if (data == null) return; // Safety check

        // 1. Fill basic text slots with real mathematical flight data!
        if (originText != null) originText.text = data.origin;
        if (destinationText != null) destinationText.text = data.destination;

        // Use ToString() to convert integer numbers into text.
        if (priceText != null) priceText.text = "$" + data.basePrice.ToString();

        // 2. The Advanced Time Math: Format the DateTime objects beautifully.
        // Format string "d MMM H:mm" looks exactly like image_3.png: "7 Mar 08:00".
        if (depTimeText != null) depTimeText.text = data.exactDeparture.ToString("d MMM H:mm");
        if (arrTimeText != null) arrTimeText.text = data.exactArrival.ToString("d MMM H:mm");

        // 3. Clear descriptions that link back to the CSV logic.


        // NEW: Hand the data over to the drag script!
        GetComponent<DraggableFlight>().flightData = data;

    }

}