using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 조합(Crafting)을 담당하는 시스템입니다.
/// 현재 테스트 기능: 'C' 키를 누르면 철 2개를 소모해 철검 1개를 만듭니다.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    [Header("연결 필수")]
    public Inventory inventory; // 재료를 확인하고 뺄 인벤토리

    [Header("레시피 설정 (테스트용)")]
    public BlockType ingredient = BlockType.Iron; // 재료: 철
    public int ingredientCount = 2;               // 필요 개수: 2개
    public BlockType resultItem = BlockType.IronSword; // 결과: 철검

    void Start()
    {
        // 인벤토리가 연결 안 되어 있으면 자동으로 찾기
        if (inventory == null)
            inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        // 테스트: C 키를 누르면 제작 시도
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryCraftIronSword();
        }
    }

    /// <summary>
    /// 철검 제작을 시도하는 함수
    /// </summary>
    public void TryCraftIronSword()
    {
        // 1. 인벤토리 확인: 철이 2개 이상 있는지?
        if (inventory.items.ContainsKey(ingredient) && inventory.items[ingredient] >= ingredientCount)
        {
            // 2. 재료 소모 (철 2개 제거)
            inventory.Consume(ingredient, ingredientCount);

            // 3. 결과물 지급 (철검 1개 추가)
            inventory.Add(resultItem, 1);

            Debug.Log("제작 성공! 철검을 획득했습니다.");
        }
        else
        {
            Debug.Log("제작 실패: 재료(철)가 부족합니다.");
        }
    }
}