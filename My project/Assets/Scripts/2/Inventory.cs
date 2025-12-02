using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 아이템 데이터를 딕셔너리로 관리하며, 변경 사항이 있을 때 이벤트를 발생시킵니다.
/// UI는 이 이벤트를 구독하여 화면을 갱신합니다.
/// </summary>
public class Inventory : MonoBehaviour
{
    // 인벤토리가 변경(추가/삭제)될 때 UI 등에게 알리는 이벤트 (델리게이트)
    public event Action OnInventoryChanged;

    // 아이템 저장소 (Key: 블록타입, Value: 개수)
    public Dictionary<BlockType, int> items = new();

    /// <summary>
    /// 아이템 획득 함수
    /// </summary>
    public void Add(BlockType type, int count = 1)
    {
        // 처음 먹는 아이템이라면 키 생성
        if (!items.ContainsKey(type))
        {
            items[type] = 0;
        }

        items[type] += count;
        Debug.Log($"[Inventory] 획득: {type} (+{count}) | 총: {items[type]}");

        // UI 갱신 요청 (구독자가 있다면 실행)
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 아이템 소모 함수 (건축 등)
    /// </summary>
    /// <returns>성공하면 true, 부족하면 false 반환</returns>
    public bool Consume(BlockType type, int count = 1)
    {
        // 아이템이 있는지, 개수가 충분한지 확인
        if (!items.TryGetValue(type, out var have) || have < count)
        {
            return false; // 실패
        }

        items[type] = have - count;
        Debug.Log($"[Inventory] 사용: {type} (-{count}) | 총: {items[type]}");

        OnInventoryChanged?.Invoke();
        return true; // 성공
    }
}