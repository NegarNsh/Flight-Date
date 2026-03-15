using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DraggableFlight : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Flight flightData;
    [HideInInspector] public Transform parentAfterDrag;

    private CanvasGroup canvasGroup;
    private Transform mainCanvas;
    private GameObject placeholder;
    private Vector2 originalSize;

    [Header("Ticket Designs")]
    public GameObject shopDesign;
    public GameObject timelineDesign;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GameObject.FindFirstObjectByType<Canvas>().transform;
        originalSize = GetComponent<RectTransform>().sizeDelta;
    }

    private void Start()
    {
        UpdateDesignMode();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySound("TicketPickup");
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

        GetComponent<RectTransform>().sizeDelta = originalSize;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;
        transform.position = eventData.position;

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
        TimelineColumn timeline = parentAfterDrag.GetComponent<TimelineColumn>();

        if (timeline == null)
        {
            // If dropped in the shop, return to normal size
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = originalSize;
        }
        else
        {
            // ---> THE ULTIMATE FIX <---
            // Tell the timeline to update itself from the ticket, so it works even if you drop it on another ticket!
            timeline.Invoke("UpdateTicketsLayout", 0.05f);
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Destroy(placeholder);

        if (PlayerUIManager.instance != null) PlayerUIManager.instance.RecalculateExpenses();
        if (MapManager.instance != null) MapManager.instance.RefreshMap();

        if (AudioManager.instance != null)
        {
            if (timeline != null) AudioManager.instance.PlaySound("TimelineDrop");
            else AudioManager.instance.PlaySound("ShopDrop");
        }

        UpdateDesignMode();
    }

    public void UpdateDesignMode()
    {
        // NO MORE MATH HERE! We just turn the graphics on or off.
        bool isInTimeline = (transform.parent != null && transform.parent.GetComponent<TimelineColumn>() != null);
        if (shopDesign != null) shopDesign.SetActive(!isInTimeline);
        if (timelineDesign != null) timelineDesign.SetActive(isInTimeline);
    }
}