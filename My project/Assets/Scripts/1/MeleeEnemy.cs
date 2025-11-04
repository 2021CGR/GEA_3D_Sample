using UnityEngine;

// 'Enemy' 클래스의 모든 기능을 물려받습니다.
public class MeleeEnemy : Enemy
{
    [Header("근접 공격 설정")]
    [Tooltip("근접 공격이 플레이어에게 입힐 데미지입니다.")]
    public int meleeDamage = 1;


    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 변수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Header("디버프 설정 (인스펙터에서 설정)")]
    [Tooltip("적용할 디버프의 종류를 선택합니다. (None으로 두면 적용 안 함)")]
    public DebuffType debuffType = DebuffType.Slow; // 기본값을 Slow로 설정

    [Tooltip("디버프의 지속 시간 (초)입니다.")]
    public float debuffDuration = 3f;

    [Tooltip("디버프의 강도 (둔화 배율 0.5, 초당 독 데미지 1 등)")]
    public float debuffMagnitude = 0.5f;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


    // 부모의 AttackPlayer()를 근접 공격 방식으로 재정의합니다.
    protected override void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            transform.LookAt(player.position);

            // 플레이어가 실제로 공격 범위 내에 있는지 다시 확인합니다.
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                // player 변수에서 PlayerController 스크립트를 찾습니다.
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // 1. PlayerController의 TakeDamage 함수를 호출하여 데미지를 줍니다.
                    playerController.TakeDamage(meleeDamage);
                }


                // 2. 플레이어 오브젝트에서 StatusEffectManager를 찾아 디버프를 적용합니다.
                StatusEffectManager effectManager = player.GetComponent<StatusEffectManager>();

                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                // 디버프 타입이 None이 아닐 때만 디버프를 적용합니다.
                if (effectManager != null && debuffType != DebuffType.None)
                {
                    // 인스펙터에서 설정된 값 3개를 그대로 전달합니다.
                    effectManager.ApplyDebuff(
                        debuffType,     // 인스펙터에서 선택한 디버프
                        debuffDuration, // 인스펙터에서 설정된 지속 시간
                        debuffMagnitude // 인스펙터에서 설정된 강도
                    );
                }
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
            }
        }
    }
}