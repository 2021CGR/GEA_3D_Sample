using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 인벤토리 UI 렌더러:
/// - 인벤토리 데이터(딕셔너리)를 순회해 슬롯 아이템을 동적으로 생성
/// - 선택된 슬롯은 Outline으로 강조 표시
/// - 아이콘 리소스를 BlockType과 매핑하여 표시
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
    public Sprite ironSwordIcon;
    public Sprite axeIcon;
    public Sprite pickaxIcon;

    [Header("선택 강조 색상")]
    public Color selectedColor = Color.red;   // 선택된 슬롯 색상
    public Color normalColor = Color.white;   // 기본 슬롯 색상
    public Color selectedOutlineColor = Color.yellow;
    public float outlineThickness = 3f;

    // 생성된 슬롯 오브젝트를 추적하기 위한 리스트
    private List<GameObject> activeSlots = new List<GameObject>();
    // 블록 타입과 아이콘을 매핑하기 위한 딕셔너리
    private Dictionary<BlockType, Sprite> iconMap = new Dictionary<BlockType, Sprite>();

    // 현재 선택된 슬롯 인덱스
    private int currentSelectedIndex = -1;

    /// <summary>
    /// 아이콘 매핑 초기화 및 레이아웃 보장 후 최초 UI 갱신
    /// </summary>
    void Start()
    {
        InitializeIconMap();

        // 인벤토리 변경 이벤트 구독(데이터가 바뀌면 UI 갱신)
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }
        EnsureLayout();
        UpdateUI(); // 초기화 후 한 번 갱신
    }

    /// <summary>
    /// 이벤트 구독 해제(메모리 누수 방지)
    /// </summary>
    void OnDestroy()
    {
        // 이벤트 구독 해제(메모리 누수 방지, 필수)
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }
    }

    /// <summary>
    /// BlockType별 아이콘 매핑 구성
    /// </summary>
    void InitializeIconMap()
    {
        iconMap[BlockType.Dirt] = dirtIcon;
        iconMap[BlockType.Grass] = grassIcon;
        iconMap[BlockType.Water] = waterIcon;
        iconMap[BlockType.Iron] = ironIcon;
        iconMap[BlockType.Diamond] = diamondIcon;
        iconMap[BlockType.IronSword] = ironSwordIcon;
        iconMap[BlockType.Axe] = axeIcon;
        iconMap[BlockType.Pickax] = pickaxIcon;
    }

    /// <summary>
    /// 현재 인벤토리 내용으로 슬롯 UI를 다시 그립니다.
    /// </summary>
    /// <summary>
    /// 현재 인벤토리 상태를 기준으로 슬롯 UI 전체를 재구성
    /// - 기존 슬롯 제거
    /// - 아이템 수량이 0보다 큰 항목만 표시
    /// - 각 슬롯에 UIItem/Outline 구성
    /// </summary>
    public void UpdateUI()
    {
        if (inventory == null || slotContainer == null || slotPrefab == null) return;
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slotContainer.GetChild(i).gameObject);
        }
        activeSlots.Clear();

        // 2. 인벤토리를 순회하며 슬롯 생성
        foreach (var item in inventory.items.OrderBy(kvp => kvp.Key))
        {
            BlockType type = item.Key;
            int count = item.Value;

            if (count <= 0) continue; // 수량 0이면 표시하지 않음

            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.transform.SetAsLastSibling();
            newSlot.transform.localScale = Vector3.one;

            // 자식 컴포넌트 찾기(이름 기준)
            Image iconImage = newSlot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI countText = newSlot.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

            // 아이콘/수량 적용
            iconImage.sprite = GetIcon(type);
            countText.text = count.ToString();

            var uiItem = newSlot.GetComponent<UIItem>() ?? newSlot.AddComponent<UIItem>();
            uiItem.Initialize(type, count, iconImage.sprite);
            var outline = newSlot.GetComponent<Outline>() ?? newSlot.AddComponent<Outline>();
            outline.effectDistance = new Vector2(outlineThickness, outlineThickness);
            outline.effectColor = selectedOutlineColor;
            outline.enabled = false;

            activeSlots.Add(newSlot);
        }

        // 3. 선택 강조 상태 갱신(순서 변경 시 다시 반영)
        RefreshSelectionVisual();
    }

    /// <summary>
    /// 타입에 대응하는 아이콘 스프라이트를 반환
    /// </summary>
    public Sprite GetIcon(BlockType type)
    {
        if (iconMap.TryGetValue(type, out Sprite icon)) return icon;
        return null;
    }

    /// <summary>
    /// 외부(입력 시스템)에서 N번째 슬롯을 선택하도록 요청합니다.
    /// </summary>
    /// <summary>
    /// 외부에서 특정 인덱스의 슬롯을 선택하도록 요청
    /// </summary>
    public void SelectSlot(int index)
    {
        currentSelectedIndex = index;
        RefreshSelectionVisual();
    }

    /// <summary>
    /// 선택 상태를 반영해 배경색을 갱신합니다.
    /// </summary>
    /// <summary>
    /// 선택된 슬롯에 Outline을 활성화하여 강조 표시
    /// </summary>
    void RefreshSelectionVisual()
    {
        for (int i = 0; i < activeSlots.Count; i++)
        {
            Image bgImage = activeSlots[i].GetComponent<Image>();
            if (bgImage != null)
            {
                // 선택된 인덱스면 선택색, 아니면 기본색
                bgImage.color = normalColor;
            }
            var outline = activeSlots[i].GetComponent<Outline>();
            if (outline != null) outline.enabled = (i == currentSelectedIndex);
        }
    }

    /// <summary>
    /// 슬롯 컨테이너에 HorizontalLayoutGroup/ContentSizeFitter를 보장하여
    /// 슬롯들이 자동으로 우측으로 정렬되도록 설정
    /// </summary>
    void EnsureLayout()
    {
        var h = slotContainer.GetComponent<HorizontalLayoutGroup>();
        if (h == null) h = slotContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6f;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        h.childAlignment = TextAnchor.MiddleLeft;
        var fitter = slotContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = slotContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
}
