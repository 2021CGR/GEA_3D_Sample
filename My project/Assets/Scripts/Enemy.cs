// 파일 이름: Enemy.cs
using UnityEngine;
using UnityEngine.UI;

// abstract(추상) 클래스는 다른 클래스가 상속하기 위한 "설계도" 역할을 합니다.
public abstract class Enemy : MonoBehaviour
{
    // --- 이벤트 선언 (StageClearManager가 이 신호를 사용합니다) ---
    public static event System.Action OnEnemyKilled;

    // --- 적의 상태 정의 (공통) ---
    public enum EnemyState { Idle, Trace, Attack, Flee }
    [Header("AI 상태")]
    public EnemyState state = EnemyState.Idle;

    // --- AI 행동 설정 (공통) ---
    [Header("AI 행동 설정")]
    public float moveSpeed = 2f;
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    // --- 체력 및 도망 설정 (공통) ---
    [Header("체력 및 도망 설정")]
    public int maxHP = 5;
    [Range(0.05f, 0.5f)]
    public float fleeHpThresholdRatio = 0.2f;
    public float fleeStopDistance = 20f;
    public float fleeSpeedMultiplier = 1.5f;

    // --- UI 설정 (공통) ---
    [Header("UI 설정")]
    public Slider hpSlider;
    public bool initHpSliderOnStart = true;

    // --- 내부 변수 (protected는 자식 클래스에서 접근 가능) ---
    protected Transform player;
    protected float lastAttackTime;
    protected int currentHP;

    // Start, Update 등 대부분의 로직은 공통이므로 그대로 사용합니다.
    void Start()
    {
        // PlayerController.cs가 있으므로 "Player" 태그를 가진 오브젝트를 찾습니다.
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

    /// <summary>
    /// [수정됨] void Update() -> protected virtual void Update()로 변경
    /// protected: 자식 클래스(Boss.cs)가 접근할 수 있도록 허용
    /// virtual: 자식 클래스가 이 함수를 재정의(override)할 수 있도록 허용
    /// </summary>
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
                else AttackPlayer(); // 공격! 실제 내용은 자식 클래스가 채웁니다.
                break;
            case EnemyState.Flee:
                if (dist >= fleeStopDistance) state = EnemyState.Idle;
                else FleeFromPlayer();
                break;
        }
    }

    // ★★★ 핵심: 공격 메서드를 추상(abstract)으로 선언 ★★★
    // 자식 클래스(RangedEnemy, MeleeEnemy)는 반드시 이 메서드를 자기만의 방식으로 구현해야 합니다.
    protected abstract void AttackPlayer();

    // TakeDamage 메서드는 Projectile.cs에서 호출하므로 반드시 public이어야 합니다.
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        UpdateHpUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        OnEnemyKilled?.Invoke(); // StageClearManager에 신호 보내기
        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    // --- 이하 공통 행동 함수들 ---
    private void UpdateHpUI()
    {
        if (hpSlider == null) return;
        hpSlider.value = currentHP;
    }

    void TracePlayer()
    {
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