using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LineAnimationTest : MonoBehaviour
{
    [Header("Attach your UI Image here!")]
    public Image artistLineImage;

    [Header("Map Nodes (For Auto-Math!)")]
    public Transform startCityNode; // Drag Mavimia here
    public Transform endCityNode;   // Drag Fokulo/Giervi here

    [Header("Animation Settings")]
    public float drawSpeed = 2.0f;

    void Start()
    {
        if (artistLineImage != null && startCityNode != null && endCityNode != null)
        {
            // 1. Calculate the exact direction automatically!
            SetupFillDirection();

            // 2. Run the animation!
            artistLineImage.fillAmount = 0f;
            StartCoroutine(AnimateLine());
        }
        else
        {
            Debug.LogError("Make sure you slotted the Image and BOTH City Nodes in the Inspector!");
        }
    }

    // --- THE MAGIC MATH ---
    private void SetupFillDirection()
    {
        // Find the difference in distance between the two cities
        float diffX = endCityNode.position.x - startCityNode.position.x;
        float diffY = endCityNode.position.y - startCityNode.position.y;

        // Is the line more wide (Horizontal) or more tall (Vertical)?
        if (Mathf.Abs(diffX) > Mathf.Abs(diffY))
        {
            artistLineImage.fillMethod = Image.FillMethod.Horizontal;

            // If the destination is to the Right (diffX > 0), Fill from the Left (0). Else Fill from Right (1).
            artistLineImage.fillOrigin = (diffX > 0) ? 0 : 1;

            Debug.Log("Auto-Calculated: Horizontal Fill");
        }
        else
        {
            artistLineImage.fillMethod = Image.FillMethod.Vertical;

            // If the destination is Up (diffY > 0), Fill from Bottom (0). Else Fill from Top (1).
            artistLineImage.fillOrigin = (diffY > 0) ? 0 : 1;

            Debug.Log("Auto-Calculated: Vertical Fill");
        }
    }

    private IEnumerator AnimateLine()
    {
        float elapsed = 0f;

        while (elapsed < drawSpeed)
        {
            elapsed += Time.deltaTime;
            artistLineImage.fillAmount = elapsed / drawSpeed;
            yield return null;
        }

        artistLineImage.fillAmount = 1f;
        Debug.Log("Animation Complete!");
    }
}