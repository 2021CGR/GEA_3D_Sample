// 파일 이름: Enemy.cs
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    // [수정됨] 이벤트가 "누가" 죽었는지 (GameObject) 정보를 전달하도록 변경합니다.
    public static event System.Action<GameObject> OnEnemyKilled;

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

    // [추가됨] 넉백 변수 (기능은 비어있지만 TakeDamage 짝을 맞추기 위해 둠)
    [Tooltip("플레이어의 공격에 맞았을 때 밀려나는 힘의 크기입니다.")]
    public float knockbackStrength = 0f; // 넉백을 안 쓰면 0으로 둠

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


    // Start() 함수는 기존과 동일
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

    /// <summary>
    /// [수정됨] 자식(Boss.cs)이 접근할 수 있도록 protected virtual로 변경
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
                else AttackPlayer();
                break;
            case EnemyState.Flee:
                if (dist >= fleeStopDistance) state = EnemyState.Idle;
                else FleeFromPlayer();
                break;
        }
    }

    protected abstract void AttackPlayer();

    /// <summary>
    /// [수정됨] Projectile.cs의 오류를 해결하기 위해 2개의 인자를 받도록 수정
    /// </summary>
    public void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        currentHP -= damage;
        UpdateHpUI();

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // (참고: CharacterController나 NavMeshAgent가 없으면 넉백 구현이 어렵습니다)
            // if (knockbackStrength > 0) { ... 넉백 로직 ... }
        }
    }

    /// <summary>
    /// [수정됨] 죽을 때 GameObject 정보를 이벤트로 보냅니다.
    /// </summary>
    protected virtual void Die()
    {
        // StageClearManager에게 "나(gameObject)"가 죽었다고 알림
        OnEnemyKilled?.Invoke(gameObject);

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

    // (transform.position을 직접 제어하는 기본 추적 함수)
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