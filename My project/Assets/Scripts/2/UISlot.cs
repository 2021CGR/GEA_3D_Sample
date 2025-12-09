using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotType { Inventory, CraftingInput, CraftingOutput }

public class UISlot : MonoBehaviour, IDropHandler
{
    public SlotType slotType = SlotType.Inventory;

    public void OnDrop(PointerEventData eventData)
    {
        if (slotType == SlotType.CraftingOutput) return;

        var dropped = eventData.pointerDrag;
        var item = dropped != null ? dropped.GetComponent<UIItem>() : null;
        if (item == null) return;

        if (transform.childCount > 0) return;

        item.parentAfterDrag = transform;
    }
}
