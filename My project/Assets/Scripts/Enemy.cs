using System.Collections;                               // ✔ 기본 컬렉션 네임스페이스
using System.Collections.Generic;                       // ✔ 제네릭 컬렉션
using UnityEngine;                                      // ✔ 유니티 엔진 API
using UnityEngine.UI;                                   // ✔ Slider 사용을 위한 네임스페이스

public class Enemy : MonoBehaviour
{
    // 🔹 기존: Idle/Trace/Attack
    // 🔹 추가: Flee (HP 20% 이하일 때 플레이어에게서 멀어지도록 도망)
    public enum EnemyState { Idle, Trace, Attack, Flee } // ✔ 상태 정의

    public EnemyState state = EnemyState.Idle;           // ✔ 현재 상태

    public float moveSpeed = 2f;                         // ✔ 이동 속도
    public float traceRange = 15f;                       // ✔ 추적 범위
    public float attackRange = 6f;                       // ✔ 공격 범위
    public float attackCooldown = 1.5f;                  // ✔ 공격 쿨타임

    public GameObject prohectilePrefab;                  // ✔ 발사체 프리팹 (원문 오타 유지)
    public Transform firePoint;                          // ✔ 발사 위치

    private Transform player;                            // ✔ 플레이어 위치
    private float lastAttackTime;                        // ✔ 마지막 공격 시간

    public int maxHP = 5;                                // ✔ 최대 체력
    private int currentHP;                               // ✔ 현재 체력

    // =========================
    // 🔻 HP UI (추가 필드) — 이제 "수동 할당"만 사용
    // =========================
    [Header("HP UI (Added)")]
    [Tooltip("이 적의 체력을 표시할 Slider UI (씬 UI 또는 월드 스페이스 모두 OK)")]
    public Slider hpSlider;                              // ✔ 인스펙터/스폰러에서 직접 할당하세요

    [Tooltip("Start에서 슬라이더의 max/value를 현재 HP로 자동 초기화할지 여부")]
    public bool initHpSliderOnStart = true;              // ✔ 자동 초기화 스위치

    // =========================
    // 🔻 도망 관련 옵션
    // =========================
    [Tooltip("HP가 20% 이하일 때 Flee 진입")]
    [Range(0.05f, 0.5f)]
    public float fleeHpThresholdRatio = 0.2f;            // ✔ 20% 임계비율

    [Tooltip("이 거리 이상 멀어지면 Idle로 전환")]
    public float fleeStopDistance = 20f;                 // ✔ 도망 종료 거리

    [Tooltip("도망칠 때 속도 배수 (기본 1.5배)")]
    public float fleeSpeedMultiplier = 1.5f;             // ✔ 도망 속도 배수

    // =========================
    // 🔻 HP UI 갱신 전용 함수 — 표시만 담당
    // =========================
    private void UpdateHpUI()
    {
        if (hpSlider == null) return;                    // 슬라이더 없으면 무시
        hpSlider.value = currentHP;                      // 현재 HP 값을 반영
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;                             // 데미지 적용
        UpdateHpUI();                                    // HP 변화 즉시 반영

        if (currentHP <= 0)                              // 사망 체크
        {
            Die();
        }
    }

    void Die()
    {
        // 죽을 때 HP바를 숨기고 싶다면 비활성화
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);

        Destroy(gameObject);                             // 오브젝트 제거
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // 플레이어 참조
        lastAttackTime = -attackCooldown;                // 시작 시 바로 공격 가능
        currentHP = maxHP;                               // 현재 체력 초기화

        // 슬라이더 초기 세팅(선택)
        if (hpSlider != null && initHpSliderOnStart)
        {
            hpSlider.minValue = 0;                       // 최소값
            hpSlider.maxValue = maxHP;                   // 최대값
            hpSlider.wholeNumbers = true;                // 정수 단계(선택)
            hpSlider.value = currentHP;                  // 시작값
        }
    }

    void Awake()
    {
        // ❌ 자동 탐색 제거: 이제 수동으로만 할당합니다.
        // (원래 있던) if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
    }

    void Update()
    {
        if (player == null) return;                      // 플레이어 없으면 처리 안 함

        float dist = Vector3.Distance(transform.position, player.position); // 거리 계산

        // HP 20% 이하 → 우선 Flee 진입
        if (currentHP <= maxHP * fleeHpThresholdRatio && state != EnemyState.Flee)
        {
            state = EnemyState.Flee;                     // 저체력 → 도망
        }

        switch (state)
        {
            case EnemyState.Idle:
                if (dist < traceRange)
                    state = EnemyState.Trace;
                break;

            case EnemyState.Trace:
                if (dist < attackRange)
                    state = EnemyState.Attack;
                else if (dist > traceRange)
                    state = EnemyState.Idle;
                else
                    TracePlayer();
                break;

            case EnemyState.Attack:
                if (dist > attackRange)
                    state = EnemyState.Trace;
                else
                    AttackPlayer();
                break;

            case EnemyState.Flee:
                if (dist >= fleeStopDistance)
                {
                    state = EnemyState.Idle;
                }
                else
                {
                    FleeFromPlayer();
                }
                break;
        }

        // -------------------------
        // 로컬 함수들 (원본 유지)
        // -------------------------
        void TracePlayer()
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.LookAt(player.position);
        }

        void AttackPlayer()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    lastAttackTime = Time.time;
                    ShootProjectile();
                }
            }
        }

        void ShootProjectile()
        {
            if (prohectilePrefab != null && firePoint != null)
            {
                transform.LookAt(player.position);
                GameObject proj = Instantiate(prohectilePrefab, firePoint.position, Quaternion.identity);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep != null)
                {
                    Vector3 dir = (player.position - firePoint.position).normalized;
                    ep.SetDirection(dir);
                }
            }
        }

        void FleeFromPlayer()
        {
            Vector3 away = (transform.position - player.position).normalized;
            float fleeSpeed = moveSpeed * Mathf.Max(1f, fleeSpeedMultiplier);
            transform.position += away * fleeSpeed * Time.deltaTime;
            transform.LookAt(transform.position + away);
        }
    }
}
