using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Player Map Pins")]
    public RectTransform avatarA;
    public RectTransform avatarB;

    [Header("The Invisible Nodes Folder")]
    public Transform mapNodesParent;

    // The Level Manager will call this and pass in the starting cities (e.g., "Germany", "France")
    public void PlaceAvatarsAtStart(string startCityA, string startCityB)
    {
        // Place Player A
        Transform nodeA = FindNodeByName(startCityA);
        if (nodeA != null)
        {
            avatarA.position = nodeA.position;
            avatarA.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Could not find a map node named: " + startCityA);
        }

        // Place Player B
        Transform nodeB = FindNodeByName(startCityB);
        if (nodeB != null)
        {
            avatarB.position = nodeB.position;
            avatarB.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Could not find a map node named: " + startCityB);
        }
    }

    // A simple helper function that searches through our MapNodes folder
    private Transform FindNodeByName(string cityName)
    {
        // Loop through all the invisible country boxes we made
        foreach (Transform node in mapNodesParent)
        {
            // If the name matches perfectly (ignoring uppercase/lowercase differences)
            if (node.name.Equals(cityName, System.StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }
        }
        return null; // We didn't find it!
    }
}