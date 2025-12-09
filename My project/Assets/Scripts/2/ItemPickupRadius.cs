using UnityEngine;

/// <summary>
/// 플레이어 주변의 아이템을 자동으로 수거하여 인벤토리에 추가합니다.
/// 플레이어 오브젝트의 자식으로 두고, 트리거 콜라이더를 사용합니다.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemPickupRadius : MonoBehaviour
{
    private Inventory inventory;

    void Start()
    {
        // 부모 오브젝트(플레이어)에서 인벤토리 스크립트를 찾습니다.
        inventory = GetComponentInParent<Inventory>();

        // 트리거 콜라이더 설정
        SphereCollider sc = GetComponent<SphereCollider>();
        if (!sc.isTrigger) sc.isTrigger = true;

        // 물리 영향이 없도록 리지드바디 설정
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // 트리거에 진입했을 때
    private void OnTriggerEnter(Collider other)
    {
        if (inventory == null) return;

        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop item = other.GetComponent<ItemDrop>();
            if (item != null)
            {
                // [중복 방지] 이미 수거된 아이템이면 무시
                if (item.isPickedUp) return;

                // 수거 처리
                item.isPickedUp = true;             // 픽업 표시
                inventory.Add(item.type, item.count); // 인벤토리에 추가
                Destroy(other.gameObject);           // 오브젝트 제거
            }
        }
    }
}
