using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("연결해야 할 오브젝트")]
    // 1. 껐다 켰다 할 인벤토리 패널 (InventoryPanel)
    public GameObject inventoryPanel;

    // 2. 플레이어의 시점을 담당하는 스크립트 (선택 사항)
    // (예: FirstPersonController, MouseLook 등 카메라 회전 스크립트를 여기에 넣으면 멈춥니다)
    public MonoBehaviour playerCameraScript;

    // 현재 인벤토리가 열려있는지 확인하는 변수
    private bool isInventoryOpen = false;

    void Start()
    {
        // 게임 시작 시 인벤토리는 닫힌 상태로 시작
        isInventoryOpen = false;
        ApplyInventoryState();
    }

    void Update()
    {
        // Tab 키를 누를 때마다 상태를 변경 (토글)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen; // true <-> false 반전
            ApplyInventoryState();
        }
    }

    // 인벤토리 열림/닫힘 상태에 따라 설정을 적용하는 함수
    void ApplyInventoryState()
    {
        // 1. 패널 켜기/끄기
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            // [상태: 인벤토리 열림]
            // 마우스 커서를 보이게 하고 자유롭게 풀어줌
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 카메라 회전 스크립트가 있다면 끄기 (화면 고정)
            if (playerCameraScript != null)
            {
                playerCameraScript.enabled = false;
            }
        }
        else
        {
            // [상태: 인벤토리 닫힘]
            // 마우스 커서를 숨기고 화면 중앙에 고정 (FPS 모드)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 카메라 회전 스크립트가 있다면 다시 켜기 (화면 회전 재개)
            if (playerCameraScript != null)
            {
                playerCameraScript.enabled = true;
            }
        }
    }
}