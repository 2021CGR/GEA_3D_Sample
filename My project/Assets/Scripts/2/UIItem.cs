using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리/제작 UI에서 표시되는 단일 아이템 셀.
/// - 드래그로 그리드 슬롯 간 이동(간단한 위치 이동)
/// - 우클릭으로 제작 패널에 재료 추가(Shift=5, Ctrl=10)
/// - Initialize로 타입/수량/아이콘 설정
/// </summary>
public class UIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public BlockType type;
    public int count;
    public Transform parentAfterDrag;
    Image image;
    CanvasGroup canvasGroup;

    /// <summary>
    /// 기본 컴포넌트(Image/CanvasGroup) 확보
    /// </summary>
    void Awake()
    {
        image = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// UI 아이템 초기화(아이콘/타입/수량)
    /// </summary>
    public void Initialize(BlockType newType, int newCount, Sprite sprite)
    {
        type = newType;
        count = newCount;
        image.sprite = sprite;
    }

    /// <summary>
    /// 드래그 시작: 부모 분리 및 레이캐스트 비활성
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        image.raycastTarget = false;
    }

    /// <summary>
    /// 드래그 중: 마우스 위치로 이동
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    /// <summary>
    /// 드래그 종료: 부모 복구 및 레시피 재검사
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;

        var crafting = CraftingUI.Instance;
        if (crafting != null) crafting.CheckRecipe();
    }

    /// <summary>
    /// 우클릭: 제작 패널 열림 상태라면 재료 추가
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var panel = CraftingPanel.Instance;
            if (panel != null && panel.IsOpen())
            {
                int addCount = 1;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) addCount = 5;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) addCount = 10;
                panel.AddPlanned(type, addCount);
            }
        }
    }
}
