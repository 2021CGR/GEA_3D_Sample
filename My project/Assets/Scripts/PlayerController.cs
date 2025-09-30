using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    // === Sprint ===
    public float sprintMultiplier = 1.7f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool allowRightShift = true;

    public float jumpPower = 5f;                 // 점프 힘
    public float gravity = -9.81f;
    public CinemachineVirtualCamera virtualCam;
    public float rotationSpeed = 10f;

    private CinemachinePOV pov;
    private CharacterController controller;
    private Vector3 velocity;
    public bool isGrounded;
    private bool isSprinting;

    // === [추가] 더블 점프 ===
    public int maxJumps = 2;                     // 최대 점프 횟수(2면 더블 점프)
    private int jumpCount = 0;                   // 현재 사용한 점프 수

    // === Sprint 카메라 느낌(옵션) ===
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

    // === Respawn 설정 ===
    [Header("Respawn")]
    public Transform respawnPoint;                 // 리스폰 지점(없으면 시작 위치)
    public string respawnFloorTag = "RespawnFloor";// 이 태그 바닥에 닿으면 리스폰
    public float respawnYOffset = 0.5f;
    private Vector3 startPos;

    // === FreeLook 시 입력 잠금 ===
    [Header("Camera/FreeLook Lock")]
    public CinemachineSwitcher camSwitcher;        // 옵션: 할당 시 정확 판정
    public bool lockMoveInFreeLook = true;         // 끄면 기존과 동일

    public int maxHP = 10;
    private int currentHP;

    public Slider hpSlider;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        // 카메라 바디 캐시
        tps = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (tps != null) baseTPSDist = tps.CameraDistance;

        ftp = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (ftp != null) baseFTPDist = ftp.m_CameraDistance;

        baseFOV = virtualCam != null ? virtualCam.m_Lens.FieldOfView : 60f;

        // 시작/리스폰 기준 저장
        startPos = respawnPoint ? respawnPoint.position : transform.position;

        currentHP = maxHP;
        hpSlider.value = 1f;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;        // 지면에 살짝 붙이기
            jumpCount = 0;           // [추가] 땅이면 점프 횟수 리셋
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            pov.m_HorizontalAxis.Value =  transform.eulerAngles.y;
            pov.m_VerticalAxis.Value = 0f;
        }
            

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        // --- FreeLook 활성 여부 ---
        bool isFreeLook =
            lockMoveInFreeLook && (
                (camSwitcher != null && camSwitcher.usingFreeLook) ||
                IsActiveVCamNotMyVirtualCam() // 스위처 없을 때 대안
            );

        // 카메라 기준 이동
        Vector3 camForward = virtualCam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = virtualCam.transform.right; camRight.y = 0; camRight.Normalize();
        Vector3 move = (camForward * z + camRight * x).normalized;

        // 스프린트
        bool sprintKeyHeld = Input.GetKey(sprintKey) || (allowRightShift && Input.GetKey(KeyCode.RightShift));
        bool hasMoveInput = move.sqrMagnitude > 0.0001f;
        isSprinting = sprintKeyHeld && hasMoveInput;

        // FreeLook면 이동/스프린트 잠금
        if (isFreeLook)
        {
            move = Vector3.zero;
            isSprinting = false;
        }

        // 속도 적용
        float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f);
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 회전(FreeLook일 땐 막음)
        if (!isFreeLook)
        {
            float cameraYaw = pov.m_HorizontalAxis.Value;
            Quaternion targetRot = Quaternion.Euler(0, cameraYaw, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // === [수정] 점프: 지상/공중 모두 처리(더블 점프) ===
        if (!isFreeLook && Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)           // 남은 점프가 있으면
            {
                velocity.y = jumpPower;        // 위로 가속도 부여
                jumpCount++;                   // 사용 횟수 증가
            }
        }

        // 중력
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // === 달리기 카메라 효과 ===
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

    // 현재 활성 VCam이 내 virtualCam이 아니면(대개 FreeLook) true
    bool IsActiveVCamNotMyVirtualCam()
    {
        var brain = Camera.main ? Camera.main.GetComponent<CinemachineBrain>() : null;
        if (brain == null || virtualCam == null) return false;
        var active = brain.ActiveVirtualCamera;
        if (active == null) return false;
        return active.VirtualCameraGameObject != virtualCam.gameObject;
    }

    // 비트리거 바닥과 부딪히면 리스폰
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider != null && hit.collider.CompareTag(respawnFloorTag))
            ResetToSpawn();
    }

    // 트리거 바닥도 지원(선택)
    void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag(respawnFloorTag))
            ResetToSpawn();
    }

    // 리스폰 처리
    void ResetToSpawn()
    {
        Vector3 target = respawnPoint ? respawnPoint.position : startPos;
        target.y += respawnYOffset;

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = target;
        controller.enabled = wasEnabled;

        velocity = Vector3.zero;
        jumpCount = 0;                // 리스폰 시 점프 상태 초기화
    }

    // 체크포인트 갱신(옵션)
    public void SetCheckpoint(Transform t)
    {
        respawnPoint = t;
        startPos = t.position;
        jumpCount = 0;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        hpSlider.value = (float)currentHP / maxHP;

        if (currentHP < 0)
        {

            Die();
        }
    }

    void Die()
    {
      Destroy(gameObject);
    }
}






