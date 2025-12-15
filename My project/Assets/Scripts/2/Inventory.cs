using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 간단한 인벤토리 데이터 저장소.
/// - Key: <see cref="BlockType"/> / Value: 보유 개수
/// - 변경(Add/Consume) 시 <see cref="OnInventoryChanged"/> 이벤트를 발행하여
///   UI가 즉시 갱신되도록 한다.
/// </summary>
public class Inventory : MonoBehaviour
{
    // 인벤토리가 변경(추가/삭제)될 때 UI 등에 알리는 이벤트
    public event Action OnInventoryChanged;

    // 아이템 저장소 (Key: 블록타입, Value: 개수)
    public Dictionary<BlockType, int> items = new();

    /// <summary>
    /// 특정 아이템의 현재 보유 개수 조회
    /// </summary>
    public int GetCount(BlockType id)
    {
        items.TryGetValue(id, out var count);
        return count;
    }

    /// <summary>
    /// 아이템 획득(보유량 증가) 및 변경 이벤트 발행
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
    /// 아이템 소모(보유량 감소). 건축/재료 투입 등에서 사용
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
