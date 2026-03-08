using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Player Map Pins")]
    public RectTransform avatarA;
    public RectTransform avatarB;

    [Header("Folders")]
    public Transform mapNodesParent;
    public Transform cloudsParent; // NEW: The folder holding your cloud images!

    public void PlaceAvatarsAtStart(string startCityA, string startCityB)
    {
        Transform nodeA = FindNodeByName(startCityA);
        if (nodeA != null) { avatarA.position = nodeA.position; avatarA.gameObject.SetActive(true); }

        Transform nodeB = FindNodeByName(startCityB);
        if (nodeB != null) { avatarB.position = nodeB.position; avatarB.gameObject.SetActive(true); }
    }

    // --- THE NEW FOG OF WAR LOGIC ---
    public void SetupClouds(List<string> cloudedCities)
    {
        // 1. Turn the correct Cloud Images ON or OFF
        if (cloudsParent != null)
        {
            foreach (Transform cloud in cloudsParent)
            {
                // Does our list contain the exact name of this cloud?
                bool shouldBeClouded = cloudedCities.Contains(cloud.name);
                cloud.gameObject.SetActive(shouldBeClouded);
            }
        }

        // 2. Turn the invisible Nodes ON or OFF
        if (mapNodesParent != null)
        {
            foreach (Transform node in mapNodesParent)
            {
                bool shouldBeClouded = cloudedCities.Contains(node.name);
                // If it IS clouded, we set the node to FALSE (inactive)
                node.gameObject.SetActive(!shouldBeClouded);
            }
        }
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