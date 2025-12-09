using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockType type;
    public int count;
    public Transform parentAfterDrag;
    Image image;
    CanvasGroup canvasGroup;

    void Awake()
    {
        image = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(BlockType newType, int newCount, Sprite sprite)
    {
        type = newType;
        count = newCount;
        image.sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;

        var crafting = CraftingUI.Instance;
        if (crafting != null) crafting.CheckRecipe();
    }
}
