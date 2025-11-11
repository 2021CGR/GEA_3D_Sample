// 파일 이름: PlayerController.cs
using UnityEngine;
using UnityEngine.EventSystems; // [추가] UI 클릭 감지를 위해 필요

public class PlayerController : MonoBehaviour
{
    // (변수 선언은 이미지와 동일합니다)
    public float moveSpeed = 5f;
    public float jumpPower = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 3f;

    float xRotation = 0f;
    CharacterController controller;
    Transform cam;
    Vector3 velocity;
    bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>()?.transform;
        }
    }

    // [추가] 게임 시작 시 커서를 잠그는 함수 호출
    void Start()
    {
        // 게임 시작 시 커서를 잠금 상태로 설정
        SetCursorLock(true);
    }

    // [수정] Update 함수에 커서 관리 로직 추가
    void Update()
    {
        // --- 1. 커서 상태 관리 ---

        // ESC 키를 누르면 커서 잠금 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(false);
        }

        // 마우스 왼쪽 클릭 시 (단, UI 버튼 클릭이 아닐 때)
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // 커서를 다시 잠금
            SetCursorLock(true);
        }

        // --- 2. 이동 및 시점 변경 ---

        // 커서가 잠겨 있을 때만(Locked) 이동 및 시점 변경 처리
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // (UI 클릭 방지 로직 - PlayerHarvester와 중복될 수 있지만,
            //  혹시 모를 상황을 대비해 두는 것이 안전합니다)
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            HandleMove(); // (이미지 기준 함수명)
            HandleLook(); // (이미지 기준 함수명)
        }
    }

    // [추가] 커서 상태를 설정하는 헬퍼(Helper) 함수
    /// <summary>
    /// 마우스 커서의 잠금 상태와 가시성을 설정합니다.
    /// </summary>
    /// <param name="isLocked">true: 잠금, false: 해제</param>
    void SetCursorLock(bool isLocked)
    {
        if (isLocked)
        {
            // 커서를 화면 중앙에 잠그고
            Cursor.lockState = CursorLockMode.Locked;
            // 커서를 보이지 않게 함
            Cursor.visible = false;
        }
        else
        {
            // 커서 잠금을 해제하고
            Cursor.lockState = CursorLockMode.None;
            // 커서를 다시 보이게 함
            Cursor.visible = true;
        }
    }


    // (HandleMove 함수는 변경 없음)
    void HandleMove()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // (HandleLook 함수는 변경 없음)
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        if (cam != null)
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}