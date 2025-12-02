using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 표시 및 마크 스타일 조합(Crafting)을 관리합니다.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance; // 싱글톤 (어디서든 접근 가능)

    [Header("연결 정보")]
    public Inventory inventory;       // 플레이어 데이터
    public GameObject uiItemPrefab;   // 생성할 아이템 아이콘 프리팹

    [Header("슬롯 부모 연결")]
    public Transform inventorySlotParent; // 10개 슬롯이 있는 패널
    public Transform craftSlot1;          // 조합 재료 슬롯 1
    public Transform craftSlot2;          // 조합 재료 슬롯 2
    public Transform resultSlot;          // 결과 슬롯

    [Header("아이콘 리소스")]
    public Sprite dirtIcon;
    public Sprite ironIcon;
    public Sprite swordIcon;
    // ... 필요한 아이콘 추가

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 인벤토리 데이터를 UI 슬롯에 채워넣음
        RefreshInventoryUI();
    }

    private void Update()
    {
        // I, Tab 키 등으로 인벤토리를 껐다 켰다 하는 로직 추가 가능
    }

    /// <summary>
    /// 인벤토리 데이터를 읽어 UI를 새로고침합니다.
    /// (단, 이미 조합창에 올려둔 아이템은 건드리지 않습니다)
    /// </summary>
    public void RefreshInventoryUI()
    {
        // 1. 인벤토리 슬롯 비우기 (기존 아이템 삭제)
        foreach (Transform slot in inventorySlotParent)
        {
            if (slot.childCount > 0) Destroy(slot.GetChild(0).gameObject);
        }

        // 2. 인벤토리 데이터 불러와서 생성
        // (단순화를 위해, Dictionary를 순회하며 빈 슬롯에 차례대로 넣습니다)
        int slotIndex = 0;
        foreach (var kvp in inventory.items)
        {
            if (kvp.Value <= 0) continue; // 개수 0개면 패스
            if (slotIndex >= inventorySlotParent.childCount) break; // 슬롯 꽉 차면 중단

            // 아이템 생성
            CreateItemInSlot(inventorySlotParent.GetChild(slotIndex), kvp.Key, kvp.Value);
            slotIndex++;
        }
    }

    void CreateItemInSlot(Transform slot, BlockType type, int count)
    {
        GameObject go = Instantiate(uiItemPrefab, slot);
        UIItem uiItem = go.GetComponent<UIItem>();
        uiItem.Initialize(type, count, GetIcon(type));
    }

    /// <summary>
    /// 드래그가 끝날 때마다 호출되어 조합법을 확인합니다.
    /// </summary>
    public void CheckCraftingRecipe()
    {
        // 1. 재료 슬롯에 있는 아이템 확인
        UIItem item1 = craftSlot1.GetComponentInChildren<UIItem>();
        UIItem item2 = craftSlot2.GetComponentInChildren<UIItem>();

        // 2. 결과창 초기화 (기존 결과 삭제)
        if (resultSlot.childCount > 0) Destroy(resultSlot.GetChild(0).gameObject);

        // 3. 재료가 없으면 리턴
        if (item1 == null || item2 == null) return;

        // 4. 조합법 검사: 철 + 철 = 철검
        if (item1.type == BlockType.Iron && item2.type == BlockType.Iron)
        {
            // 결과 슬롯에 '철검' 미리보기 생성
            GameObject result = Instantiate(uiItemPrefab, resultSlot);
            UIItem resultItem = result.GetComponent<UIItem>();
            resultItem.Initialize(BlockType.IronSword, 1, GetIcon(BlockType.IronSword));

            // 결과 아이템에는 클릭 이벤트를 추가하여 실제 획득 로직 연결
            Button btn = result.AddComponent<Button>();
            btn.onClick.AddListener(() => OnClickResult(item1, item2));
        }
    }

    /// <summary>
    /// 결과 아이템을 클릭했을 때 (제작 완료)
    /// </summary>
    public void OnClickResult(UIItem mat1, UIItem mat2)
    {
        // 1. 실제 데이터 반영 (인벤토리에서 재료 소모, 결과 획득)
        // (UIItem은 단순히 비주얼이므로, 실제 데이터인 inventory.Consume 등을 호출해야 함)
        // 여기서는 간단히 UI상에서 처리하고 실제 데이터 동기화는 별도로 맞춤

        Debug.Log("제작 완료: Iron Sword!");

        // 2. 재료 아이템 파괴 (소모됨)
        Destroy(mat1.gameObject);
        Destroy(mat2.gameObject);

        // 3. 결과 아이템을 인벤토리로 이동 (또는 인벤토리 데이터에 추가 후 Refresh)
        // 여기서는 인벤토리 데이터에 추가하고 Refresh 하는 정석적인 방법을 사용
        inventory.Consume(BlockType.Iron, 2);
        inventory.Add(BlockType.IronSword, 1);

        // UI 전체 갱신 (결과물은 인벤토리로 들어감)
        if (resultSlot.childCount > 0) Destroy(resultSlot.GetChild(0).gameObject);
        RefreshInventoryUI();
    }

    Sprite GetIcon(BlockType type)
    {
        // 기존의 아이콘 매핑 로직을 가져오거나 if문 사용
        if (type == BlockType.Dirt) return null; // 흙 아이콘 연결 필요
        if (type == BlockType.Iron) return ironIcon;
        if (type == BlockType.IronSword) return swordIcon;
        return null;
    }
}