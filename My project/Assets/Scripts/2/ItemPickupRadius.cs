using UnityEngine;

/// <summary>
/// 플레이어 주변의 아이템을 감지하여 인벤토리에 넣습니다.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemPickupRadius : MonoBehaviour
{
    private Inventory inventory;

    void Start()
    {
        inventory = GetComponentInParent<Inventory>();

        // 트리거 설정 강제
        SphereCollider sc = GetComponent<SphereCollider>();
        if (!sc.isTrigger) sc.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (inventory == null) return;

        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop item = other.GetComponent<ItemDrop>();
            if (item != null)
            {
                // [중요] 이미 누군가 주운 아이템이면 무시 (버그 방지)
                if (item.isPickedUp) return;

                // 획득 처리
                item.isPickedUp = true;
                inventory.Add(item.type, item.count);
                Destroy(other.gameObject);
            }
        }
    }
}