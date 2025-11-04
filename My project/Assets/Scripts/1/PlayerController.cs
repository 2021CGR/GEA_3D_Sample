// 파일 이름: PlayerController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // 시네머신 기능을 사용하기 위해 필요
using UnityEngine.UI; // Slider 같은 UI 요소를 사용하기 위해 필요합니다.

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed = 5f;
    public float jumpPower = 5f;
    public float gravity = -9.81f;
    public float rotationSpeed = 10f;

    [Tooltip("플레이어를 따라다닐 시네머신 가상 카메라. (비워두면 자식에서 자동으로 찾음)")]
    public CinemachineVirtualCamera virtualCam;
    [Tooltip("카메라가 따라다닐 플레이어의 머리(HEAD) Transform입니다.")]
    public Transform headFollowTarget;

    // ... (달리기, 점프, 체력 설정 변수들은 동일) ...
    [Header("달리기 설정")]
    public float sprintMultiplier = 1.7f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool allowRightShift = true;

    [Header("점프 설정 (싱글 점프)")]
    public int maxJumps = 1;
    private int jumpCount = 0;

    [Header("더 나은 점프 (Better Jumping)")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("체력 설정")]
    public int maxHP = 10;
    public Slider hpSlider;
    private int currentHP;


    private CharacterController controller;
    private CinemachinePOV pov;
    private Vector3 velocity;
    public bool isGrounded;
    private bool isSprinting;

    [HideInInspector]
    public bool canMove = true;

    // Start() 함수는 게임 시작 시 첫 프레임이 업데이트되기 전에 한 번만 호출됩니다. (초기화 담당)
    void Start()
    {
        controller = GetComponent<CharacterController>();

        // --- [핵심 수정 부분] ---
        // 1. 인스펙터에서 virtualCam이 할당되지 않았다면(None),
        if (virtualCam == null)
        {
            // 2. 이 오브젝트의 '자식'들 중에서 VCam 컴포넌트를 찾습니다.
            virtualCam = GetComponentInChildren<CinemachineVirtualCamera>();
        }
        // --- [여기까지 수정] ---


        // 3. virtualCam을 찾았거나, 원래 할당되어 있었다면
        if (virtualCam != null)
        {
            // 4. 카메라 컴포넌트들을 가져옵니다.
            pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();
            tps = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (tps != null) baseTPSDist = tps.CameraDistance;
            ftp = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (ftp != null) baseFTPDist = ftp.m_CameraDistance;
            baseFOV = virtualCam != null ? virtualCam.m_Lens.FieldOfView : 60f;

            // 5. (중요) 프리팹 설정이 잘 되었는지 확인합니다.
            if (virtualCam.Follow == null || virtualCam.LookAt == null)
            {
                Debug.LogWarningFormat(this,
                    "PlayerController: VCam '{0}'에 Follow 또는 LookAt 타겟이 설정되지 않았습니다. " +
                    "**Player 프리팹**을 열고 VCam 인스펙터에서 'Head Follow Target'을 연결해주세요.",
                    virtualCam.name);
            }
        }
        else
        {
            // 카메라가 할당되지 않았다면 경고를 띄우고 기본 FOV만 설정
            Debug.LogWarning("PlayerController: virtualCam을 찾을 수 없습니다. 월드 좌표 기준으로 이동합니다.");
            baseFOV = 60f;
        }

        // (리스폰 및 체력 관련 초기 설정)
        startPos = respawnPoint ? respawnPoint.position : transform.position;
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    /// <CodeOmitted /> 
    /// Update() - (이전과 동일, 변경점 없음)
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

        if (!canMove)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && pov != null)
        {
            pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
            pov.m_VerticalAxis.Value = 0f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isFreeLook = false;
        if (virtualCam != null)
        {
            isFreeLook = lockMoveInFreeLook && (
               (camSwitcher != null && camSwitcher.usingFreeLook) ||
               IsActiveVCamNotMyVirtualCam()
           );
        }

        Vector3 move;
        if (virtualCam != null)
        {
            Vector3 camForward = virtualCam.transform.forward; camForward.y = 0; camForward.Normalize();
            Vector3 camRight = virtualCam.transform.right; camRight.y = 0; camRight.Normalize();
            move = (camForward * z + camRight * x).normalized;
        }
        else
        {
            move = new Vector3(x, 0, z).normalized;
        }

        bool sprintKeyHeld = Input.GetKey(sprintKey) || (allowRightShift && Input.GetKey(KeyCode.RightShift));
        bool hasMoveInput = move.sqrMagnitude > 0.0001f;
        isSprinting = sprintKeyHeld && hasMoveInput;

        if (isFreeLook)
        {
            move = Vector3.zero;
            isSprinting = false;
        }

        float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f);
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 회전 로직 (이동과 분리되어 가만히 있어도 회전됨)
        if (!isFreeLook)
        {
            if (virtualCam != null && pov != null) // 1. 카메라가 할당되었다면, 카메라 방향으로 회전
            {
                float cameraYaw = pov.m_HorizontalAxis.Value;
                Quaternion targetRot = Quaternion.Euler(0, cameraYaw, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            else if (move != Vector3.zero) // 2. 카메라가 없다면, 이동 방향으로 회전
            {
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        // 점프 로직
        if (!isFreeLook && Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumps)
            {
                velocity.y = jumpPower;
                jumpCount++;
            }
        }

        // 중력 로직
        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);

        // 카메라 효과 로직
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

    /// <CodeOmitted /> 
    /// TakeDamage(), Die(), Helper Functions - (변경점 없음)
    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어가 사망하여 리스폰합니다.");
        ResetToSpawn();
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }
    }

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