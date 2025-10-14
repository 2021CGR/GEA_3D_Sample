// 파일 이름: Enemy.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Slider 같은 UI 요소를 코드에서 사용하기 위해 필요합니다.

public class Enemy : MonoBehaviour
{
    // --- 이벤트 선언 ---
    /// <summary>
    /// 적이 죽었을 때 게임의 다른 부분(예: StageClearManager)에 알리기 위한 신호(이벤트)입니다.
    /// static으로 선언되어 있어, 어떤 Enemy 스크립트에서든 이 신호를 보낼 수 있습니다.
    /// </summary>
    public static event System.Action OnEnemyKilled;


    // --- 적의 상태 정의 ---
    /// <summary>
    /// 적이 가질 수 있는 행동 상태를 정의합니다.
    /// Idle: 대기, Trace: 추적, Attack: 공격, Flee: 도망
    /// </summary>
    public enum EnemyState { Idle, Trace, Attack, Flee }

    [Header("AI 상태")]
    [Tooltip("적의 현재 행동 상태를 보여줍니다.")]
    public EnemyState state = EnemyState.Idle; // 적의 현재 상태를 저장하는 변수, 시작은 Idle


    // --- AI 행동 설정 ---
    [Header("AI 행동 설정")]
    [Tooltip("적의 평소 이동 속도입니다.")]
    public float moveSpeed = 2f;

    [Tooltip("이 거리 안으로 플레이어가 들어오면 추적을 시작합니다.")]
    public float traceRange = 15f;

    [Tooltip("이 거리 안으로 플레이어가 들어오면 공격을 시작합니다.")]
    public float attackRange = 6f;

    [Tooltip("한 번 공격한 후 다음 공격까지의 대기 시간(초)입니다.")]
    public float attackCooldown = 1.5f;


    // --- 공격 관련 설정 ---
    [Header("공격 설정")]
    [Tooltip("발사할 총알 프리팹을 연결해주세요.")]
    public GameObject prohectilePrefab; // 원본 코드의 오타(prohectile)를 유지했습니다.

    [Tooltip("총알이 발사될 위치입니다. (보통 총구에 빈 오브젝트를 만들어 연결합니다)")]
    public Transform firePoint;


    // --- 체력 및 도망 설정 ---
    [Header("체력 및 도망 설정")]
    [Tooltip("적의 최대 체력입니다.")]
    public int maxHP = 5;

    [Tooltip("체력이 이 비율(%) 이하로 떨어지면 도망가기 시작합니다. (0.2 = 20%)")]
    [Range(0.05f, 0.5f)] // 인스펙터에서 슬라이더로 편하게 조절할 수 있게 합니다.
    public float fleeHpThresholdRatio = 0.2f;

    [Tooltip("도망갈 때, 플레이어와 이 거리 이상 멀어지면 추적을 멈추고 대기 상태로 돌아갑니다.")]
    public float fleeStopDistance = 20f;

    [Tooltip("도망갈 때의 속도 배율입니다. (1.5 = 평소 속도의 1.5배)")]
    public float fleeSpeedMultiplier = 1.5f;


    // --- UI 설정 ---
    [Header("UI 설정")]
    [Tooltip("적의 체력을 표시할 UI Slider를 연결해주세요.")]
    public Slider hpSlider;

    [Tooltip("true로 설정하면 게임 시작 시 슬라이더의 최대값을 maxHP에 맞춰 자동으로 설정합니다.")]
    public bool initHpSliderOnStart = true;


    // --- 내부 변수 (Private) ---
    private Transform player;       // 플레이어의 위치 정보를 저장하기 위한 변수
    private float lastAttackTime;   // 마지막으로 공격한 시간을 기록하는 변수
    private int currentHP;          // 적의 현재 체력을 저장하는 변수


