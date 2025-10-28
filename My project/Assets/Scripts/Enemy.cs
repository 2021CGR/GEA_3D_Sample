// ���� �̸�: Enemy.cs
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    // [������] �̺�Ʈ�� "����" �׾����� (GameObject) ������ �����ϵ��� �����մϴ�.
    public static event System.Action<GameObject> OnEnemyKilled;

    // --- ���� ���� ���� (����) ---
    public enum EnemyState { Idle, Trace, Attack, Flee }
    [Header("AI ����")]
    public EnemyState state = EnemyState.Idle;

    // --- AI �ൿ ���� (����) ---
    [Header("AI �ൿ ����")]
    public float moveSpeed = 2f;
    public float traceRange = 15f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;

    // [�߰���] �˹� ���� (����� ��������� TakeDamage ¦�� ���߱� ���� ��)
    [Tooltip("�÷��̾��� ���ݿ� �¾��� �� �з����� ���� ũ���Դϴ�.")]
    public float knockbackStrength = 0f; // �˹��� �� ���� 0���� ��

    // --- ü�� �� ���� ���� (����) ---
    [Header("ü�� �� ���� ����")]
    public int maxHP = 5;
    [Range(0.05f, 0.5f)]
    public float fleeHpThresholdRatio = 0.2f;
    public float fleeStopDistance = 20f;
    public float fleeSpeedMultiplier = 1.5f;

    // --- UI ���� (����) ---
    [Header("UI ����")]
    public Slider hpSlider;
    public bool initHpSliderOnStart = true;

    // --- ���� ���� (protected�� �ڽ� Ŭ�������� ���� ����) ---
    protected Transform player;
    protected float lastAttackTime;
    protected int currentHP;


    // Start() �Լ��� ������ ����
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
    /// [������] �ڽ�(Boss.cs)�� ������ �� �ֵ��� protected virtual�� ����
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
    /// [������] Projectile.cs�� ������ �ذ��ϱ� ���� 2���� ���ڸ� �޵��� ����
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
            // (����: CharacterController�� NavMeshAgent�� ������ �˹� ������ ��ƽ��ϴ�)
            // if (knockbackStrength > 0) { ... �˹� ���� ... }
        }
    }

    /// <summary>
    /// [������] ���� �� GameObject ������ �̺�Ʈ�� �����ϴ�.
    /// </summary>
    protected virtual void Die()
    {
        // StageClearManager���� "��(gameObject)"�� �׾��ٰ� �˸�
        OnEnemyKilled?.Invoke(gameObject);

        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);

        Destroy(gameObject);
    }

    // --- ���� ���� �ൿ �Լ��� ---
    private void UpdateHpUI()
    {
        if (hpSlider == null) return;
        hpSlider.value = currentHP;
    }

    // (transform.position�� ���� �����ϴ� �⺻ ���� �Լ�)
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