using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [수정] 둥둥 뜸 -> 떨어짐 상태로 전환될 때
/// 속도를 강제로 0으로 리셋하여 튕김 현상을 수정합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ItemDrop : MonoBehaviour
{
    public BlockType type;
    public int count = 1;

    [Header("Floating Behavior")]
    public float floatHeight = 0.1f;
    public float baseFloatHeight = 0.1f;
    public float floatSpeed = 1f;
    public float rotateSpeed = 50f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.4f;

    [Header("Grouping")]
    public float groupingRadius = 1.0f;

    [Header("Falling")]
    public float extraGravityForce = 10f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private bool isFloating = false;
    private bool isMerging = false;
    private SphereCollider groupingTrigger;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<BoxCollider>().isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        groupingTrigger = gameObject.AddComponent<SphereCollider>();
        groupingTrigger.isTrigger = true;
        groupingTrigger.radius = groupingRadius;
        groupingTrigger.enabled = false;
    }

    /// <summary>
    /// 물리 상태 및 둥둥 뜨는 위치 이동을 담당합니다.
    /// </summary>
    void FixedUpdate()
    {
        if (isFloating)
        {
            // --- 1. "둥둥 뜨는" 상태일 때 ---

            if (IsGrounded())
            {
                // [A] 땅이 있음: 둥둥 뜨는 위치 이동 (rb.MovePosition)
                float yBob = ((Mathf.Sin(Time.time * floatSpeed) + 1f) / 2f) * floatHeight;
                float newY = initialPosition.y + baseFloatHeight + yBob;
                Vector3 newPosition = new Vector3(initialPosition.x, newY, initialPosition.z);
                rb.MovePosition(newPosition);
            }
            else
            {
                // [B] 땅이 없음: "떨어지는" 상태로 전환
                isFloating = false;
                rb.isKinematic = false;
                rb.useGravity = true;
                groupingTrigger.enabled = false;

                // --- [핵심 수정!] ---
                // Kinematic -> Dynamic으로 전환 시 발생하는
                // 튕김(Velocity Pop) 현상을 막기 위해 속도를 강제로 0으로 리셋합니다.
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // ---------------------
            }
        }
        else // (isFloating == false, 즉 "떨어지는" 상태일 때)
        {
            // --- 2. "떨어지는" 상태일 때 ---

            // [C] 추가 중력 적용
            if (!rb.isKinematic)
            {
                rb.AddForce(Vector3.down * extraGravityForce, ForceMode.Acceleration);
            }

            // [D] 땅에 닿고 안정화되었는지 확인
            if (IsGrounded() && Mathf.Abs(rb.velocity.y) < 0.1f)
            {
                // [E] "둥둥 뜨는" 상태로 전환
                isFloating = true;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
                initialPosition = rb.position;
                groupingTrigger.enabled = true;
            }
        }
    }

    // (Update 함수는 변경 없음 - 회전만 담당)
    /// <CodeOmitted /> 
    void Update()
    {
        if (isFloating)
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    // (OnTriggerStay, MergeInto, Combine 함수는 변경 없음)
    /// <CodeOmitted /> 
    private void OnTriggerStay(Collider other)
    {
        if (isMerging || !isFloating) return;
        if (other.CompareTag("ItemDrop"))
        {
            ItemDrop otherItem = other.GetComponent<ItemDrop>();
            if (otherItem != null && otherItem != this && otherItem.type == this.type)
            {
                if (otherItem.GetInstanceID() > this.GetInstanceID())
                {
                    MergeInto(otherItem);
                }
            }
        }
    }
    /// <CodeOmitted /> 
    void MergeInto(ItemDrop otherItem)
    {
        isMerging = true;
        otherItem.Combine(this.count);
        Destroy(this.gameObject);
    }
    /// <CodeOmitted /> 
    public void Combine(int amount)
    {
        this.count += amount;
    }

    // (IsGrounded 함수는 변경 없음)
    /// <CodeOmitted /> 
    private bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance))
        {
            if (hit.collider.CompareTag("Block"))
            {
                return true;
            }
        }
        return false;
    }
}