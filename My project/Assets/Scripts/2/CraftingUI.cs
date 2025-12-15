using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 그리드 기반 간단 제작 UI(샘플).
/// - 드래그로 gridSlots에 아이템을 배치하고 CheckRecipe로 결과를 계산
/// - 결과 슬롯은 버튼으로 동작하여 Craft 실행
/// - 현재 프로젝트에서는 CraftingPanel 기반 제작을 주로 사용하며,
///   이 클래스는 샘플/보조 UI 용도로 유지된다.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance;

    public Inventory inventory;
    public InventoryUI inventoryUI;
    public GameObject uiItemPrefab;
    public Transform[] gridSlots = new Transform[9];
    public Transform resultSlot;
    public GameObject bigInventoryPanel;
    public GameObject craftingPanel;

    struct Recipe
    {
        public Dictionary<BlockType, int> ingredients;
        public BlockType result;
    }

    List<Recipe> recipes = new List<Recipe>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 참조 자동 연결 및 샘플 레시피 초기화
    /// </summary>
    void Start()
    {
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        InitRecipes();
    }

    /// <summary>
    /// 큰 인벤토리 창 열기
    /// </summary>
    public void OpenInventoryView()
    {
        if (bigInventoryPanel != null) bigInventoryPanel.SetActive(true);
        if (craftingPanel != null) craftingPanel.SetActive(false);
    }

    /// <summary>
    /// 제작 창 열기
    /// </summary>
    public void OpenCraftingView()
    {
        if (bigInventoryPanel != null) bigInventoryPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(true);
    }

    /// <summary>
    /// 샘플 레시피 초기화(철검 2xIron)
    /// </summary>
    void InitRecipes()
    {
        var ironSword = new Recipe
        {
            ingredients = new Dictionary<BlockType, int> { { BlockType.Iron, 2 } },
            result = BlockType.IronSword
        };
        recipes.Add(ironSword);
    }

    /// <summary>
    /// gridSlots를 스캔하여 레시피 일치 시 결과 슬롯에 미리보기 버튼 생성
    /// </summary>
    public void CheckRecipe()
    {
        if (resultSlot == null) return;
        foreach (Transform child in resultSlot)
        {
            Destroy(child.gameObject);
        }

        var bag = new Dictionary<BlockType, int>();
        foreach (var slot in gridSlots)
        {
            var uiItem = slot != null ? slot.GetComponentInChildren<UIItem>() : null;
            if (uiItem == null) continue;
            if (!bag.ContainsKey(uiItem.type)) bag[uiItem.type] = 0;
            bag[uiItem.type] += 1;
        }

        foreach (var r in recipes)
        {
            if (Match(bag, r.ingredients))
            {
                var res = Instantiate(uiItemPrefab, resultSlot);
                var iconImage = res.AddComponent<Image>();
                if (inventoryUI != null) iconImage.sprite = inventoryUI.GetIcon(r.result);
                var uiItem = res.AddComponent<UIItem>();
                uiItem.Initialize(r.result, 1, iconImage.sprite);
                var btn = res.AddComponent<Button>();
                btn.onClick.AddListener(() => Craft(r));
                break;
            }
        }
    }

    /// <summary>
    /// 보유(bag)가 요구(need)를 충족하는지 검사
    /// </summary>
    bool Match(Dictionary<BlockType, int> bag, Dictionary<BlockType, int> need)
    {
        foreach (var kv in need)
        {
            if (!bag.TryGetValue(kv.Key, out var have)) return false;
            if (have < kv.Value) return false;
        }
        return true;
    }

    /// <summary>
    /// 결과 아이템 지급 및 그리드/결과 슬롯 초기화
    /// </summary>
    void Craft(Recipe r)
    {
        foreach (var kv in r.ingredients)
        {
            inventory.Consume(kv.Key, kv.Value);
        }
        inventory.Add(r.result, 1);

        foreach (var slot in gridSlots)
        {
            if (slot == null) continue;
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in resultSlot)
        {
            Destroy(child.gameObject);
        }

        if (inventoryUI != null) inventoryUI.UpdateUI();
    }
}