    /// <summary>
    /// 게임 오브젝트가 처음 생성될 때 한 번 호출되는 함수입니다. (초기화 담당)
    /// </summary>
    void Start()
    {
        // "Player"라는 태그를 가진 게임 오브젝트를 씬에서 찾아 그 위치 정보를 player 변수에 저장합니다.
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 게임이 시작되자마자 바로 공격할 수 있도록 마지막 공격 시간을 초기화합니다.
        lastAttackTime = -attackCooldown;

        // 현재 체력을 최대 체력과 같게 설정합니다.
        currentHP = maxHP;

        // hpSlider가 연결되어 있고, 자동 초기화 옵션이 켜져 있다면
        if (hpSlider != null && initHpSliderOnStart)
        {
            hpSlider.minValue = 0;       // 슬라이더의 최소값을 0으로
            hpSlider.maxValue = maxHP;   // 슬라이더의 최대값을 적의 최대 체력으로
            hpSlider.value = currentHP;  // 슬라이더의 현재 값을 현재 체력으로 맞춰줍니다.
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되는 함수입니다. (적의 상태를 계속 확인하고 행동을 결정)
    /// </summary>
    void Update()
    {
        // 플레이어를 찾지 못했다면 아무것도 하지 않고 함수를 종료합니다. (오류 방지)
        if (player == null) return;

        // 적과 플레이어 사이의 거리를 계산합니다.
        float dist = Vector3.Distance(transform.position, player.position);

        // --- 상태 결정 로직 ---
        // 1. 체력이 도망갈 수준인가? (가장 먼저 확인)
        // 현재 체력이 (최대체력 * 도망 기준 비율)보다 낮고, 아직 도망 상태가 아니라면
        if (currentHP <= maxHP * fleeHpThresholdRatio && state != EnemyState.Flee)
        {
            state = EnemyState.Flee; // 상태를 '도망'으로 변경
        }

        // 2. 현재 상태에 따라 다른 행동을 하도록 분기합니다. (State Machine)
        switch (state)
        {
            case EnemyState.Idle: // 대기 상태일 때
                // 플레이어가 추적 범위 안으로 들어왔다면
                if (dist < traceRange)
                    state = EnemyState.Trace; // '추적' 상태로 변경
                break;

            case EnemyState.Trace: // 추적 상태일 때
                if (dist < attackRange) // 플레이어가 공격 범위 안으로 들어왔다면
                    state = EnemyState.Attack; // '공격' 상태로 변경
                else if (dist > traceRange) // 플레이어가 추적 범위를 벗어났다면
                    state = EnemyState.Idle; // '대기' 상태로 변경
                else // 그 외의 경우 (추적 범위 안에 있지만 공격 범위 밖일 때)
                    TracePlayer(); // 플레이어를 계속 추적합니다.
                break;

            case EnemyState.Attack: // 공격 상태일 때
                if (dist > attackRange) // 플레이어가 공격 범위를 벗어났다면
                    state = EnemyState.Trace; // '추적' 상태로 변경
                else // 공격 범위 안에 있다면
                    AttackPlayer(); // 플레이어를 공격합니다.
                break;

            case EnemyState.Flee: // 도망 상태일 때
                if (dist >= fleeStopDistance) // 플레이어와 충분히 멀어졌다면
                    state = EnemyState.Idle; // '대기' 상태로 변경
                else // 아직 충분히 멀어지지 않았다면
                    FleeFromPlayer(); // 계속 도망갑니다.
                break;
        }
    }

    /// <summary>
    /// 외부(주로 총알)로부터 데미지를 받는 함수입니다.
    /// </summary>
    /// <param name="damage">입을 데미지의 양</param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage; // 현재 체력에서 데미지만큼 뺍니다.
        UpdateHpUI();        // 체력 바 UI를 갱신합니다.

        // 만약 체력이 0 이하가 되었다면
        if (currentHP <= 0)
        {
            Die(); // 죽음 처리 함수를 호출합니다.
        }
    }

    /// <summary>
    /// 적이 죽었을 때 처리할 내용을 담은 함수입니다.
    /// </summary>
    void Die()
    {
        // ★★★ 중요: OnEnemyKilled 이벤트를 구독하는 모든 곳에 "적이 죽었다"는 신호를 보냅니다.
        OnEnemyKilled?.Invoke(); // '?'는 구독자가 아무도 없을 때 오류가 나지 않도록 해줍니다.

        // 만약 HP 슬라이더가 연결되어 있다면, 죽을 때 함께 보이지 않도록 비활성화합니다.
        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);

        // 이 적 게임 오브젝트를 씬에서 제거합니다.
        Destroy(gameObject);
    }

