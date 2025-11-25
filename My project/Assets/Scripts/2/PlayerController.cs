using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 물리적 이동(걷기, 점프)과 1인칭 시점 제어,
/// 그리고 마우스 커서의 잠금/해제 상태를 관리하는 핵심 컨트롤러입니다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;        // 걷는 속도
    public float jumpPower = 5f;        // 점프력
    public float gravity = -9.81f;      // 중력 가속도

    [Header("시점 설정")]
    public float mouseSensitivity = 3f; // 마우스 감도

    // 내부 변수
    float xRotation = 0f;               // 상하 시점 각도 누적값
    CharacterController controller;     // 유니티 캐릭터 컨트롤러 컴포넌트
    Transform cam;                      // 자식으로 있는 메인 카메라
    Vector3 velocity;                   // 수직 속도 (점프/중력 계산용)
    bool isGrounded;                    // 땅에 닿았는지 여부

    private void Awake()
    {
        // 캐릭터 컨트롤러 컴포넌트 가져오기
        controller = GetComponent<CharacterController>();

        // 자식 오브젝트 중에서 카메라 찾기
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>()?.transform;
        }
    }

    void Start()
    {
        // 게임 시작 시 마우스 커서를 화면 중앙에 고정하고 숨김
        SetCursorLock(true);
    }

    void Update()
    {
        // --- 1. 커서 상태 관리 (UI 및 메뉴) ---

        // ESC 키를 누르면 커서 잠금 해제 (메뉴 등을 위해)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(false);
        }

        // 마우스 왼쪽 클릭 시 (UI 버튼 클릭이 아닐 때만) 다시 게임 모드로 복귀
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            SetCursorLock(true);
        }

        // --- 2. 이동 및 시점 처리 ---

        // 커서가 잠겨 있을 때만(게임 플레이 중일 때만) 조작 가능
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // 혹시라도 마우스가 UI 위에 있다면 조작 차단 (안전장치)
            if (EventSystem.current.IsPointerOverGameObject()) return;

            HandleMove(); // 키보드 이동 처리
            HandleLook(); // 마우스 시점 처리
        }
    }

    /// <summary>
    /// 마우스 커서의 잠금 상태와 가시성을 설정하는 헬퍼 함수
    /// </summary>
    /// <param name="isLocked">true: 잠금(게임중), false: 해제(메뉴)</param>
    void SetCursorLock(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked; // 중앙 고정
            Cursor.visible = false;                   // 숨김
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;   // 자유 이동
            Cursor.visible = true;                    // 보임
        }
    }

    /// <summary>
    /// WASD 이동 및 점프 로직
    /// </summary>
    void HandleMove()
    {
        // 1. 땅에 닿아있는지 확인
        isGrounded = controller.isGrounded;

        // 땅에 있을 때 수직 속도 초기화 (계속 떨어지는 가속도 방지)
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // 2. 키보드 입력 받기 (수평, 수직)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 3. 이동 방향 계산 (플레이어가 보는 방향 기준)
        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 4. 점프 처리
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // 물리 공식: v = sqrt(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
        }

        // 5. 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 마우스 움직임에 따른 시점 회전 로직
    /// </summary>
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 좌우 회전 (플레이어 몸통 전체 회전)
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전 (카메라만 회전)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // 고개를 너무 젖히지 못하게 제한

        if (cam != null)
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}