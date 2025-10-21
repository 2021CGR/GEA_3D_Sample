// 파일 이름: PlayerController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // Cinemachine 관련 기능을 사용하기 위해 필요합니다.
using UnityEngine.UI; // Slider 같은 UI 요소를 사용하기 위해 필요합니다.

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed = 5f; // 플레이어의 기본 이동 속도입니다.
    public float jumpPower = 5f; // 점프 시 위로 솟구치는 힘의 크기입니다.
    public float gravity = -9.81f; // 플레이어에게 적용될 중력 값입니다.
    public float rotationSpeed = 10f; // 플레이어가 카메라 방향으로 회전하는 속도입니다.
    public CinemachineVirtualCamera virtualCam; // 플레이어를 따라다니는 시네머신 가상 카메라입니다.

    [Header("달리기 설정")]
    public float sprintMultiplier = 1.7f; // 달릴 때 기본 속도에 곱해지는 배율입니다.
    public KeyCode sprintKey = KeyCode.LeftShift; // 달리기에 사용할 키입니다.
    public bool allowRightShift = true; // 오른쪽 Shift 키로도 달리기를 허용할지 여부입니다.

    [Header("더블 점프 설정")]
    public int maxJumps = 2; // 최대 점프 횟수입니다. (2로 설정하면 더블 점프가 가능합니다.)
    private int jumpCount = 0; // 현재까지 몇 번 점프했는지 저장하는 변수입니다.

    [Header("더 나은 점프 (Better Jumping)")]
    [Tooltip("떨어질 때 적용할 중력 배율입니다. 높을수록 빨리 떨어집니다.")]
    public float fallMultiplier = 2.5f;
    [Tooltip("점프 키를 짧게 눌렀을 때의 중력 배율입니다. 낮은 점프를 만듭니다.")]
    public float lowJumpMultiplier = 2f;

    [Header("체력 설정")]
    public int maxHP = 10; // 최대 체력입니다.
    public Slider hpSlider; // 체력을 표시할 UI 슬라이더입니다.
    private int currentHP; // 현재 체력을 저장하는 변수입니다.

    [Header("지속 데미지 설정 (용암 바닥 등)")]
    [Tooltip("활성화하면 땅에 닿아있을 때 주기적으로 데미지를 입습니다.")]
    public bool enableGroundDamage = false; // 이 기능을 켤지 끌지 결정하는 스위치입니다.
    [Tooltip("몇 초마다 데미지를 입을지 설정합니다.")]
    public float groundDamageInterval = 1f; // 요청하신대로 1초마다 1씩 닳게 1f로 설정 (조절 가능)
    [Tooltip("지속 데미지로 입을 데미지 양입니다.")]
    public int groundDamageAmount = 1; // 요청하신대로 1씩 닳게 1로 설정 (조절 가능)

    private float groundContactTimer = 0f; // 땅에 닿은 시간을 추적하는 내부 타이머 변수입니다.

    // --- 이하 private 변수들 (스크립트 내부에서만 사용) ---
    private CharacterController controller; // 물리적인 이동과 충돌을 처리하는 컴포E트입니다.
    private CinemachinePOV pov; // 1인칭/3인칭 시점 조작을 위한 시네머신 컴포넌트입니다.
    private Vector3 velocity; // 플레이어의 y축 속도(중력, 점프)를 저장하는 변수입니다.
    public bool isGrounded; // 플레이어가 땅에 닿아있는지 여부를 나타냅니다.
    private bool isSprinting; // 현재 달리고 있는지 여부를 나타냅니다.

    // (나머지 변수들은 코드 하단에서 설명합니다)

    // Start() 함수는 게임 시작 시 첫 프레임이 업데이트되기 전에 한 번만 호출됩니다. (초기화 담당)
    void Start()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트에서 CharacterController 컴포넌트를 찾아와 변수에 저장합니다.
        controller = GetComponent<CharacterController>();
        // virtualCam에서 CinemachinePOV 컴포넌트를 찾아와 변수에 저장합니다.
        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        // (이하 카메라 및 리스폰 관련 초기 설정)
        tps = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (tps != null) baseTPSDist = tps.CameraDistance;
        ftp = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (ftp != null) baseFTPDist = ftp.m_CameraDistance;
        baseFOV = virtualCam != null ? virtualCam.m_Lens.FieldOfView : 60f;
        startPos = respawnPoint ? respawnPoint.position : transform.position;

        // 체력을 최대로 설정하고 UI에 반영합니다.
        currentHP = maxHP;
        if (hpSlider != null) // hpSlider가 할당되었는지 확인
        {
            hpSlider.maxValue = maxHP; // (예: 15)
            hpSlider.value = currentHP;    // (예: 15) -> 이 방식(0~maxHP)으로 통일
        }
    }

    // Update() 함수는 매 프레임마다 호출됩니다. (게임의 핵심 로직 담당)
    void Update()
    {
        // CharacterController가 땅에 닿았는지 확인하여 isGrounded 변수를 업데이트합니다.
        isGrounded = controller.isGrounded;

        // 만약 땅에 닿아 있고, 수직 속도가 0보다 작다면 (안정적으로 착지했다면)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 캐릭터가 땅에 살짝 붙어있도록 y 속도를 낮게 설정합니다. (경사로에서 미끄러짐 방지)
            jumpCount = 0; // 땅에 닿았으므로 점프 횟수를 초기화합니다.
        }

        // [추가된 로직] 지속 데미지 처리
        // '지속 데미지' 기능이 활성화되어 있다면
        if (enableGroundDamage)
        {
            // 현재 땅에 닿아있다면
            if (isGrounded)
            {
                // 땅에 닿아있는 시간을 계속 더합니다.
                groundContactTimer += Time.deltaTime;

                // 누적된 시간이 설정한 데미지 간격(groundDamageInterval)을 넘어서면
                if (groundContactTimer >= groundDamageInterval)
                {
                    TakeDamage(groundDamageAmount); // 설정된 데미지만큼 피해를 줍니다.
                    groundContactTimer = 0f; // 타이머를 0으로 리셋하여 다음 데미지까지 다시 셉니다.
                }
            }
            else
            {
                // 땅에서 떨어졌다면 (점프 또는 추락 중) 타이머를 0으로 리셋합니다.
                groundContactTimer = 0f;
            }
        }

        // Tab 키를 누르면 카메라 시점을 플레이어가 보는 방향으로 초기화합니다.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
            pov.m_VerticalAxis.Value = 0f;
        }

        // 키보드 입력을 받아옵니다. ("Horizontal"은 A,D키, "Vertical"은 W,S키에 해당합니다.)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // (카메라 상태 확인 로직)
        bool isFreeLook =
            lockMoveInFreeLook && (
                (camSwitcher != null && camSwitcher.usingFreeLook) ||
                IsActiveVCamNotMyVirtualCam()
            );

        // 카메라가 바라보는 방향을 기준으로 이동 방향을 계산합니다.
        Vector3 camForward = virtualCam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = virtualCam.transform.right; camRight.y = 0; camRight.Normalize();
        Vector3 move = (camForward * z + camRight * x).normalized;

        // 달리기 키를 누르고 있고, 움직임 입력이 있다면 달리기 상태로 설정합니다.
        bool sprintKeyHeld = Input.GetKey(sprintKey) || (allowRightShift && Input.GetKey(KeyCode.RightShift));
        bool hasMoveInput = move.sqrMagnitude > 0.0001f;
        isSprinting = sprintKeyHeld && hasMoveInput;

        // FreeLook 카메라 상태일 때는 이동과 달리기를 막습니다.
        if (isFreeLook)
        {
            move = Vector3.zero;
            isSprinting = false;
        }

        // 달리기 상태에 따라 최종 이동 속도를 계산합니다.
        float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f);
        // 계산된 방향과 속도로 캐릭터를 이동시킵니다.
        controller.Move(move * currentSpeed * Time.deltaTime);

        // FreeLook 상태가 아닐 때만, 캐릭터가 카메라가 보는 방향을 따라 회전하도록 합니다.
        if (!isFreeLook)
        {
            float cameraYaw = pov.m_HorizontalAxis.Value;
            Quaternion targetRot = Quaternion.Euler(0, cameraYaw, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 스페이스 바를 "누르는 순간"을 감지하고, FreeLook 상태가 아닐 때
        if (!isFreeLook && Input.GetKeyDown(KeyCode.Space))
        {
            // 현재 점프 횟수가 최대 점프 횟수보다 적다면
            if (jumpCount < maxJumps)
            {
                velocity.y = jumpPower; // y축 속도에 점프 힘을 부여합니다.
                jumpCount++; // 점프 횟수를 1 증가시킵니다.
            }
        }

        // [수정된 부분] 더 나은 점프(Better Jumping) 로직
        // 1. 캐릭터가 아래로 떨어지고 있을 때 (y 속도가 음수일 때)
        if (velocity.y < 0)
        {
            velocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        // 2. 캐릭터가 위로 올라가고 있는데, 점프 키에서 손을 뗐을 때
        else if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            velocity.y += gravity * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // 기본 중력은 매 프레임 항상 적용됩니다.
        velocity.y += gravity * Time.deltaTime;
        // 최종적으로 계산된 y축 속도(velocity)를 캐릭터 이동에 반영합니다.
        controller.Move(velocity * Time.deltaTime);

        // (달릴 때 카메라 시야각(FOV)을 변경하는 효과)
        if (sprintCamEffect && virtualCam != null)
        {
            float targetFOV = isSprinting ? sprintFOV : baseFOV;
            virtualCam.m_Lens.FieldOfView =
                Mathf.Lerp(virtualCam.m_Lens.FieldOfView, targetFOV, camLerpSpeed * Time.deltaTime);
            if (tps != null)
            {
                float target = isSprinting ? (baseTPSDist + sprintDistanceAdd) : baseTPSDist;
                tps.CameraDistance = Mathf.Lerp(tps.CameraDistance, target, camLerpSpeed * Time.deltaTime);
            }
            if (ftp != null)
            {
                float target = isSprinting ? (baseFTPDist + sprintDistanceAdd) : baseFTPDist;
                ftp.m_CameraDistance = Mathf.Lerp(ftp.m_CameraDistance, target, camLerpSpeed * Time.deltaTime);
            }
        }
    }

    // 데미지를 받는 함수입니다. (외부 스크립트, 예: 총알, 적의 공격에서 호출됩니다.)
    public void TakeDamage(int damage)
    {
        // 이미 죽었다면 데미지를 받지 않도록 처리
        if (currentHP <= 0) return;

        currentHP -= damage; // 현재 체력에서 데미지만큼 nbsp;니다.

        // 체력이 0보다 작아지지 않도록 보정
        if (currentHP < 0) currentHP = 0;

        if (hpSlider != null) // hpSlider가 할당되었는지 확인
        {
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // (float)currentHP / maxHP; -> 비율(0~1)이 아닌 실제 체력 값(0~maxHP)을 넣어줍니다.
            hpSlider.value = currentHP;
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }

        // 만약 체력이 0 이하가 되었다면
        if (currentHP <= 0)
        {
            Die(); // 죽음 처리 함수를 호출합니다.
        }
    }

    // 죽었을 때 처리할 내용을 담은 함수입니다.
    void Die()
    {
        // [수정된 부분] Die() 함수 로직 변경
        // 플레이어가 죽으면 파괴되는 대신, 리스폰 위치로 이동하고 체력을 회복합니다.

        Debug.Log("플레이어가 사망하여 리스폰합니다.");

        // 스크립트에 이미 구현되어 있는 리스폰 함수를 호출합니다.
        ResetToSpawn();

        // 체력을 최대로 다시 채웁니다.
        currentHP = maxHP;

        if (hpSlider != null) // hpSlider가 할당되었는지 확인
        {
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // (float)currentHP / maxHP; -> 비율(0~1)이 아닌 실제 체력 값(0~maxHP)을 넣어줍니다.
            hpSlider.value = currentHP; // UI도 최대치로 다시 채웁니다.
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
    }

    // (이하 카메라 및 리스폰 관련 헬퍼 함수들은 생략합니다)
    #region Helper Functions
    bool IsActiveVCamNotMyVirtualCam()
    {
        var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain == null || virtualCam == null) return false;
        var active = brain.ActiveVirtualCamera;
        if (active == null) return false;
        return active.VirtualCameraGameObject != virtualCam.gameObject;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider != null && hit.collider.CompareTag(respawnFloorTag))
            ResetToSpawn();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag(respawnFloorTag))
            ResetToSpawn();
    }
    void ResetToSpawn()
    {
        Vector3 target = respawnPoint ? respawnPoint.position : startPos;
        target.y += respawnYOffset;
        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = target;
        controller.enabled = wasEnabled;
        velocity = Vector3.zero;
        jumpCount = 0;
    }
    public void SetCheckpoint(Transform t)
    {
        respawnPoint = t;
        startPos = t.position;
        jumpCount = 0;
    }

    // 카메라 효과 관련 변수들
    [Header("Sprint Camera Feel")]
    public bool sprintCamEffect = true;
    public float sprintFOV = 70f;
    public float camLerpSpeed = 8f;
    public float sprintDistanceAdd = 0.8f;
    private float baseFOV;
    private Cinemachine3rdPersonFollow tps;
    private float baseTPSDist;
    private CinemachineFramingTransposer ftp;
    private float baseFTPDist;
    [Header("Respawn")]
    public Transform respawnPoint;
    public string respawnFloorTag = "RespawnFloor";
    public float respawnYOffset = 0.5f;
    private Vector3 startPos;
    [Header("Camera/FreeLook Lock")]
    public CinemachineSwitcher camSwitcher;
    public bool lockMoveInFreeLook = true;
    #endregion
}