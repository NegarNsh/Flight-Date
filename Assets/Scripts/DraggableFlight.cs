using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System; // Needed for DateTime

public class DraggableFlight : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Flight flightData;
    [HideInInspector] public Transform parentAfterDrag;

    private CanvasGroup canvasGroup;
    private Transform mainCanvas;
    private GameObject placeholder;

    private Vector2 originalSize; // NEW: We must remember the shop size!

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GameObject.FindFirstObjectByType<Canvas>().transform;
        originalSize = GetComponent<RectTransform>().sizeDelta;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;

        parentAfterDrag = transform.parent;

        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(parentAfterDrag);
        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        placeholderRect.sizeDelta = originalSize;

        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = originalSize.x;
        le.preferredHeight = originalSize.y;

        transform.SetParent(mainCanvas);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        // Return to normal size while dragging!
        GetComponent<RectTransform>().sizeDelta = originalSize;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;
        transform.position = eventData.position;

        // Only do the sibling index swapping if we are hovering over the SHOP!
        if (parentAfterDrag != null && parentAfterDrag.GetComponent<FlightDropZone>() != null)
        {
            if (placeholder.transform.parent != parentAfterDrag)
                placeholder.transform.SetParent(parentAfterDrag);

            int newSiblingIndex = parentAfterDrag.childCount;
            for (int i = 0; i < parentAfterDrag.childCount; i++)
            {
                Transform child = parentAfterDrag.GetChild(i);
                if (child != this.transform && child != placeholder.transform)
                {
                    if (eventData.position.y > child.position.y)
                    {
                        newSiblingIndex = i;
                        if (placeholder.transform.GetSiblingIndex() < newSiblingIndex) newSiblingIndex--;
                        break;
                    }
                }
            }
            placeholder.transform.SetSiblingIndex(newSiblingIndex);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;

        transform.SetParent(parentAfterDrag, false);

        // Check if we dropped on a Timeline!
        // Check if we dropped on a Timeline!
        TimelineColumn timeline = parentAfterDrag.GetComponent<TimelineColumn>();
        if (timeline != null)
        {
            // THE SHAPE SHIFT!
            DateTime start = flightData.exactDeparture;
            DateTime end = flightData.exactArrival;

            // Ask the timeline exactly where the top and bottom of the ticket belong!
            float yPosStart = timeline.GetYPosition(start);
            float yPosEnd = timeline.GetYPosition(end);

            // The absolute difference between the start and end is the true height!
            // This seamlessly stretches the ticket over the Day text if it crosses midnight!
            float newHeight = Mathf.Abs(yPosEnd - yPosStart);

            RectTransform rt = GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);

            rt.sizeDelta = new Vector2(originalSize.x, newHeight);
            rt.anchoredPosition = new Vector2(0, yPosStart);
        }
        else
        {
            // We dropped back in the shop! Snap to the placeholder and restore anchors.
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = originalSize;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Destroy(placeholder);

        if (PlayerUIManager.instance != null) PlayerUIManager.instance.RecalculateExpenses();

        // Update the map to perfectly match wherever this ticket ended up!
        if (MapManager.instance != null)
        {
            MapManager.instance.RefreshMap();
        }

    }
}