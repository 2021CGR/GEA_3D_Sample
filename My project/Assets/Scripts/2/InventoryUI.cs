using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용
using TMPro; // TextMeshProUGUI 컴포넌트 사용

/// <summary>
/// Inventory.cs의 데이터를 기반으로 '동적' UI를 표시하고 갱신합니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("연결 필수")]
    // 1. 플레이어에 붙어있는 Inventory.cs
    public Inventory inventory;

    // 2. UI 슬롯들을 담을 부모 오브젝트 (Grid Layout Group이 붙어있음)
    public Transform slotContainer;

    // 3. 복제해서 사용할 UI 슬롯 프리팹 (1개)
    public GameObject slotPrefab;

    [Header("아이콘 매칭")]
    public Sprite dirtIcon;
    public Sprite grassIcon;
    public Sprite waterIcon;
    public Sprite ironIcon;
    public Sprite diamondIcon;

    // 생성된 UI 슬롯 오브젝트들을 관리하기 위한 리스트
    private List<GameObject> activeSlots = new List<GameObject>();
    // 블록 타입별 아이콘을 빠르게 찾기 위한 딕셔너리
    private Dictionary<BlockType, Sprite> iconMap = new Dictionary<BlockType, Sprite>();

    void Start()
    {
        // 1. 아이콘 맵 초기화 (빠른 조회를 위함)
        InitializeIconMap();

        // 2. Inventory.cs의 OnInventoryChanged 이벤트에 UpdateUI 함수를 구독(연결)
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        // 3. 게임 시작 시 UI 초기화
        UpdateUI();
    }

    /// <summary>
    /// 스크립트가 비활성화되거나 파괴될 때 호출됩니다.
    /// </summary>
    void OnDestroy()
    {
        // [중요] 이벤트 구독 해제 (메모리 누수 방지)
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }
    }

    /// <summary>
    /// 인스펙터에서 설정한 아이콘들을 딕셔너리에 저장합니다.
    /// </summary>
    void InitializeIconMap()
    {
        // [수정] 'None' 타입 제거
        iconMap[BlockType.Dirt] = dirtIcon;
        iconMap[BlockType.Grass] = grassIcon;
        iconMap[BlockType.Water] = waterIcon;
        iconMap[BlockType.Iron] = ironIcon;
        iconMap[BlockType.Diamond] = diamondIcon;
    }

    /// <summary>
    /// 인벤토리 UI를 새로고침하는 메인 함수
    /// </summary>
    public void UpdateUI()
    {
        // 1. 기존에 생성했던 모든 슬롯들을 파괴 (Destroy)
        foreach (GameObject slot in activeSlots)
        {
            Destroy(slot);
        }
        activeSlots.Clear(); // 리스트 비우기

        // 2. inventory.items 딕셔너리에 있는 모든 아이템을 순회
        foreach (var item in inventory.items)
        {
            BlockType type = item.Key;   // 아이템 타입 (예: BlockType.Dirt)
            int count = item.Value;  // 아이템 개수 (예: 10)

            // 3. 아이템 개수가 0 이하면 UI에 표시하지 않음
            if (count <= 0) continue;

            // 4. 새로운 슬롯 프리팹을 slotContainer의 자식으로 복제 생성 (Instantiate)
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);

            // 5. 슬롯의 UI 컴포넌트 가져오기
            // (slotPrefab의 구조에 따라 "Icon", "CountText" 이름은 달라질 수 있음)
            Image iconImage = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI countText = newSlot.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

            // 6. UI 데이터 설정
            iconImage.sprite = GetIcon(type);    // 아이콘 이미지 변경
            countText.text = count.ToString(); // 개수 텍스트 변경

            // 7. 관리 리스트에 추가 (나중에 파괴하기 위함)
            activeSlots.Add(newSlot);
        }
    }

    /// <summary>
    /// 아이콘 맵에서 BlockType에 맞는 스프라이트를 찾아 반환합니다.
    /// </summary>
    Sprite GetIcon(BlockType type)
    {
        if (iconMap.TryGetValue(type, out Sprite icon))
        {
            return icon;
        }
        Debug.LogWarning($"'{type}'에 대한 아이콘이 iconMap에 설정되지 않았습니다.");
        return null;
    }
}