    /// <summary>
    /// 체력 바 UI의 값을 현재 체력에 맞게 업데이트하는 함수입니다.
    /// </summary>
    private void UpdateHpUI()
    {
        // hpSlider가 연결되어 있지 않으면 아무것도 하지 않습니다.
        if (hpSlider == null) return;

        // 슬라이더의 값을 현재 체력으로 설정합니다.
        hpSlider.value = currentHP;
    }


    // --- 상태별 실제 행동 함수들 ---

    /// <summary>
    /// 플레이어를 향해 이동하고 플레이어를 바라보게 합니다.
    /// </summary>
    void TracePlayer()
    {
        // 플레이어 방향으로의 방향 벡터를 구합니다. (크기는 1로 정규화)
        Vector3 dir = (player.position - transform.position).normalized;
        // 계산된 방향으로 이동 속도에 맞춰 이동합니다.
        transform.position += dir * moveSpeed * Time.deltaTime;
        // 적이 항상 플레이어를 바라보게 합니다.
        transform.LookAt(player.position);
    }

    /// <summary>
    /// 공격 쿨타임을 확인하고 총알을 발사합니다.
    /// </summary>
    void AttackPlayer()
    {
        // 현재 게임 시간이 (마지막 공격 시간 + 쿨타임)보다 크거나 같아졌다면 (공격할 준비가 되었다면)
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time; // 마지막 공격 시간을 현재 시간으로 기록합니다.
            ShootProjectile();          // 총알 발사 함수를 호출합니다.
        }
    }

    /// <summary>
    /// 설정된 총알 프리팹을 발사 위치에 생성합니다.
    /// </summary>
    void ShootProjectile()
    {
        // 총알 프리팹과 발사 위치가 모두 제대로 설정되었는지 확인합니다.
        if (prohectilePrefab != null && firePoint != null)
        {
            // 발사하기 직전에 플레이어를 정확히 조준합니다.
            transform.LookAt(player.position);

            // 총알 프리팹을 firePoint의 위치에, 기본 회전값으로 생성합니다.
            GameObject proj = Instantiate(prohectilePrefab, firePoint.position, Quaternion.identity);

            // 생성된 총알에서 EnemyProjectile 스크립트를 가져옵니다.
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();

            // 스크립트를 성공적으로 가져왔다면
            if (ep != null)
            {
                // 총알이 날아갈 방향을 계산하여 설정해줍니다.
                Vector3 dir = (player.position - firePoint.position).normalized;
                ep.SetDirection(dir);
            }
        }
    }

    /// <summary>
    /// 플레이어로부터 멀어지는 방향으로 빠르게 이동합니다.
    /// </summary>
    void FleeFromPlayer()
    {
        // 플레이어로부터 멀어지는 방향 벡터를 계산합니다.
        Vector3 awayDir = (transform.position - player.position).normalized;
        // 도망 속도를 계산합니다. (기본 속도 * 속도 배율)
        float fleeSpeed = moveSpeed * fleeSpeedMultiplier;
        // 계산된 방향과 속도로 이동합니다.
        transform.position += awayDir * fleeSpeed * Time.deltaTime;
        // 도망가는 방향을 바라보게 합니다.
        transform.LookAt(transform.position + awayDir);
    }
}