using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        if (inventory == null) inventory = FindObjectOfType<Inventory>();
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
        InitRecipes();
    }

    public void OpenInventoryView()
    {
        if (bigInventoryPanel != null) bigInventoryPanel.SetActive(true);
        if (craftingPanel != null) craftingPanel.SetActive(false);
    }

    public void OpenCraftingView()
    {
        if (bigInventoryPanel != null) bigInventoryPanel.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(true);
    }

    void InitRecipes()
    {
        var ironSword = new Recipe
        {
            ingredients = new Dictionary<BlockType, int> { { BlockType.Iron, 2 } },
            result = BlockType.IronSword
        };
        recipes.Add(ironSword);
    }

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

    bool Match(Dictionary<BlockType, int> bag, Dictionary<BlockType, int> need)
    {
        foreach (var kv in need)
        {
            if (!bag.TryGetValue(kv.Key, out var have)) return false;
            if (have < kv.Value) return false;
        }
        return true;
    }

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
