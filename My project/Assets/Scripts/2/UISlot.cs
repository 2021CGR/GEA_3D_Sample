using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotType { Inventory, CraftingInput, CraftingOutput }

/// <summary>
/// 아이템을 놓을 수 있는 네모난 칸(슬롯)입니다.
/// </summary>
public class UISlot : MonoBehaviour, IDropHandler
{
    public SlotType slotType = SlotType.Inventory; // 이 슬롯의 역할

    // 슬롯에 아이템이 드롭되었을 때 호출
    public void OnDrop(PointerEventData eventData)
    {
        // 결과창(Output)에는 아이템을 놓을 수 없음
        if (slotType == SlotType.CraftingOutput) return;

        // 드롭된 물체가 UIItem인지 확인
        GameObject dropped = eventData.pointerDrag;
        UIItem item = dropped.GetComponent<UIItem>();

        if (item != null)
        {
            // 만약 이 슬롯에 이미 다른 아이템이 있다면? -> 교체하거나 원래대로 돌려보냄
            if (transform.childCount > 0)
            {
                // 간단하게 구현하기 위해 기존 아이템이 있으면 교체하지 않고 원래 자리로 돌려보냄
                // (고급 기능으로 교체 기능을 넣을 수 있음)
                return;
            }

            // 아이템의 '돌아갈 부모'를 이 슬롯으로 설정
            item.parentAfterDrag = transform;
        }
    }
}