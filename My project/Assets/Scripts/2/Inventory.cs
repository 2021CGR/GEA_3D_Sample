using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // Action을 사용하기 위해 필요

/// <summary>
/// 아이템(블록)을 '동적' 딕셔너리 방식으로 저장하고 관리합니다.
/// </summary>
public class Inventory : MonoBehaviour
{
    // [추가] 인벤토리가 변경될 때마다 호출될 이벤트
    public event Action OnInventoryChanged;

    // [수정] 딕셔너리(Dictionary) 방식으로 되돌림
    public Dictionary<BlockType, int> items = new();

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다. (Block.cs에서 호출)
    /// </summary>
    public void Add(BlockType type, int count = 1)
    {
        // 1. 딕셔너리에 해당 아이템 타입(Key)이 존재하는지 확인
        if (!items.ContainsKey(type))
        {
            // 2. 존재하지 않으면, 이 타입의 아이템을 0개로 새로 추가
            items[type] = 0;
        }

        // 3. 해당 아이템의 개수를 count만큼 증가
        items[type] += count;
        Debug.Log($"[Inventory] + {count} {type} (총 {items[type]})");

        // 4. [중요] 아이템이 추가되었음을 이벤트 구독자(InventoryUI)에게 알림
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 인벤토리에서 아이템을 사용(소모)합니다.
    /// </summary>
    public bool Consume(BlockType type, int count = 1)
    {
        // 1. 아이템이 없거나 개수가 부족하면 false 반환
        if (!items.TryGetValue(type, out var have) || have < count)
        {
            return false;
        }

        // 2. 아이템이 충분하면, 개수 차감
        items[type] = have - count;
        Debug.Log($"[Inventory] - {count} {type} (총 {items[type]})");

        // 3. [중요] 아이템이 소모되었음을 이벤트 구독자(InventoryUI)에게 알림
        OnInventoryChanged?.Invoke();

        return true; // 소모 성공
    }
}