// 파일 이름: MeleeEnemy.cs
using UnityEngine;

// 'Enemy' 클래스의 모든 기능을 물려받습니다.
public class MeleeEnemy : Enemy
{
    [Header("근접 공격 설정")]
    [Tooltip("근접 공격이 플레이어에게 입힐 데미지입니다.")]
    public int meleeDamage = 1;

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
                    // PlayerController의 TakeDamage 함수를 호출하여 데미지를 줍니다.
                    playerController.TakeDamage(meleeDamage);
                }
            }
        }
    }
}