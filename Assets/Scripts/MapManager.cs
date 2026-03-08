using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("Timelines (Drag from Hierarchy!)")]
    public TimelineColumn timelineA;
    public TimelineColumn timelineB;

    [Header("Player Map Pins")]
    public RectTransform avatarA;
    public RectTransform avatarB;

    [Header("Folders")]
    public Transform mapNodesParent;
    public Transform cloudsParent;
    public Transform flightLinesFolder;

    [Header("Prefabs")]
    public GameObject flightLinePrefab;

    [Header("Player Colors")]
    public Color colorPlayerA = new Color(0.2f, 0.5f, 1f);
    public Color colorPlayerB = new Color(0.7f, 0.2f, 0.8f);
    public Color colorError = new Color(1f, 0.1f, 0.1f);

    private string startCityA;
    private string startCityB;

    private Dictionary<string, Image> activeLines = new Dictionary<string, Image>();

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    public void PlaceAvatarsAtStart(string cityA, string cityB)
    {
        startCityA = cityA;
        startCityB = cityB;

        Transform nodeA = FindNodeByName(cityA);
        if (nodeA != null) { avatarA.position = nodeA.position; avatarA.gameObject.SetActive(true); }

        Transform nodeB = FindNodeByName(cityB);
        if (nodeB != null) { avatarB.position = nodeB.position; avatarB.gameObject.SetActive(true); }
    }

    public void SetupClouds(List<string> cloudedCities)
    {
        if (cloudsParent != null)
        {
            foreach (Transform cloud in cloudsParent) { cloud.gameObject.SetActive(cloudedCities.Contains(cloud.name)); }
        }
        if (mapNodesParent != null)
        {
            foreach (Transform node in mapNodesParent) { node.gameObject.SetActive(!cloudedCities.Contains(node.name)); }
        }
    }

    public void ClearAllLines()
    {
        activeLines.Clear();
        if (flightLinesFolder != null)
        {
            foreach (Transform child in flightLinesFolder) Destroy(child.gameObject);
        }
    }

    public void RefreshMap()
    {
        List<string> requiredLines = new List<string>();

        // Notice we are correctly asking for GetSortedTickets() now!
        if (timelineA != null && !string.IsNullOrEmpty(startCityA))
            ProcessTimeline(timelineA.GetSortedTickets(), startCityA, colorPlayerA, requiredLines, TimelineColumn.PlayerSide.PlayerA);

        if (timelineB != null && !string.IsNullOrEmpty(startCityB))
            ProcessTimeline(timelineB.GetSortedTickets(), startCityB, colorPlayerB, requiredLines, TimelineColumn.PlayerSide.PlayerB);

        List<string> linesToRemove = new List<string>();
        foreach (var memory in activeLines)
        {
            if (!requiredLines.Contains(memory.Key))
            {
                StartCoroutine(AnimateOutAndDestroy(memory.Value));
                linesToRemove.Add(memory.Key);
            }
        }

        foreach (string key in linesToRemove) activeLines.Remove(key);
    }

    private void ProcessTimeline(List<DraggableFlight> sortedTickets, string startingCity, Color playerColor, List<string> requiredLines, TimelineColumn.PlayerSide playerSide)
    {
        string currentCity = startingCity;

        for (int i = 0; i < sortedTickets.Count; i++)
        {
            DraggableFlight ticket = sortedTickets[i];
            Flight flight = ticket.flightData;

            bool isCorrect = flight.origin.Equals(currentCity, System.StringComparison.OrdinalIgnoreCase);
            Color lineTint = isCorrect ? playerColor : colorError;

            // Paint the physical UI Ticket!
            TicketStyleManager styler = ticket.GetComponent<TicketStyleManager>();
            if (styler != null) styler.SetTimelineStyle(playerSide, isCorrect);

            string lineID = $"{flight.origin}_{flight.destination}_{playerSide}_{i}";
            requiredLines.Add(lineID);

            if (!activeLines.ContainsKey(lineID))
            {
                Image newLine = SpawnLineImage(flight.origin, flight.destination, lineTint);
                if (newLine != null)
                {
                    activeLines.Add(lineID, newLine);
                    StartCoroutine(AnimateIn(newLine));
                }
            }
            else
            {
                activeLines[lineID].color = lineTint;
            }

            currentCity = flight.destination;
        }
    }

    private Image SpawnLineImage(string originCity, string destinationCity, Color tintColor)
    {
        if (flightLinePrefab == null) return null;

        Transform startNode = FindNodeByName(originCity);
        Transform endNode = FindNodeByName(destinationCity);
        if (startNode == null || endNode == null) return null;

        Sprite routeSprite = Resources.Load<Sprite>("FlightRoutes/Line_" + originCity + "_" + destinationCity);
        if (routeSprite == null)
        {
            Debug.LogError($"Missing Route Image! You need a file named exactly: Line_{originCity}_{destinationCity}");
            return null;
        }

        GameObject newLine = Instantiate(flightLinePrefab, flightLinesFolder);
        Image lineImage = newLine.GetComponent<Image>();
        lineImage.sprite = routeSprite;
        lineImage.color = tintColor;

        float diffX = endNode.position.x - startNode.position.x;
        float diffY = endNode.position.y - startNode.position.y;

        if (Mathf.Abs(diffX) > Mathf.Abs(diffY))
        {
            lineImage.fillMethod = Image.FillMethod.Horizontal;
            lineImage.fillOrigin = (diffX > 0) ? 0 : 1;
        }
        else
        {
            lineImage.fillMethod = Image.FillMethod.Vertical;
            lineImage.fillOrigin = (diffY > 0) ? 0 : 1;
        }

        return lineImage;
    }

    private IEnumerator AnimateIn(Image img)
    {
        img.fillAmount = 0f;
        float speed = 2.0f;
        while (img != null && img.fillAmount < 1f)
        {
            img.fillAmount += Time.deltaTime * speed;
            yield return null;
        }
        if (img != null) img.fillAmount = 1f;
    }

    private IEnumerator AnimateOutAndDestroy(Image img)
    {
        float speed = 2.5f;
        while (img != null && img.fillAmount > 0f)
        {
            img.fillAmount -= Time.deltaTime * speed;
            yield return null;
        }
        if (img != null) Destroy(img.gameObject);
    }

    private Transform FindNodeByName(string cityName)
    {
        foreach (Transform node in mapNodesParent)
        {
            if (node.name.Equals(cityName, System.StringComparison.OrdinalIgnoreCase)) return node;
        }
        return null;
    }
}