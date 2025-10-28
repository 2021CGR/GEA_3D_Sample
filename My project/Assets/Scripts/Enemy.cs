// 파일 이름: Enemy.cs
using UnityEngine;
using UnityEngine.UI;

// abstract(추상) 클래스는 다른 클래스가 상속하기 위한 "설계도" 역할을 합니다.
public abstract class Enemy : MonoBehaviour
{
    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    // 이벤트가 "누가" 죽었는지 (GameObject) 정보를 전달하도록 변경합니다.
    public static event System.Action<GameObject> OnEnemyKilled;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    // --- 적의 상태 정의 (공통) ---
    public enum EnemyState { Idle, Trace, Attack, Flee }
    [Header("AI 상태")]
    public EnemyState state = EnemyState.Idle;

    // ... (AI 행동, 체력, UI 설정 변수들은 모두 기존과 동일) ...
    [Header("AI 행동 설정")]
    public float moveSpeed = 2f;
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    [Header("체력 및 도망 설정")]
    public int maxHP = 5;
    [Range(0.05f, 0.5f)]
    public float fleeHpThresholdRatio = 0.2f;
    public float fleeStopDistance = 20f;
    public float fleeSpeedMultiplier = 1.5f;

    [Header("UI 설정")]
    public Slider hpSlider;
    public bool initHpSliderOnStart = true;

    // --- 내부 변수 (protected는 자식 클래스에서 접근 가능) ---
    protected Transform player;
    protected float lastAttackTime;
    protected int currentHP;

    // Start() 함수는 기존과 동일합니다.
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastAttackTime = -attackCooldown;
        currentHP = maxHP;

        if (hpSlider != null && initHpSliderOnStart)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    // Update() 함수는 기존과 동일합니다.
    protected virtual void Update()
    {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);

        if (currentHP <= maxHP * fleeHpThresholdRatio && state != EnemyState.Flee)
        {
            state = EnemyState.Flee;
        }

        switch (state)
        {
            case EnemyState.Idle:
                if (dist < traceRange) state = EnemyState.Trace;
                break;
            case EnemyState.Trace:
                if (dist < attackRange) state = EnemyState.Attack;
                else if (dist > traceRange) state = EnemyState.Idle;
                else TracePlayer();
                break;
            case EnemyState.Attack:
                if (dist > attackRange) state = EnemyState.Trace;
                else AttackPlayer();
                break;
            case EnemyState.Flee:
                if (dist >= fleeStopDistance) state = EnemyState.Idle;
                else FleeFromPlayer();
                break;
        }
    }

    protected abstract void AttackPlayer();

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        UpdateHpUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    // 1. 자식(Boss.cs)이 이 함수를 재정의(override)할 수 있도록 protected virtual로 변경
    // 2. Die() 함수가 private(기본값)이 아니어야 StageClearManager와 연동됩니다.
    protected virtual void Die()
    {
        // 2. 이벤트에 "나 자신"(gameObject)을 담아 보냅니다.
        OnEnemyKilled?.Invoke(gameObject);

        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);

        Destroy(gameObject);
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    // --- 이하 공통 행동 함수들 ---
    private void UpdateHpUI()
    {
        if (hpSlider == null) return;
        hpSlider.value = currentHP;
    }

    void TracePlayer()
    {
        // (이 코드는 NavMeshAgent를 사용하지 않는 이전 버전 기준입니다.
        //  만약 NavMeshAgent를 쓰고 계시다면 이 부분은 이미 agent.SetDestination으로 되어있을 것입니다.)
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(player.position);
    }

    void FleeFromPlayer()
    {
        Vector3 awayDir = (transform.position - player.position).normalized;
        float fleeSpeed = moveSpeed * fleeSpeedMultiplier;
        transform.position += awayDir * fleeSpeed * Time.deltaTime;
        transform.LookAt(transform.position + awayDir);
    }
}