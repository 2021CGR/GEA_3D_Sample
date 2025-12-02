using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 드래그 이벤트를 위해 필수

/// <summary>
/// UI 상에서 드래그가 가능한 아이템 아이콘입니다.
/// </summary>
public class UIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("데이터")]
    public BlockType type; // 어떤 아이템인지
    public int count;      // 몇 개인지 (표시용)

    [HideInInspector] public Transform parentAfterDrag; // 드래그가 끝난 후 돌아갈 부모(슬롯)
    private Image image;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 아이템 정보 초기화 함수
    public void Initialize(BlockType newType, int newCount, Sprite sprite)
    {
        type = newType;
        count = newCount;
        image.sprite = sprite;
    }

    // 1. 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 원래 부모(슬롯)를 기억해둠
        parentAfterDrag = transform.parent;

        // 드래그 중에는 화면 최상단에 그려지도록 부모를 Canvas(또는 최상위 패널)로 잠시 변경
        transform.SetParent(transform.root);
        transform.SetAsLastSibling(); // 맨 위에 그리기

        // 마우스가 아이템을 통과해서 뒤에 있는 슬롯을 감지할 수 있도록 레이캐스트 차단 해제
        canvasGroup.blocksRaycasts = false;
        image.raycastTarget = false;
    }

    // 2. 드래그 중 호출 (매 프레임)
    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 따라다님
        transform.position = eventData.position;
    }

    // 3. 드래그 종료 시 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        // 다시 레이캐스트 감지 켜기
        canvasGroup.blocksRaycasts = true;
        image.raycastTarget = true;

        // 드롭된 곳이 없거나 잘못된 곳이면 원래 슬롯으로 복귀
        transform.SetParent(parentAfterDrag);

        // 위치를 슬롯 정중앙으로 맞춤
        transform.localPosition = Vector3.zero;

        // [중요] 조합창에 변화가 생겼는지 확인하기 위해 매니저에게 알림
        CraftingUI.Instance.CheckCraftingRecipe();
    }
}