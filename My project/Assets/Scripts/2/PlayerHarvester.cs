using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // (원래 있었음)

public class PlayerHarvester : MonoBehaviour
{
    // (변수 선언은 동일)
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;
    public int toolDamage = 1;
    public float hitCooldown = 0.15f;

    private float _nextHitTime;
    private Camera _cam;
    public Inventory inventory;

    // (Awake 함수는 동일)
    void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        if (_cam == null)
        {
            Debug.LogError("PlayerHarvester: 자식 오브젝트에서 카메라를 찾을 수 없습니다.");
        }

        if (inventory == null)
            inventory = GetComponent<Inventory>();
        if (inventory == null)
            inventory = gameObject.AddComponent<Inventory>();
    }

    // [수정] Update 함수에 커서 상태 체크 추가
    void Update()
    {
        // [추가]
        // 1. 커서가 잠겨있지 않은 상태(예: ESC 메뉴)면 채광 안함
        // 2. 마우스가 UI 위에 있어도 채광 안함
        if (Cursor.lockState != CursorLockMode.Locked || EventSystem.current.IsPointerOverGameObject())
        {
            return; // 함수 즉시 종료
        }

        // (기존 채광 로직)
        // 마우스 왼쪽 버튼을 누르고 있고, 쿨다운 시간이 지났다면
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldown; // 다음 발사 시간 갱신

            // 카메라 화면 정중앙에서 레이저 발사
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
            {
                var block = hit.collider.GetComponent<Block>();
                if (block != null)
                {
                    block.Hit(toolDamage, inventory);
                }
            }
        }
    }
}