using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 인벤토리 데이터를 시각화하고, 현재 선택된 슬롯 강조를 처리하는 UI 스크립트입니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("참조 필수")]
    public Inventory inventory;       // 데이터 소스(플레이어 인벤토리)
    public Transform slotContainer;   // 슬롯들을 담는 부모 오브젝트(Content 등)
    public GameObject slotPrefab;     // 슬롯 프리팹(아이콘, 개수 텍스트 포함)

    [Header("아이콘 리소스")]
    public Sprite dirtIcon;
    public Sprite grassIcon;
    public Sprite waterIcon;
    public Sprite ironIcon;
    public Sprite diamondIcon;

    [Header("선택 강조 색상")]
    public Color selectedColor = Color.red;   // 선택된 슬롯 색상
    public Color normalColor = Color.white;   // 기본 슬롯 색상

    // 생성된 슬롯 오브젝트를 추적하기 위한 리스트
    private List<GameObject> activeSlots = new List<GameObject>();
    // 블록 타입과 아이콘을 매핑하기 위한 딕셔너리
    private Dictionary<BlockType, Sprite> iconMap = new Dictionary<BlockType, Sprite>();

    // 현재 선택된 슬롯 인덱스
    private int currentSelectedIndex = -1;

    void Start()
    {
        InitializeIconMap();

        // 인벤토리 변경 이벤트 구독(데이터가 바뀌면 UI 갱신)
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }
        UpdateUI(); // 초기화 후 한 번 갱신
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제(메모리 누수 방지, 필수)
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }
    }

    void InitializeIconMap()
    {
        iconMap[BlockType.Dirt] = dirtIcon;
        iconMap[BlockType.Grass] = grassIcon;
        iconMap[BlockType.Water] = waterIcon;
        iconMap[BlockType.Iron] = ironIcon;
        iconMap[BlockType.Diamond] = diamondIcon;
    }

    /// <summary>
    /// 현재 인벤토리 내용으로 슬롯 UI를 다시 그립니다.
    /// </summary>
    public void UpdateUI()
    {
        if (inventory == null || slotContainer == null || slotPrefab == null) return;
        // 1. 기존 슬롯 모두 제거
        foreach (GameObject slot in activeSlots)
        {
            Destroy(slot);
        }
        activeSlots.Clear();

        // 2. 인벤토리를 순회하며 슬롯 생성
        foreach (var item in inventory.items.OrderBy(kvp => kvp.Key))
        {
            BlockType type = item.Key;
            int count = item.Value;

            if (count <= 0) continue; // 수량 0이면 표시하지 않음

            GameObject newSlot = Instantiate(slotPrefab, slotContainer);

            // 자식 컴포넌트 찾기(이름 기준)
            Image iconImage = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI countText = newSlot.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

            // 아이콘/수량 적용
            iconImage.sprite = GetIcon(type);
            countText.text = count.ToString();

            var uiItem = newSlot.GetComponent<UIItem>() ?? newSlot.AddComponent<UIItem>();
            uiItem.Initialize(type, count, iconImage.sprite);

            activeSlots.Add(newSlot);
        }

        // 3. 선택 강조 상태 갱신(순서 변경 시 다시 반영)
        RefreshSelectionVisual();
    }

    public Sprite GetIcon(BlockType type)
    {
        if (iconMap.TryGetValue(type, out Sprite icon)) return icon;
        return null;
    }

    /// <summary>
    /// 외부(입력 시스템)에서 N번째 슬롯을 선택하도록 요청합니다.
    /// </summary>
    public void SelectSlot(int index)
    {
        currentSelectedIndex = index;
        RefreshSelectionVisual();
    }

    /// <summary>
    /// 선택 상태를 반영해 배경색을 갱신합니다.
    /// </summary>
    void RefreshSelectionVisual()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            Image bgImage = activeSlots[i].GetComponent<Image>();
            if (bgImage != null)
            {
                // 선택된 인덱스면 선택색, 아니면 기본색
                bgImage.color = (i == currentSelectedIndex) ? selectedColor : normalColor;
            }
        }
    }
}
