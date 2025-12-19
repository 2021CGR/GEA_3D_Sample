﻿﻿﻿﻿﻿﻿﻿using System.Collections;
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
    // [데이터 지속성] 씬 전환 시에도 인벤토리 유지 (플레이어용)
    // static이므로 씬이 바뀌어도 값이 유지됨.
    // 하지만 에디터에서 플레이 중지 후 다시 시작하면 초기화됨.
    private static Dictionary<BlockType, int> globalItems = new Dictionary<BlockType, int>();
    private static bool hasGlobalData = false;

    // 인벤토리가 변경(추가/삭제)될 때 UI 등에 알리는 이벤트
    public event Action OnInventoryChanged;

    // 아이템 저장소 (Key: 블록타입, Value: 개수)
    public Dictionary<BlockType, int> items = new();

    void Awake()
    {
        // 씬 로드 시(Awake), 글로벌 데이터가 있다면 현재 인벤토리에 덮어씌움
        if (hasGlobalData)
        {
            items = new Dictionary<BlockType, int>(globalItems);
            Debug.Log($"[Inventory] 글로벌 데이터 로드 완료 (아이템 {items.Count}종)");
        }
    }

    /// <summary>
    /// 현재 인벤토리 상태를 글로벌 저장소에 백업
    /// </summary>
    public void SyncToGlobal()
    {
        globalItems = new Dictionary<BlockType, int>(items);
        hasGlobalData = true;
        Debug.Log("[Inventory] 글로벌 데이터 저장됨");
    }

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

        SyncToGlobal(); // 변경 사항 저장
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

        SyncToGlobal(); // 변경 사항 저장
        OnInventoryChanged?.Invoke();
        return true; // 성공
    }
}
