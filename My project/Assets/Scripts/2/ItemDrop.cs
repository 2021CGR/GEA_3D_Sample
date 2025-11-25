using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 드롭된 아이템의 물리 동작을 제어합니다.
/// 떨어질 때는 물리 엔진을 사용하고, 착지하면 둥둥 뜨는 애니메이션으로 전환됩니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class ItemDrop : MonoBehaviour
{
    public BlockType type;
    public int count = 1;

    [Header("둥둥 뜨기 설정")]
    public float floatHeight = 0.05f;     // 위아래 흔들림 폭 (작게 설정됨)
    public float baseFloatHeight = 0.1f;  // 바닥에서 띄우는 기본 높이
    public float floatSpeed = 2f;         // 흔들리는 속도
    public float rotateSpeed = 50f;       // 회전 속도

    [Header("낙하 및 감지 설정")]
    public float groundCheckDistance = 0.4f;
    public float extraGravityForce = 10f; // 빨리 떨어지게 하는 추가 중력

    [Header("아이템 합치기")]
    public float groupingRadius = 1.0f;

    // [중요] 이미 획득된 아이템인지 표시 (중복 획득 버그 방지)
    [HideInInspector] public bool isPickedUp = false;

    // 내부 변수
    private Rigidbody rb;
    private Vector3 initialPosition; // 둥둥 뜨기 시작한 기준 좌표
    private bool isFloating = false; // 현재 둥둥 뜨는 중인가?
    private bool isMerging = false;  // 합쳐지는 중인가?

    private SphereCollider groupingTrigger; // 주변 아이템 감지용
    private SphereCollider physicsCollider; // 실제 물리 충돌용

    // 최적화를 위한 타이머
    private float groundCheckTimer = 0f;
    private float groundCheckInterval = 0.2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        physicsCollider = GetComponent<SphereCollider>();

        // 초기 물리 설정 (떨어지는 상태)
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 부드러운 움직임
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 통과 방지

        // 합치기 감지용 트리거 생성
        groupingTrigger = gameObject.AddComponent<SphereCollider>();
        groupingTrigger.isTrigger = true;
        groupingTrigger.radius = groupingRadius;
        groupingTrigger.enabled = false; // 떨어지는 동안에는 끔
    }

    // 시각적 연산 (둥둥 뜨기, 회전) -> Update
    void Update()
    {
        if (isPickedUp) return;

        if (isFloating)
        {
            // 1. 제자리 회전
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            // 2. 위아래 둥둥 (Sin 그래프)
            float yBob = ((Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f) * floatHeight;

            Vector3 newPos = initialPosition;
            newPos.y += baseFloatHeight + yBob;
            transform.position = newPos;
        }
    }

    // 물리 연산 (낙하, 땅 체크) -> FixedUpdate
    void FixedUpdate()
    {
        if (isPickedUp) return;

        if (isFloating)
        {
            // 둥둥 떠있을 때도 가끔 땅이 사라졌는지 확인
            groundCheckTimer += Time.fixedDeltaTime;
            if (groundCheckTimer >= groundCheckInterval)
            {
                groundCheckTimer = 0f;
                if (!IsGrounded())
                {
                    SwitchToFalling(); // 땅 없으면 다시 떨어짐
                }
            }
        }
        else
        {
            // 떨어지는 중: 추가 중력 적용
            if (!rb.isKinematic)
            {
                rb.AddForce(Vector3.down * extraGravityForce, ForceMode.Acceleration);
            }

            // 착지 확인 (속도가 느려졌을 때)
            if (IsGrounded() && rb.velocity.magnitude < 0.5f)
            {
                SwitchToFloating();
            }
        }
    }

    /// <summary>
    /// 떨어짐 -> 둥둥 뜨기로 전환
    /// </summary>
    void SwitchToFloating()
    {
        isFloating = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;  // 물리 끄기 (직접 이동)
        rb.useGravity = false;

        initialPosition = transform.position; // 현재 위치 고정
        groupingTrigger.enabled = true;       // 합치기 켜기
    }

    /// <summary>
    /// 둥둥 뜨기 -> 떨어짐으로 전환
    /// </summary>
    void SwitchToFalling()
    {
        isFloating = false;
        rb.isKinematic = false; // 물리 켜기
        rb.useGravity = true;
        groupingTrigger.enabled = false;
    }

    // 아이템 합치기 로직
    private void OnTriggerStay(Collider other)
    {
        if (isMerging || !isFloating || isPickedUp) return;

        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop otherItem = other.GetComponent<ItemDrop>();
            // 같은 종류이고 아직 살아있는 아이템끼리 합침
            if (otherItem != null && !otherItem.isPickedUp && otherItem != this && otherItem.type == this.type)
            {
                // 한쪽으로 몰아주기 (ID가 큰 쪽이 흡수됨)
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
        otherItem.Combine(this.count);
        Destroy(this.gameObject);
    }

    public void Combine(int amount)
    {
        this.count += amount;
    }

    // 땅 감지 로직
    private bool IsGrounded()
    {
        float radius = physicsCollider != null ? physicsCollider.radius : 0.25f;
        float checkDist = 0.2f;

        // [중요] 떠 있을 때는 레이저를 더 길게 쏴서 덜컹거림 방지
        if (isFloating)
        {
            checkDist += baseFloatHeight + floatHeight + 0.1f;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, radius + checkDist))
        {
            if (hit.collider.CompareTag("Block")) return true;
        }
        return false;
    }
}