using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Character A (Left Column)")]
    public Image portraitA;
    public TextMeshProUGUI nameTextA;
    public TextMeshProUGUI moneyTextA;
    public TextMeshProUGUI expenseTextA;
    public Transform dropZoneA;

    [Header("Character B (Right Column)")]
    public Image portraitB;
    public TextMeshProUGUI nameTextB;
    public TextMeshProUGUI moneyTextB;
    public TextMeshProUGUI expenseTextB;
    public Transform dropZoneB;

    [Header("Travel Button Logic")]
    public Button travelButton;               // The clickable part
    public Image travelButtonImage;           // The visual part
    public Sprite activeButtonSprite;         // The lit-up image
    public Sprite inactiveButtonSprite;       // The grayed-out image

    // We need a secret place to store their starting money so we can do the math later!
    private float budgetA;
    private float budgetB;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void SetupLevelCharacters(LevelConfig currentLevelData)
    {
        // 1. Save the budgets
        budgetA = currentLevelData.characterA.startingMoney;
        budgetB = currentLevelData.characterB.startingMoney;

        // 2. Setup Top Bar Character A
        if (portraitA != null) portraitA.sprite = Resources.Load<Sprite>($"Characters/{currentLevelData.characterA.characterName}_Normal");
        if (nameTextA != null) nameTextA.text = currentLevelData.characterA.characterName;
        if (moneyTextA != null) moneyTextA.text = "$" + budgetA.ToString();
        if (expenseTextA != null) expenseTextA.text = "";

        // 3. Setup Top Bar Character B
        if (portraitB != null) portraitB.sprite = Resources.Load<Sprite>($"Characters/{currentLevelData.characterB.characterName}_Normal");
        if (nameTextB != null) nameTextB.text = currentLevelData.characterB.characterName;
        if (moneyTextB != null) moneyTextB.text = "$" + budgetB.ToString();
        if (expenseTextB != null) expenseTextB.text = "";

        // 4. Setup the little Map Avatars!
        if (MapManager.instance != null)
        {
            MapManager.instance.avatarA.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>($"Characters/{currentLevelData.characterA.characterName}_Normal");
            MapManager.instance.avatarB.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>($"Characters/{currentLevelData.characterB.characterName}_Normal");
        }

        // 5. Setup the Happy and Sad faces on the Win/Lose Screens!
        if (GameResultManager.instance != null)
        {
            GameResultManager.instance.SetupLevelPortraits(currentLevelData.characterA.characterName, currentLevelData.characterB.characterName);
        }

        // 6. Force the button to be off when the level first starts!
        UpdateButtonState(false);
    }


    // --- THE MATH MAGIC ---
    public void RecalculateExpenses()
    {
        // 1. Calculate Player A
        float totalCostA = 0f;
        int flightCountA = 0; // Keep track of how many flights are actually here!

        foreach (Transform child in dropZoneA)
        {
            DraggableFlight flight = child.GetComponent<DraggableFlight>();
            if (flight != null)
            {
                totalCostA += flight.flightData.basePrice;
                flightCountA++;
            }
        }

        if (expenseTextA != null)
        {
            if (totalCostA > 0) expenseTextA.text = "-$" + totalCostA.ToString();
            else expenseTextA.text = "";
        }

        // 2. Calculate Player B
        float totalCostB = 0f;
        int flightCountB = 0;

        foreach (Transform child in dropZoneB)
        {
            DraggableFlight flight = child.GetComponent<DraggableFlight>();
            if (flight != null)
            {
                totalCostB += flight.flightData.basePrice;
                flightCountB++;
            }
        }

        if (expenseTextB != null)
        {
            if (totalCostB > 0) expenseTextB.text = "-$" + totalCostB.ToString();
            else expenseTextB.text = "";
        }

        // 3. THE VALIDATION CHECK
        // Are both budgets safe? (Total cost is less than or equal to starting money)
        bool isBudgetSafeA = totalCostA <= budgetA;
        bool isBudgetSafeB = totalCostB <= budgetB;

        // Do they both have at least 1 flight?
        bool hasFlightsA = flightCountA > 0;
        bool hasFlightsB = flightCountB > 0;

        // If ALL of these are true, the button activates. Otherwise, it deactivates.
        if (isBudgetSafeA && isBudgetSafeB && hasFlightsA && hasFlightsB)
        {
            UpdateButtonState(true);
        }
        else
        {
            UpdateButtonState(false);
        }
    }

    // A simple helper function to instantly swap the button visuals and clickability
    private void UpdateButtonState(bool isReady)
    {
        if (travelButton != null) travelButton.interactable = isReady; // Locks/Unlocks clicking
        if (travelButtonImage != null)
        {
            // Swap the picture based on the isReady status
            travelButtonImage.sprite = isReady ? activeButtonSprite : inactiveButtonSprite;
        }
    }
}