using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 캐릭터 AI:
/// - 평소에는 경계(Idle/Patrol) 상태
/// - 플레이어가 범위 내에 들어오면 추적(Chase)
/// - 공격 범위 내면 공격(Attack)
/// - 멀어지면 다시 경계 상태로 복귀
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class VoxelEnemy : MonoBehaviour
{
    public enum State { Idle, Chase, Attack }
    
    [Header("현재 상태")]
    public State currentState = State.Idle;

    [Header("능력치")]
    public int maxHp = 20;
    public int currentHp;
    public int damage = 5;

    [Header("AI 설정")]
    public float detectRange = 10f;   // 감지 범위
    public float attackRange = 2.0f;  // 공격 범위
    public float moveSpeed = 3.5f;    // 이동 속도
    public float attackCooldown = 1.5f; // 공격 간격
    public bool useNavMesh = false;
    public float groundSnapHeight = 2f;
    public float groundOffset = 0.05f;

    protected NavMeshAgent agent;
    protected Transform target; // 플레이어
    protected float lastAttackTime;
    protected Vector3 startPos; // 처음 위치 (복귀용)

    protected virtual void Start()
    {
        currentHp = maxHp;
        startPos = transform.position;
        
        // 맵 관리자에 적 등록
        if (NosieVoxelMap.Instance != null)
        {
            NosieVoxelMap.Instance.RegisterEnemy();
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange - 0.5f; // 공격 범위보다 조금 앞에서 멈춤
            agent.enabled = useNavMesh;
        }

        // NavMesh를 안 쓸 때는 물리 엔진의 중력 간섭을 끄고 직접 제어합니다.
        var rb = GetComponent<Rigidbody>();
        if (rb != null && !useNavMesh)
        {
            rb.isKinematic = true; // 물리 낙하 방지
            rb.useGravity = false;
        }
    }

    protected virtual void Update()
    {
        // 1. 위치 검증 (너무 높거나 낮으면 삭제 후 리스폰)
        // 지하 깊이가 늘어났으므로 하한선을 더 낮춤 (-bedrockDepth - 10)
        // NosieVoxelMap이 없거나 싱글톤 접근이 안 될 수도 있으니 안전값(-50) 사용
        if (transform.position.y < -50f || transform.position.y > 50f)
        {
            Debug.Log($"[Enemy] 위치 오류({transform.position})로 삭제 및 리스폰 요청");
            if (NosieVoxelMap.Instance != null)
            {
                // 버그로 인한 삭제이므로 Kill이 아닌 Unregister 처리
                NosieVoxelMap.Instance.UnregisterEnemy();
                NosieVoxelMap.Instance.RespawnEnemy();
            }
            Destroy(gameObject);
            return;
        }

        // 타겟(플레이어)이 없으면 찾기
        if (target == null)
        {
            var p = FindObjectOfType<PlayerController>();
            if (p != null) target = p.transform;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case State.Idle:
                // 플레이어가 감지 범위 안에 들어오면 추적 시작
                if (distance <= detectRange)
                {
                    currentState = State.Chase;
                    Debug.Log("[Enemy] 플레이어 발견! 추적 시작");
                }
                break;

            case State.Chase:
                // 플레이어가 너무 멀어지면 포기하고 복귀(또는 경계)
                if (distance > detectRange * 1.5f)
                {
                    currentState = State.Idle;
                    if (useNavMesh && agent != null) agent.SetDestination(startPos); // 원래 위치로 복귀 (선택 사항)
                    Debug.Log("[Enemy] 플레이어 놓침. 경계 상태 복귀");
                }
                // 공격 사거리 진입
                else if (distance <= attackRange)
                {
                    currentState = State.Attack;
                    if (useNavMesh && agent != null) agent.ResetPath(); // 이동 멈춤
                }
                else
                {
                    // 계속 추적
                    if (useNavMesh && agent != null)
                        agent.SetDestination(target.position);
                    else
                        MoveTowards(target.position);
                }
                break;

            case State.Attack:
                // 플레이어가 공격 사거리를 벗어나면 다시 추적
                if (distance > attackRange)
                {
                    currentState = State.Chase;
                }
                else
                {
                    // 플레이어 바라보기
                    Vector3 dir = target.position - transform.position;
                    dir.y = 0;
                    if (dir != Vector3.zero) 
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

                    // 쿨다운 체크 후 공격
                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        Attack();
                        lastAttackTime = Time.time;
                    }
                }
                break;
        }
        if (!useNavMesh) SnapToGround();
    }

    protected virtual void Attack()
    {
        Debug.Log($"[Enemy] {name} 공격!");
        // 플레이어에게 데미지 전달
        var player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"[Enemy] {name} 피격! 데미지: {damage}, 남은 HP: {currentHp}");

        // 맞으면 바로 추적 상태로 전환 (반격)
        if (currentState == State.Idle)
        {
            currentState = State.Chase;
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"[Enemy] {name} 처치됨!");
        
        // 맵 관리자에 사망 알림 (클리어 체크)
        if (NosieVoxelMap.Instance != null)
        {
            NosieVoxelMap.Instance.NotifyEnemyKilled();
        }

        Destroy(gameObject);
    }
    
    // 에디터에서 범위 확인용 기즈모
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 pos = transform.position;
        Vector3 dir = (targetPos - pos);
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist > 0.001f)
        {
            dir /= dist;
            transform.position = pos + dir * (moveSpeed * Time.deltaTime);
        }
    }

    void SnapToGround()
    {
        // SphereCast: 발바닥 면적만큼 굵은 레이를 쏴서 블록 틈새로 빠지는 것을 방지합니다.
        Vector3 origin = transform.position + Vector3.up * 1.5f; // 몸통 높이에서 시작
        float radius = 0.3f; // 적당한 두께
        
        // 아래로 SphereCast
        // QueryTriggerInteraction.Ignore: 트리거(감지범위 등)는 무시하고 실제 콜라이더(블록)만 감지
        if (Physics.SphereCast(origin, radius, Vector3.down, out var hit, 5.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // 너무 높은 벽(1.1m 이상)은 오르지 못하게 막거나 무시
            if (hit.point.y > transform.position.y + 1.2f) return;

            // 목표 위치: 바닥 높이 + 오프셋
            Vector3 targetPos = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
            
            // "점프 높이 줄여줘": 급격히 튀어오르지 않도록 오르막일 때는 천천히 보간
            float diff = targetPos.y - transform.position.y;
            float lerpSpeed = (diff > 0) ? 5f : 15f; // 올라갈 땐 부드럽게(5), 내려갈 땐 빠르게(15) 착지

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
        }
        else
        {
            // 바닥을 못 찾았을 경우(공중/틈새): 
            // "밑으로 쭉 내려가는" 현상을 막기 위해 Y축 이동을 멈추거나 현재 높이 유지
            // (Rigidbody가 Kinematic이므로 가만히 두면 높이가 유지됩니다)
        }
    }
}
