﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 이동(WASD/점프)과 1인칭 카메라 회전(Move/Look),
/// UI 모드 전환(E/ESC) 및 마우스 커서 잠금/해제를 통합 관리하는 컨트롤러.
/// - Cursor.lockState가 Locked일 때만 이동/시점 입력을 처리하여
///   UI가 열렸을 때(커서 해제) 카메라가 움직이지 않도록 보장한다.
/// - E 키로 CraftingPanel을 열고 닫으며, ESC로 커서를 해제한다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("플레이어의 이동 속도입니다.")]
    public float moveSpeed = 5f;

    [Tooltip("점프 시 적용될 힘입니다.")]
    public float jumpPower = 5f;

    [Tooltip("중력 가속도 (보통 -9.81 사용)")]
    public float gravity = -9.81f;

    [Header("시점 설정")]
    [Tooltip("마우스 회전 감도 (값이 클수록 빠르게 회전)")]
    public float mouseSensitivity = 3f;

    // 내부 변수들
    float xRotation = 0f;               // 위아래(X축) 회전 각도 누적값

    CharacterController controller;     // 유니티 내장 이동 컴포넌트

    Transform cam;                      // 자식으로 있는 메인 카메라

    Vector3 velocity;                   // 수직 속도 (점프/중력 계산용 벡터)

    bool isGrounded;                    // 현재 땅에 닿아있는지 여부
    static int _lastEToggleFrame = -1;  // E 토글 다중 처리 방지
    public PlayerAnimation playerAnim;
    float _lastMouseX;

    /// <summary>
    /// 컴포넌트/카메라 참조 확보
    /// </summary>
    private void Awake()
    {
        // 캐릭터 컨트롤러 컴포넌트 가져오기 (없으면 에러 발생 가능하므로 주의)
        controller = GetComponent<CharacterController>();

        // 자식 오브젝트 중에서 카메라 찾기 (계층 구조에서 첫 번째 카메라)
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>()?.transform;
        }
        if (playerAnim == null) playerAnim = GetComponentInChildren<PlayerAnimation>();
    }

    /// <summary>
    /// 시작 시 FPS 모드로 커서 잠금
    /// </summary>
    void Start()
    {
        // 게임 시작 시 마우스 커서를 화면 중앙에 고정하고 숨김 (FPS 모드)
        SetCursorLock(true);
    }

    /// <summary>
    /// 입력 처리 루프:
    /// - ESC: 커서 해제(메뉴/UI 조작)
    /// - 좌클릭: UI가 아니고 패널이 닫혀 있으면 커서 잠금 복귀
    /// - 커서 잠금 상태에서만 이동/시점 처리
    /// - E: 제작 패널 토글 + 커서 상태 동기화
    /// </summary>
    void Update()
    {
        // --- 1. 커서 상태 관리 (UI 및 메뉴) ---

        // ESC 키를 누르면 커서 잠금 해제 (일시정지 메뉴 등을 위해)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLock(false);
        }

        // 마우스 왼쪽 클릭 시, UI 버튼 위가 아니고 제작 패널이 열려 있지 않을 때만 게임 모드로 복귀
        if (Input.GetMouseButtonDown(0)
            && !EventSystem.current.IsPointerOverGameObject()
            && (CraftingPanel.Instance == null || !CraftingPanel.Instance.IsOpen()))
        {
            SetCursorLock(true);
        }

        // --- 2. 이동 및 시점 처리 ---

        // 커서가 잠겨 있을 때만(게임 플레이 중일 때만) 캐릭터 조작 허용
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMove(); // 키보드 이동 및 점프 처리
            HandleLook(); // 마우스 시점 회전 처리
        }

        // 제작 패널 토글: E 키로 열고 닫기
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_lastEToggleFrame == Time.frameCount) return;
            _lastEToggleFrame = Time.frameCount;
            Debug.Log("[플레이어] E 키 입력 감지");
            var panel = CraftingPanel.Instance != null ? CraftingPanel.Instance : FindObjectOfType<CraftingPanel>();
            if (panel != null)
            {
                bool willOpen = !panel.IsOpen();
                Debug.Log($"[플레이어] 패널 열기 여부: {willOpen}, 현재 상태(이전): {panel.IsOpen()}");
                panel.SetOpen(willOpen);
                Debug.Log($"[플레이어] 패널 현재 상태(이후): {panel.IsOpen()}");
                if (willOpen)
                {
                    Debug.Log("[플레이어] 커서 해제 및 카메라/이동 중지");
                    SetCursorLock(false); // 마우스 표시, 카메라/이동 중지
                }
                else
                {
                    Debug.Log("[플레이어] 커서 잠금 및 게임 조작 복귀");
                    SetCursorLock(true);  // 게임 조작 복귀
                }
            }
            else
            {
                Debug.LogWarning("제작 패널을 찾지 못했습니다. 씬에 CraftingPanel이 있는지와 root 연결을 확인하세요.");
            }
        }
    }

    /// <summary>
    /// 마우스 커서의 잠금 상태와 가시성을 설정하는 함수
    /// </summary>
    /// <param name="isLocked">true: 잠금(게임중), false: 해제(메뉴)</param>
    /// <summary>
    /// 커서 잠금/해제 및 표시 상태를 일관되게 설정
    /// </summary>
    void SetCursorLock(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked; // 마우스를 화면 중앙에 고정
            Cursor.visible = false;                   // 마우스 커서 숨김
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;   // 마우스 자유 이동
            Cursor.visible = true;                    // 마우스 커서 보임
        }
    }

    /// <summary>
    /// WASD 이동 및 점프 로직
    /// </summary>
    /// <summary>
    /// WASD 이동/점프/중력 처리
    /// </summary>
    void HandleMove()
    {
        // 1. 땅에 닿아있는지 확인 (CharacterController의 기능 활용)
        isGrounded = controller.isGrounded;

        // 땅에 있을 때 수직 속도 초기화 
        // (0으로 하지 않고 -2로 하는 이유는 땅 감지 판정을 확실하게 하기 위함)
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // 2. 키보드 입력 받기 (-1 ~ 1 사이의 값)
        float h = Input.GetAxis("Horizontal"); // A, D 키
        float v = Input.GetAxis("Vertical");   // W, S 키
        bool runKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 3. 이동 방향 계산 (플레이어가 바라보는 방향 기준)
        // transform.right: 플레이어의 오른쪽 방향, transform.forward: 플레이어의 앞쪽 방향
        Vector3 move = transform.right * h + transform.forward * v;

        // 실제 이동 적용 (방향 * 속도 * 프레임보정)
        controller.Move(move * moveSpeed * Time.deltaTime);
        if (playerAnim != null)
        {
            playerAnim.ApplyLocomotion(h, v, runKey);
            playerAnim.SetGrounded(isGrounded);
        }

        // 4. 점프 처리
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // 물리 공식: 필요한 속도 = sqrt(높이 * -2 * 중력)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
            if (playerAnim != null) playerAnim.TriggerJump();
        }

        // 5. 중력 적용 (매 프레임 아래로 가속)
        velocity.y += gravity * Time.deltaTime;

        // 중력에 의한 수직 이동 적용
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 마우스 움직임에 따른 시점 회전 로직
    /// </summary>
    /// <summary>
    /// 마우스 X/Y 입력을 이용하여 몸통(Yaw)과 카메라(Pitch) 회전
    /// </summary>
    void HandleLook()
    {
        // 마우스 입력값 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 좌우 회전: 플레이어 몸통(Y축)을 전체 회전시킵니다.
        transform.Rotate(Vector3.up * mouseX);
        _lastMouseX = mouseX;

        // 상하 회전: 카메라(X축)만 회전시킵니다.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // 고개를 너무 젖히거나 숙이지 못하게 제한

        // 카메라의 로컬 회전값 적용
        if (cam != null)
            cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerAnim != null) playerAnim.SetHeadTurn(_lastMouseX);
    }
}
