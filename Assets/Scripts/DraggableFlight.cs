using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableFlight : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Flight flightData;
    [HideInInspector] public Transform parentAfterDrag;

    private CanvasGroup canvasGroup;
    private Transform mainCanvas;
    private GameObject placeholder;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GameObject.FindFirstObjectByType<Canvas>().transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // THE LOCKDOWN FIX: Stop immediately if level is inactive
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;

        parentAfterDrag = transform.parent;
        int originalSiblingIndex = transform.GetSiblingIndex();

        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(parentAfterDrag);
        placeholder.transform.SetSiblingIndex(originalSiblingIndex);

        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        RectTransform myRect = GetComponent<RectTransform>();
        placeholderRect.sizeDelta = myRect.sizeDelta;

        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = myRect.sizeDelta.x;
        le.preferredHeight = myRect.sizeDelta.y;

        transform.SetParent(mainCanvas);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // THE LOCKDOWN FIX: Stop the drag from continuing if the level locked!
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;

        transform.position = eventData.position;

        if (parentAfterDrag != null && parentAfterDrag.GetComponent<VerticalLayoutGroup>() != null)
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
                        if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                            newSiblingIndex--;
                        break;
                    }
                }
            }
            placeholder.transform.SetSiblingIndex(newSiblingIndex);
        }
        else if (parentAfterDrag != null)
        {
            if (placeholder.transform.parent != parentAfterDrag)
                placeholder.transform.SetParent(parentAfterDrag);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (LevelManager.instance != null && !LevelManager.instance.isLevelActive) return;

        // THE NUCLEAR SNAP: Adding ', false' forces the card to visually snap 
        // cleanly into the Drop Zone layout instead of floating!
        transform.SetParent(parentAfterDrag, false);
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Destroy(placeholder);

        if (PlayerUIManager.instance != null) PlayerUIManager.instance.RecalculateExpenses();
    }
}