using UnityEngine;

/// <summary>
/// 플레이어 주변의 아이템을 감지하여 인벤토리에 넣습니다.
/// 플레이어 오브젝트의 자식으로 있고, Trigger Collider가 있어야 합니다.
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

        // 충돌 감지용 트리거 설정 강제
        SphereCollider sc = GetComponent<SphereCollider>();
        if (!sc.isTrigger) sc.isTrigger = true;

        // 이 오브젝트 자체는 물리 힘을 받지 않도록 설정
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // 아이템이 범위 내에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (inventory == null) return;

        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop item = other.GetComponent<ItemDrop>();
            if (item != null)
            {
                // [중요] 이미 누군가 주운 아이템이면 무시 (이중 획득 방지)
                if (item.isPickedUp) return;

                // 획득 처리
                item.isPickedUp = true; // 플래그 세움
                inventory.Add(item.type, item.count); // 인벤토리에 추가
                Destroy(other.gameObject); // 월드에서 삭제
            }
        }
    }
}