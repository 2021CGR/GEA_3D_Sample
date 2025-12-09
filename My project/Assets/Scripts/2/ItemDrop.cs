using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드에 떨어진 아이템을 표현합니다.
/// 떨어질 때 물리로 튕기고, 바닥에 안착하면 부유/회전하며 동일 아이템끼리 합쳐집니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class ItemDrop : MonoBehaviour
{
    public BlockType type; // 아이템 종류
    public int count = 1;  // 수량

    [Header("부유 연출 설정")]
    public float floatHeight = 0.05f;     // 상하 바운싱 높이
    public float baseFloatHeight = 0.1f;  // 바닥 위 기본 높이
    public float floatSpeed = 2f;         // 바운싱 속도
    public float rotateSpeed = 50f;       // 회전 속도

    [Header("낙하/그룹화 물리 설정")]
    public float extraGravityForce = 10f; // 낙하 중 추가 중력
    public float groupingRadius = 1.0f;   // 주변 아이템 합치기 반경

    // [중복 방지] 이미 수거된 상태 표시(중복 픽업 방지)
    [HideInInspector] public bool isPickedUp = false;

    // ���� ����
    private Rigidbody rb;
    private Vector3 initialPosition; // 부유 시작 기준 위치
    private bool isFloating = false; // 부유 중인가?
    private bool isMerging = false;  // 합치는 중인가?
    private SphereCollider groupingTrigger; // 주변 합치기 감지 트리거
    private SphereCollider physicsCollider; // 물리 충돌 콜라이더

    // 부유 유지 바닥 체크 타이머(과도한 부유 방지)
    private float groundCheckTimer = 0f;
    private float groundCheckInterval = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        physicsCollider = GetComponent<SphereCollider>();

        // 초기 물리 설정
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 움직임 보간으로 부드럽게
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 고속 충돌 누락 방지

        // 주변 합치기 트리거 추가(코드로 생성)
        groupingTrigger = gameObject.AddComponent<SphereCollider>();
        groupingTrigger.isTrigger = true;
        groupingTrigger.radius = groupingRadius;
        groupingTrigger.enabled = false; // 낙하 중에는 비활성(착지 후 활성화)
    }

    void Update()
    {
        if (isPickedUp) return;

        // 착지 후 부유 연출 처리
        if (isFloating)
        {
            // 1. 상향 회전
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            // 2. 상하 바운싱(Sin 그래프 활용)
            float yBob = ((Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f) * floatHeight;

            Vector3 newPos = initialPosition;
            newPos.y += baseFloatHeight + yBob;
            transform.position = newPos;
        }
    }

    void FixedUpdate()
    {
        if (isPickedUp) return;

        if (isFloating)
        {
            // 부유 유지 여부를 주기적으로 검사(바닥이 사라진 경우 등)
            groundCheckTimer += Time.fixedDeltaTime;
            if (groundCheckTimer >= groundCheckInterval)
            {
                groundCheckTimer = 0f;
                if (!IsGrounded())
                {
                    SwitchToFalling(); // 바닥이 없으면 다시 낙하
                }
            }
        }
        else
        {
            // 낙하 중: 더 빨리 안정되도록 추가 중력 적용
            if (!rb.isKinematic)
            {
                rb.AddForce(Vector3.down * extraGravityForce, ForceMode.Acceleration);
            }

            // 착지 확인(속도가 충분히 낮으면)
            if (IsGrounded() && rb.velocity.magnitude < 0.5f)
            {
                SwitchToFloating();
            }
        }
    }

    // 낙하 -> 부유 전환
    void SwitchToFloating()
    {
        isFloating = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;  // 물리 정지(계산 이동)
        rb.useGravity = false;

        initialPosition = transform.position; // 현재 위치 저장
        groupingTrigger.enabled = true;       // 주변 합치기 감지 활성화
    }

    // 부유 -> 낙하 전환
    void SwitchToFalling()
    {
        isFloating = false;
        rb.isKinematic = false; // ���� �ѱ�
        rb.useGravity = true;
        groupingTrigger.enabled = false;
    }

    // 주변에서 머무는 트리거(합치기 감지)
    private void OnTriggerStay(Collider other)
    {
        if (isMerging || !isFloating || isPickedUp) return;

        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop otherItem = other.GetComponent<ItemDrop>();

            // 같은 종류, 아직 미수거, 자신 아님인지 확인
            if (otherItem != null && !otherItem.isPickedUp && otherItem != this && otherItem.type == this.type)
            {
                // InstanceID가 큰 쪽을 기준으로 합치기(충돌 중복 방지)
                if (otherItem.GetInstanceID() > this.GetInstanceID())
                {
                    MergeInto(otherItem);
                }
            }
        }
    }

    void MergeInto(ItemDrop otherItem)
    {
        isMerging = true;
        otherItem.Combine(this.count); // 수량 이동
        Destroy(this.gameObject);      // 자신 오브젝트 제거
    }

    public void Combine(int amount)
    {
        this.count += amount;
    }

    // 바닥 검사(Raycast)
    private bool IsGrounded()
    {
        float radius = physicsCollider != null ? physicsCollider.radius : 0.25f;
        float checkDist = 0.2f;

        // 부유 높이를 고려해 검사 거리 보정
        if (isFloating) checkDist += baseFloatHeight + floatHeight + 0.1f;

        RaycastHit hit;
        // 아래로 레이캐스트 후 Block 태그 확인
        if (Physics.Raycast(transform.position, Vector3.down, out hit, radius + checkDist))
        {
            if (hit.collider.CompareTag("Block")) return true;
        }
        return false;
    }
}
