using UnityEngine;

/// <summary>
/// 보스 캐릭터 AI (VoxelEnemy 상속)
/// - 애니메이터 상태 머신을 제어하여 공격/피격/사망 모션을 재생합니다.
/// </summary>
public class BossEnemy : VoxelEnemy
{
    [Header("보스 설정")]
    public Animator animator;

    // 애니메이션 상태 이름 (Animator의 State Name과 일치해야 함)
    private readonly string STATE_IDLE = "idle";
    private readonly string STATE_RUN = "run"; // 또는 walk_forward
    private readonly string STATE_ATTACK1 = "attack1";
    private readonly string STATE_HIT = "hit_1";
    private readonly string STATE_DEATH = "death";

    // 애니메이션 파라미터 해시 (최적화)
    // private int hashSpeed; 
    // 여기서는 CrossFade로 직접 상태 전이를 하므로 파라미터 대신 상태 이름을 사용합니다.

    private bool isDead = false;

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();
        
        // 보스는 체력이 더 많음
        maxHp = 100;
        currentHp = maxHp;
        damage = 10;
        attackRange = 3.0f; // 공격 사거리도 조금 더 긺
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // 상태에 따른 애니메이션 제어
        // 주의: CrossFade는 매 프레임 호출하면 애니메이션이 처음으로 계속 리셋될 수 있으므로
        // 현재 재생 중인 상태를 체크하거나, 상태가 바뀔 때만 호출해야 합니다.
        // 여기서는 간단히 상태별로 반복 호출하되, 이미 해당 상태라면 무시하도록 처리하는 것이 좋습니다.
        // 하지만 AnimatorController 구조상 파라미터 제어가 정석입니다.
        // 사용자 요청 그래프를 보면 파라미터를 알 수 없으므로, CrossFadeInFixedTime을 사용하되
        // "현재 상태가 아닐 때만" 전이하도록 합니다.
        
        // 현재 상태 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        switch (currentState)
        {
            case State.Idle:
                if (!stateInfo.IsName(STATE_IDLE) && !IsAttacking(stateInfo) && !IsHurt(stateInfo))
                {
                    animator.CrossFadeInFixedTime(STATE_IDLE, 0.2f);
                }
                break;

            case State.Chase:
                if (!stateInfo.IsName(STATE_RUN) && !IsAttacking(stateInfo) && !IsHurt(stateInfo))
                {
                    animator.CrossFadeInFixedTime(STATE_RUN, 0.2f);
                }
                break;

            case State.Attack:
                // Attack() 메서드에서 트리거하므로 여기서는 처리하지 않음
                // 다만 공격 후 Idle로 돌아오는 로직은 Animator Transition이 처리하거나
                // base.Update()에서 currentState가 Idle/Chase로 바뀌면 위 로직이 처리함
                break;
        }
    }

    bool IsAttacking(AnimatorStateInfo info)
    {
        return info.IsName(STATE_ATTACK1) || info.IsName("attack2") || info.IsName("attack3") 
            || info.IsName("attack4_kick") || info.IsName("attack5_kick");
    }

    bool IsHurt(AnimatorStateInfo info)
    {
        return info.IsName(STATE_HIT) || info.IsName("hit_2");
    }

    protected override void Attack()
    {
        // 공격 쿨다운은 base.Update()에서 체크됨
        // 공격 애니메이션 실행
        if (animator != null)
        {
            animator.CrossFadeInFixedTime(STATE_ATTACK1, 0.1f);
        }

        // 데미지 적용
        base.Attack();
    }

    public override void TakeDamage(int damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);

        // 피격 애니메이션
        if (animator != null && currentHp > 0)
        {
            animator.CrossFadeInFixedTime(STATE_HIT, 0.1f);
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[Boss] {name} 사망!");
        
        // 맵 관리자에 사망 알림 (클리어 체크)
        if (NosieVoxelMap.Instance != null)
        {
            NosieVoxelMap.Instance.NotifyEnemyKilled();
        }

        if (animator != null)
        {
            animator.CrossFadeInFixedTime(STATE_DEATH, 0.1f);
        }

        // 바로 삭제하지 않고 애니메이션이 끝날 때까지 기다림 (Coroutine 사용)
        // VoxelEnemy는 바로 Destroy하므로 이를 덮어씀
        Destroy(gameObject, 5f); // 5초 뒤 삭제
        
        // 충돌체 제거 (시체 위로 지나가게)
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        if (agent != null) agent.enabled = false;
    }
}
