using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 주변의 아이템을 감지하여 픽업합니다.
/// 플레이어의 자식 오브젝트("PickupRadius")에 붙여야 합니다.
/// </summary>
[RequireComponent(typeof(SphereCollider))] // 픽업 범위를 위한 스피어 콜라이더
[RequireComponent(typeof(Rigidbody))]      // 트리거 감지를 위한 리지드바디
public class ItemPickupRadius : MonoBehaviour
{
    // 플레이어의 인벤토리 (부모 오브젝트에 있음)
    private Inventory inventory;

    void Start()
    {
        // 1. 부모 오브젝트(Player)에서 Inventory 컴포넌트를 찾아옴
        inventory = GetComponentInParent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("ItemPickupRadius: 부모 오브젝트에서 Inventory.cs를 찾을 수 없습니다.");
        }

        // 2. 픽업 범위(SphereCollider)가 트리거 모드인지 확인
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (!sphereCollider.isTrigger)
        {
            Debug.LogWarning("ItemPickupRadius: SphereCollider의 'Is Trigger'가 체크되지 않았습니다. 자동으로 설정합니다.");
            sphereCollider.isTrigger = true;
        }

        // 3. 리지드바디 설정 (Kinematic, 중력끄기)
        // (트리거가 안정적으로 작동하기 위해 리지드바디가 필요함)
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // 물리엔진의 힘에 반응하지 않음
        rb.useGravity = false; // 중력 사용 안함
    }

    /// <summary>
    /// 이 트리거 영역 안에 다른 콜라이더가 들어왔을 때 호출됩니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 인벤토리가 없으면 작동 중지
        if (inventory == null) return;

        // 2. 들어온 대상의 태그가 "ItemDrop"인지 확인
        if (other.CompareTag("ItemDrop"))
        {
            // 3. 대상으로부터 ItemDrop 컴포넌트를 가져옴
            ItemDrop item = other.GetComponent<ItemDrop>();
            if (item != null)
            {
                // 4. 인벤토리에 아이템 추가
                // (Add 함수가 OnInventoryChanged 이벤트를 호출하여 UI를 갱신)
                inventory.Add(item.type, item.count);

                // 5. 아이템 드롭 오브젝트를 파괴
                Destroy(other.gameObject);
            }
        }
    }
}