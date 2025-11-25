using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인벤토리 데이터를 시각화하고, 현재 선택된 슬롯의 색상을 변경하는 UI 스크립트입니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("연결 필수")]
    public Inventory inventory;       // 데이터 소스
    public Transform slotContainer;   // 슬롯들이 생성될 부모 패널
    public GameObject slotPrefab;     // 슬롯 프리팹

    [Header("아이콘 리소스")]
    public Sprite dirtIcon;
    public Sprite grassIcon;
    public Sprite waterIcon;
    public Sprite ironIcon;
    public Sprite diamondIcon;

    [Header("선택 강조 설정")]
    public Color selectedColor = Color.red;   // 선택 시 배경색
    public Color normalColor = Color.white;   // 평상시 배경색

    private List<GameObject> activeSlots = new List<GameObject>();
    private Dictionary<BlockType, Sprite> iconMap = new Dictionary<BlockType, Sprite>();

    // 현재 선택된 슬롯 번호
    private int currentSelectedIndex = -1;

    void Start()
    {
        InitializeIconMap();

        // 인벤토리 변경 이벤트 구독
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }
        UpdateUI();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
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
    /// 모든 슬롯을 지우고 다시 그리는 함수
    /// </summary>
    public void UpdateUI()
    {
        // 기존 슬롯 삭제
        foreach (GameObject slot in activeSlots)
        {
            Destroy(slot);
        }
        activeSlots.Clear();

        // 인벤토리를 순회하며 슬롯 생성
        foreach (var item in inventory.items)
        {
            BlockType type = item.Key;
            int count = item.Value;

            if (count <= 0) continue; // 0개면 표시 안 함

            GameObject newSlot = Instantiate(slotPrefab, slotContainer);

            // UI 컴포넌트 연결
            Image iconImage = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI countText = newSlot.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

            iconImage.sprite = GetIcon(type);
            countText.text = count.ToString();

            activeSlots.Add(newSlot);
        }

        // 재생성 후 선택 상태 다시 적용
        RefreshSelectionVisual();
    }

    Sprite GetIcon(BlockType type)
    {
        if (iconMap.TryGetValue(type, out Sprite icon)) return icon;
        return null;
    }

    /// <summary>
    /// 외부에서 "N번째 슬롯 선택해줘"라고 요청하는 함수
    /// </summary>
    public void SelectSlot(int index)
    {
        currentSelectedIndex = index;
        RefreshSelectionVisual();
    }

    /// <summary>
    /// 실제 배경색을 변경하여 선택됨을 표시하는 함수
    /// </summary>
    void RefreshSelectionVisual()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            Image bgImage = activeSlots[i].GetComponent<Image>();
            if (bgImage != null)
            {
                // 인덱스가 일치하면 강조색, 아니면 일반색
                bgImage.color = (i == currentSelectedIndex) ? selectedColor : normalColor;
            }
        }
    }
}