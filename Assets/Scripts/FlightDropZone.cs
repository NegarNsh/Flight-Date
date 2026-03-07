using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlightDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler
{
    public enum ZoneType { PlayerA, PlayerB, Shop }
    public ZoneType myZone;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableFlight draggedFlight = eventData.pointerDrag.GetComponent<DraggableFlight>();

            if (draggedFlight != null)
            {
                if (GetComponent<LayoutGroup>() != null)
                {
                    draggedFlight.parentAfterDrag = this.transform;
                }
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableFlight draggedFlight = eventData.pointerDrag.GetComponent<DraggableFlight>();
            if (draggedFlight != null)
            {
                // THE FIX: We MUST tell the ticket to physically stay here when dropped!
                if (GetComponent<LayoutGroup>() != null)
                {
                    draggedFlight.parentAfterDrag = this.transform;

                    if (myZone != ZoneType.Shop)
                    {
                        Debug.Log(myZone.ToString() + " finalized a flight purchase!");
                    }
                }
            }
        }
    }
}